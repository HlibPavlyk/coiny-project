using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Features.Lots;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Coiny.Application.Features.Lots.Events;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Daily Hangfire job that cancels every <see cref="PaymentStatus.PendingAuthorization"/> payment
/// past its 96h <see cref="Payment.DueAt"/>. Tells Stripe to cancel the intent (<c>"abandoned"</c>
/// reason); the <c>payment_intent.canceled</c> webhook is the source of truth for
/// <c>Payment.Status = Cancelled</c>. The linked Lot is flipped to <see cref="LotStatus.EndedNoSale"/>
/// in the same pass with a <c>LotEnded</c> outbox event so search-index sync can drop it.
///
/// Idempotent per row — already-cancelled payments skip the Stripe call; lots already in
/// terminal states aren't touched.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public class NonPaymentCancelJob(
    IApplicationDbContext db,
    IStripeClient stripe,
    IDateTimeProvider clock,
    ILogger<NonPaymentCancelJob> logger)
{
    private const int _batchSize = 100;

    public async Task RunAsync(CancellationToken ct)
    {
        DateTime now = clock.UtcNow;

        // The partial index IX_Payments_Status_DueAt (filter "Status = 'PendingAuthorization'")
        // covers this query exactly.
        List<Payment> expired = await db.Payments
            .Where(p => p.Status == PaymentStatus.PendingAuthorization && p.DueAt <= now)
            .OrderBy(p => p.DueAt)
            .Take(_batchSize)
            .ToListAsync(ct);

        if (expired.Count == 0)
            return;

        logger.LogInformation("NonPaymentCancelJob: cancelling {Count} expired payments", expired.Count);

        foreach (Payment payment in expired)
        {
            try
            {
                await stripe.CancelPaymentIntentAsync(payment.StripePaymentIntentId, "abandoned", ct);
            }
            catch (Exception ex)
            {
                // Don't kill the whole batch on one bad row; webhook reconciliation will catch up later.
                logger.LogWarning(ex,
                    "NonPaymentCancelJob: stripe.CancelPaymentIntentAsync failed for payment {PaymentId} (pi={Pi}) — skipping",
                    payment.Id, payment.StripePaymentIntentId);
                continue;
            }

            // Penalize the buyer for non-payment (THESIS-SCOPE.md §9: −10). Applied here rather
            // than in the payment_intent.canceled webhook because Stripe also fires that event
            // for legitimate buyer-initiated cancellations (and admin refunds), which should NOT
            // tank trust. Reaching this code path means the 96h window expired with the buyer
            // never authorizing — that's the trust-eroding signal.
            User? buyer = await db.Users.FirstOrDefaultAsync(u => u.Id == payment.BuyerId, ct);
            if (buyer is not null)
            {
                buyer.TrustScore -= 10;
                buyer.UpdatedAt = now;
            }
            else
            {
                logger.LogWarning(
                    "NonPaymentCancelJob: cannot debit trust score — buyer {BuyerId} for payment {PaymentId} not found",
                    payment.BuyerId, payment.Id);
            }

            Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == payment.LotId, ct);
            if (lot is null || lot.Status != LotStatus.Sold)
                continue;

            lot.Status = LotStatus.EndedNoSale;
            lot.UpdatedAt = now;

            db.SearchOutboxEvents.Add(new SearchOutboxEvent
            {
                AggregateType = nameof(Lot),
                AggregateId = lot.Id,
                EventType = LotEndedPayload.EventType,
                Payload = new LotEndedPayload(lot.Id, lot.Status).Serialize(),
                CreatedAt = now,
                AttemptCount = 0,
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
