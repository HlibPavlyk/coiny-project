using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Currency;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.CreatePaymentIntent;

/// <summary>
/// Mints (or surfaces) the Stripe PaymentIntent for a sold lot.
///
/// <para>Lifecycle context: <c>AuctionCloseJob</c> pre-creates a Payment row at close-time with a
/// null <see cref="Payment.StripePaymentIntentId"/> so the 96h deadline is enforceable regardless
/// of buyer engagement. This handler therefore deals with three cases:</para>
/// <list type="bullet">
///   <item><description><b>First open</b>: row exists, intent id null → mint Stripe intent, write it onto the row.</description></item>
///   <item><description><b>Resume</b>: row exists, intent id set → retrieve client_secret from Stripe so Elements can remount.</description></item>
///   <item><description><b>Defensive create</b>: row missing (legacy data pre-dating Option A, or a manual cleanup) → full create as before.</description></item>
/// </list>
/// </summary>
public class CreatePaymentIntentHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock,
    IStripeClient stripe)
    : IRequestHandler<CreatePaymentIntentRequest, Result<CreatePaymentIntentResponse>>
{
    private static readonly TimeSpan PaymentWindow = TimeSpan.FromHours(96);

    public async Task<Result<CreatePaymentIntentResponse>> Handle(
        CreatePaymentIntentRequest request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == request.LotId, ct);
        if (lot is null)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.NotFound("Lot.NotFound", $"Lot {request.LotId} does not exist."));

        if (lot.Status != LotStatus.Sold || lot.WinningBidId is null)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Validation("Lot.NotSold", "PaymentIntent can only be created for a sold lot."));

        Bid? winningBid = await db.Bids.FirstOrDefaultAsync(b => b.Id == lot.WinningBidId, ct);
        if (winningBid is null)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.NotFound("Bid.NotFound", "Winning bid not found."));

        if (winningBid.BidderId != userId)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Forbidden("Lot.NotWinner", "Only the winning bidder can create a PaymentIntent."));

        Payment? existingPayment = await db.Payments.FirstOrDefaultAsync(p => p.LotId == lot.Id, ct);

        // Branch on existing-row state. Non-pending statuses are terminal/locked — refuse to mint a
        // second intent for the same lot.
        if (existingPayment is not null && existingPayment.Status != PaymentStatus.PendingAuthorization)
        {
            return existingPayment.Status is PaymentStatus.Authorized or PaymentStatus.Captured
                ? Result.Failure<CreatePaymentIntentResponse>(Error.Conflict(
                    "Payment.AlreadyAuthorized",
                    "This lot has already been paid — open it in My Purchases."))
                : Result.Failure<CreatePaymentIntentResponse>(Error.Conflict(
                    "Payment.Terminal",
                    $"This lot's payment is in terminal {existingPayment.Status} — contact support."));
        }

        // Resume path — the buyer already minted an intent in a previous session. Retrieve it from
        // Stripe and surface the same client_secret so Elements remounts cleanly.
        if (existingPayment is { StripePaymentIntentId: { Length: > 0 } })
        {
            return await ResumeExistingAsync(existingPayment, ct);
        }

        // First-mint path. Both fresh-from-AuctionCloseJob rows (intent id null) and the defensive
        // "no row at all" fallback fall through here. Validate shipment + seller before touching
        // Stripe — these are real preconditions for a successful charge.
        Shipment? shipment = await db.Shipments
            .FirstOrDefaultAsync(s => s.LotId == lot.Id && s.Status == ShipmentStatus.PendingTtn, ct);
        if (shipment is null)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Validation("Shipment.NoCheckoutDetails", "Submit checkout details before creating a PaymentIntent."));

        User? seller = await db.Users.FirstOrDefaultAsync(u => u.Id == lot.SellerId, ct);
        if (seller is null || string.IsNullOrWhiteSpace(seller.StripeAccountId) || !seller.StripeOnboarded)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Validation("Stripe.SellerNotOnboarded", "Seller is not ready to receive payments via Stripe Connect."));

        // For the existing-row case we keep the close-time rate locked (audit). For the defensive
        // path we lock at the current rate the same way AuctionCloseJob would have.
        decimal rate = existingPayment?.RateUsedUahPerUsd ?? stripe.UahPerUsdRate;
        long amountUah = existingPayment?.AmountUahKopiykas ?? winningBid.AmountUahKopiykas;
        long amountUsdCents = existingPayment?.AmountUsdCents
            ?? CurrencyConverter.UahKopiykasToUsdCents(amountUah, rate);

        Guid paymentId = existingPayment?.Id ?? Guid.NewGuid();
        DateTime now = clock.UtcNow;

        var metadata = new Dictionary<string, string>
        {
            ["lotId"] = lot.Id.ToString(),
            ["paymentId"] = paymentId.ToString(),
            ["buyerId"] = userId.ToString(),
            ["sellerId"] = seller.Id.ToString(),
        };

        StripePaymentIntentResult intent;
        try
        {
            intent = await stripe.CreatePaymentIntentAsync(
                amountUsdCents,
                seller.StripeAccountId!,
                metadata,
                idempotencyKey: lot.Id.ToString(),
                ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Internal("Stripe.IntentCreateFailed", ex.Message));
        }

        if (existingPayment is not null)
        {
            // Update the pre-created row in-place. CreatedAt stays as close-time so DueAt remains
            // anchored correctly; only fields the intent contributes are touched.
            existingPayment.StripePaymentIntentId = intent.Id;
            existingPayment.UpdatedAt = now;
        }
        else
        {
            // Defensive fallback only — Option A guarantees AuctionCloseJob inserted a row, but
            // historical rows or manual cleanups could still hit this path.
            db.Payments.Add(new Payment
            {
                Id = paymentId,
                LotId = lot.Id,
                BuyerId = userId,
                SellerId = seller.Id,
                AmountUahKopiykas = amountUah,
                AmountUsdCents = amountUsdCents,
                RateUsedUahPerUsd = rate,
                StripePaymentIntentId = intent.Id,
                Status = PaymentStatus.PendingAuthorization,
                DueAt = lot.EndsAt + PaymentWindow,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        shipment.PaymentId = paymentId;
        shipment.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return Result.Success(new CreatePaymentIntentResponse(
            paymentId,
            intent.ClientSecret ?? string.Empty,
            stripe.PublishableKey,
            amountUah,
            amountUsdCents,
            rate));
    }

    /// <summary>
    /// Resume path for a returning buyer: fetch the existing intent's client_secret from Stripe so
    /// Elements can remount the card form. We do NOT touch local Payment state — it stays in
    /// PendingAuthorization until the webhook says otherwise.
    /// </summary>
    private async Task<Result<CreatePaymentIntentResponse>> ResumeExistingAsync(Payment payment, CancellationToken ct)
    {
        StripePaymentIntentResult intent;
        try
        {
            intent = await stripe.RetrievePaymentIntentAsync(payment.StripePaymentIntentId!, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Internal("Stripe.IntentRetrieveFailed", ex.Message));
        }

        return Result.Success(new CreatePaymentIntentResponse(
            payment.Id,
            intent.ClientSecret ?? string.Empty,
            stripe.PublishableKey,
            payment.AmountUahKopiykas,
            payment.AmountUsdCents,
            payment.RateUsedUahPerUsd));
    }
}
