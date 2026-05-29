using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Results;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.GetLotPaymentState;

/// <summary>
/// Resolves the lot's purchase-flow state for the calling user. Caller must be the winning bidder;
/// otherwise 403. If the lot has no payment yet but the caller is the winner, returns an empty
/// state so the frontend can render step 1.
/// </summary>
public class GetLotPaymentStateHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetLotPaymentStateRequest, Result<LotPaymentStateModel>>
{
    public async Task<Result<LotPaymentStateModel>> Handle(GetLotPaymentStateRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<LotPaymentStateModel>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        var lot = await db.Lots
            .AsNoTracking()
            .Where(l => l.Id == request.LotId)
            .Select(l => new { l.Id, l.Status, l.WinningBidId })
            .FirstOrDefaultAsync(ct);

        if (lot is null)
            return Result.Failure<LotPaymentStateModel>(
                Error.NotFound("Lot.NotFound", $"Lot {request.LotId} does not exist."));

        if (lot.Status != LotStatus.Sold || lot.WinningBidId is null)
            return Result.Failure<LotPaymentStateModel>(
                Error.Validation("Lot.NotSold", "Payment flow is only available for sold lots."));

        Guid? winnerId = await db.Bids
            .Where(b => b.Id == lot.WinningBidId)
            .Select(b => (Guid?)b.BidderId)
            .FirstOrDefaultAsync(ct);

        if (winnerId != userId)
            return Result.Failure<LotPaymentStateModel>(
                Error.Forbidden("Lot.NotWinner", "Only the winning bidder can view this state."));

        bool shipmentExists = await db.Shipments
            .AsNoTracking()
            .AnyAsync(s => s.LotId == lot.Id, ct);

        PaymentStateRow? payment = await db.Payments
            .AsNoTracking()
            .Where(p => p.LotId == lot.Id)
            .Select(p => new PaymentStateRow(p.Id, p.Status))
            .FirstOrDefaultAsync(ct);

        return Result.Success(new LotPaymentStateModel(shipmentExists, payment));
    }
}
