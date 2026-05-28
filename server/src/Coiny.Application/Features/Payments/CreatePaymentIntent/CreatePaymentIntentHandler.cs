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

        bool paymentAlreadyExists = await db.Payments.AnyAsync(p => p.LotId == lot.Id, ct);
        if (paymentAlreadyExists)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Conflict("Payment.AlreadyExists", "A payment for this lot has already been created."));

        Shipment? shipment = await db.Shipments
            .FirstOrDefaultAsync(s => s.LotId == lot.Id && s.Status == ShipmentStatus.PendingTtn, ct);
        if (shipment is null)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Validation("Shipment.NoCheckoutDetails", "Submit checkout details before creating a PaymentIntent."));

        User? seller = await db.Users.FirstOrDefaultAsync(u => u.Id == lot.SellerId, ct);
        if (seller is null || string.IsNullOrWhiteSpace(seller.StripeAccountId) || !seller.StripeOnboarded)
            return Result.Failure<CreatePaymentIntentResponse>(
                Error.Validation("Stripe.SellerNotOnboarded", "Seller is not ready to receive payments via Stripe Connect."));

        decimal rate = stripe.UahPerUsdRate;
        long amountUah = winningBid.AmountUahKopiykas;
        long amountUsdCents = CurrencyConverter.UahKopiykasToUsdCents(amountUah, rate);

        Guid paymentId = Guid.NewGuid();
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

        var payment = new Payment
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
        };

        db.Payments.Add(payment);
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
}
