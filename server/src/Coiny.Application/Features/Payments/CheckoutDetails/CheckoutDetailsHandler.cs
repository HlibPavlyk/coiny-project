using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.CheckoutDetails;

public class CheckoutDetailsHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<CheckoutDetailsRequest, Result>
{
    public async Task<Result> Handle(CheckoutDetailsRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == request.LotId, ct);
        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", $"Lot {request.LotId} does not exist."));

        if (lot.Status != LotStatus.Sold || lot.WinningBidId is null)
            return Result.Failure(Error.Validation("Lot.NotSold", "Checkout details are only accepted for sold lots."));

        Bid? winningBid = await db.Bids.FirstOrDefaultAsync(b => b.Id == lot.WinningBidId, ct);
        if (winningBid is null)
            return Result.Failure(Error.NotFound("Bid.NotFound", "Winning bid not found."));

        if (winningBid.BidderId != userId)
            return Result.Failure(Error.Forbidden("Lot.NotWinner", "Only the winning bidder can submit checkout details."));

        bool alreadyStaged = await db.Shipments.AnyAsync(s => s.LotId == lot.Id, ct);
        if (alreadyStaged)
            return Result.Failure(Error.Conflict("Shipment.AlreadyStaged", "Checkout details already submitted for this lot."));

        DateTime now = clock.UtcNow;

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            // PaymentId is null at this stage — CreatePaymentIntentHandler links it on next step.
            PaymentId = null,
            LotId = lot.Id,
            BuyerId = userId,
            SellerId = lot.SellerId,
            RecipientCityRef = request.Model.RecipientCityRef.Trim(),
            RecipientCityLabel = request.Model.RecipientCityLabel.Trim(),
            RecipientWarehouseRef = request.Model.RecipientWarehouseRef.Trim(),
            RecipientWarehouseLabel = request.Model.RecipientWarehouseLabel.Trim(),
            RecipientName = request.Model.RecipientName.Trim(),
            RecipientPhone = request.Model.RecipientPhone.Trim(),
            DeclaredValueUahKopiykas = winningBid.AmountUahKopiykas,
            Status = ShipmentStatus.PendingTtn,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
