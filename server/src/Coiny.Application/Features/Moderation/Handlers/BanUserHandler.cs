using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Payments;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Realtime;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.Requests;
using Coiny.Application.Features.Lots.Events;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Application.Features.Moderation.Handlers;

/// <summary>
/// Bans a user and runs the auto-cleanup pipeline (per <c>/docs/06-open-questions.md</c> A5).
///
/// Two-stage by design: stage 1 commits all DB transitions in one transaction (flag the user, cancel
/// their Active lots, emit <c>LotEnded</c> outbox events, decrement category counts, snapshot the
/// in-flight payments); stage 2 runs the side effects after commit (SignalR notifications, Stripe
/// PaymentIntent cancels). A Stripe outage must never roll back the ban — on the success path Stripe
/// state reconciles via the <c>payment_intent.canceled</c> webhook. A failed cancel is logged and
/// counted; the handler returns 502 so the admin can re-run. No retry table is built (the retry-job
/// approach is DESCRIBE-ONLY for the thesis).
/// </summary>
public class BanUserHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IStripeClient stripe,
    IAuctionNotifier notifier,
    IDateTimeProvider clock,
    ILogger<BanUserHandler> logger)
    : IRequestHandler<BanUserRequest, Result>
{
    public async Task<Result> Handle(BanUserRequest request, CancellationToken ct)
    {
        if (!currentUser.CanModerate())
            return Result.Failure(Error.Forbidden("Admin.Forbidden", "Moderator or Administrator role required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        if (user.IsBanned)
            return Result.Failure(Error.Conflict("User.AlreadyBanned", "This user is already banned."));

        DateTime now = clock.UtcNow;
        var cancelledLotIds = new List<Guid>();
        List<Payment> inFlightPayments;

        // ── Stage 1: all DB transitions in one transaction ──────────────────
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            user.IsBanned = true;
            user.BannedAt = now;
            user.BannedByUserId = currentUser.UserId;
            user.BanReason = request.Reason;
            user.UpdatedAt = now;

            List<Lot> activeLots = await db.Lots
                .Where(l => l.SellerId == request.UserId && l.Status == LotStatus.Active)
                .ToListAsync(ct);

            foreach (Lot lot in activeLots)
            {
                lot.Status = LotStatus.Cancelled;
                lot.UpdatedAt = now;
                lot.AuctionCloseJobId = null; // the scheduled close job will no-op on the status guard

                db.SearchOutboxEvents.Add(new SearchOutboxEvent
                {
                    AggregateType = "Lot",
                    AggregateId = lot.Id,
                    EventType = LotEndedPayload.EventType,
                    Payload = new LotEndedPayload(lot.Id, lot.Status).Serialize(),
                    CreatedAt = now,
                });

                Category? category = await db.Categories.FirstOrDefaultAsync(c => c.Id == lot.CategoryId, ct);
                if (category is not null)
                    category.LotCountActive = Math.Max(category.LotCountActive - 1, 0);

                cancelledLotIds.Add(lot.Id);
            }

            inFlightPayments = await db.Payments
                .Where(p => (p.BuyerId == request.UserId || p.SellerId == request.UserId)
                    && (p.Status == PaymentStatus.PendingAuthorization || p.Status == PaymentStatus.Authorized))
                .ToListAsync(ct);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        // ── Stage 2: side effects after commit ──────────────────────────────
        foreach (Guid lotId in cancelledLotIds)
        {
            try
            {
                await notifier.NotifyLotChangedAsync(lotId, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "BanUserHandler: notify failed for cancelled lot {LotId}", lotId);
            }
        }

        int stripeFailures = 0;
        foreach (Payment payment in inFlightPayments)
        {
            try
            {
                // Success path: Stripe emits payment_intent.canceled → the webhook handler flips
                // Payment.Status = Cancelled (and moves a seller-side lot to EndedNoSale). Idempotent.
                await stripe.CancelPaymentIntentAsync(payment.StripePaymentIntentId, "user_banned", ct);
            }
            catch (Exception ex)
            {
                stripeFailures++;
                logger.LogError(ex,
                    "BanUserHandler: Stripe cancel failed for payment {PaymentId} (PI {PaymentIntentId})",
                    payment.Id, payment.StripePaymentIntentId);
            }
        }

        if (stripeFailures > 0)
            return Result.Failure(Error.ExternalService(
                "Ban.StripeCancelFailed",
                $"Stripe-side cancel failed for {stripeFailures} payment(s); re-run the ban to retry."));

        return Result.Success();
    }
}
