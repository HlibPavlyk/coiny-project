using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class GetMyLotsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyLotsRequest, Result<Paginated<MyLotItemModel>>>
{
    public async Task<Result<Paginated<MyLotItemModel>>> Handle(GetMyLotsRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<Paginated<MyLotItemModel>>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Result<IQueryable<Lot>> sortResult = db.Lots
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(l => l.SellerId == userId)
            .WhereIf(request.Filters.Status is not null, l => l.Status == request.Filters.Status)
            .SortBy(
                request.SortBy,
                l => l.EndsAt,
                l => l.CreatedAt,
                l => l.CurrentPriceUahKopiykas,
                l => l.BidCount);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<MyLotItemModel>>(sortResult.Error);

        var totalCount = await sortResult.Value.CountAsync(ct);

        List<MyLotItemModel> items = await sortResult.Value
            .Paginate(request)
            .Select(l => new MyLotItemModel(
                l.Id,
                l.Title,
                l.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty,
                l.CurrentPriceUahKopiykas,
                l.BidCount,
                l.EndsAt,
                l.Status,
                l.DeletedAt))
            .ToListAsync(ct);

        return Result.Success(new Paginated<MyLotItemModel>(totalCount, items));
    }
}
