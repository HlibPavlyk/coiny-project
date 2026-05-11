using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Bids.Handlers;

public class GetMyBidsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyBidsRequest, Result<Paginated<MyBidItemModel>>>
{
    public async Task<Result<Paginated<MyBidItemModel>>> Handle(GetMyBidsRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<Paginated<MyBidItemModel>>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Result<IQueryable<Bid>> sortResult = db.Bids
            .AsNoTracking()
            .Where(b => b.BidderId == userId)
            .SortBy(request.SortBy, b => b.CreatedAt, b => b.AmountUahKopiykas);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<MyBidItemModel>>(sortResult.Error);

        int totalCount = await sortResult.Value.CountAsync(ct);

        List<MyBidItemModel> items = await sortResult.Value
            .Paginate(request)
            .Select(b => new MyBidItemModel(
                b.Id,
                b.AmountUahKopiykas,
                b.CreatedAt,
                new MyBidLotInfo(
                    b.Lot!.Id,
                    b.Lot.Title,
                    b.Lot.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty,
                    b.Lot.CurrentPriceUahKopiykas,
                    b.Lot.Status,
                    b.Lot.EndsAt,
                    // Per task spec simplification: leader iff this bid's amount equals the lot's current price.
                    // Older bids by the same user on the same lot stay false; the most recent winning bid is true.
                    b.AmountUahKopiykas == b.Lot.CurrentPriceUahKopiykas)))
            .ToListAsync(ct);

        return Result.Success(new Paginated<MyBidItemModel>(totalCount, items));
    }
}
