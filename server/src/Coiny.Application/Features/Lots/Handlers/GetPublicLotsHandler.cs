using Coiny.Application.Abstractions.Data;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

/// <summary>
/// Public, paginated lot listing. Optional <c>CategoryId</c> (expanded across leaf descendants) and
/// <c>SellerId</c> filters compose freely; status is constrained to published values either by the
/// caller's filter or by the public-status fallback below. Soft-deleted lots are excluded via the
/// global query filter (not bypassed here), so this path can never expose seller-private lots.
/// </summary>
public class GetPublicLotsHandler(IApplicationDbContext db)
    : IRequestHandler<GetPublicLotsRequest, Result<Paginated<LotCardModel>>>
{
    public async Task<Result<Paginated<LotCardModel>>> Handle(GetPublicLotsRequest request, CancellationToken ct)
    {
        GetPublicLotsFilters filters = request.Filters;

        IReadOnlyList<int>? leafIds = null;
        if (filters.CategoryId is { } categoryId)
        {
            List<Category> all = await db.Categories.AsNoTracking().ToListAsync(ct);

            Category? root = all.FirstOrDefault(c => c.Id == categoryId);
            if (root is null)
                return Result.Failure<Paginated<LotCardModel>>(
                    Error.NotFound("Category.NotFound", $"Category {categoryId} does not exist."));

            leafIds = CategoryHierarchy.LeafIdsUnder(root, all);
        }

        Result<IQueryable<Lot>> queryResult = db.Lots
            .AsNoTracking()
            .WhereIf(leafIds is not null, l => leafIds!.Contains(l.CategoryId))
            .WhereIf(filters.SellerId is not null, l => l.SellerId == filters.SellerId)
            .WhereIfElse(
                filters.Status is not null,
                l => l.Status == filters.Status,
                l => l.Status == LotStatus.Active || l.Status == LotStatus.Sold)
            .SortBy(
                request.SortBy,
                l => l.EndsAt,
                l => l.CreatedAt,
                l => l.CurrentPriceUahKopiykas,
                l => l.BidCount);

        if (queryResult.IsFailure)
            return Result.Failure<Paginated<LotCardModel>>(queryResult.Error);

        int totalCount = await queryResult.Value.CountAsync(ct);

        List<LotCardModel> items = await queryResult.Value
            .Paginate(request)
            .Select(l => new LotCardModel(
                l.Id,
                l.Title,
                l.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty,
                l.CurrentPriceUahKopiykas,
                l.BidCount,
                l.EndsAt))
            .ToListAsync(ct);

        return Result.Success(new Paginated<LotCardModel>(totalCount, items));
    }
}
