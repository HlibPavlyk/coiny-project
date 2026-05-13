using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Payments;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Features.Lots;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == payment.LotId, ct);
            if (lot is null || lot.Status != LotStatus.Sold)
                continue;

            lot.Status = LotStatus.EndedNoSale;
            lot.UpdatedAt = now;

            db.OutboxEvents.Add(new OutboxEvent
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
