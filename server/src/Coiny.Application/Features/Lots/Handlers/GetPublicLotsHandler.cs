using Coiny.Application.Abstractions.Data;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories.Services;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

/// <summary>
/// Public, paginated lot browse over real columns (category subtree / seller / status), served from
/// Postgres for immediate consistency. Free-text and faceted/attribute search go through Meilisearch
/// via <c>SearchLotsHandler</c> (JSONB must never be filtered in EF — THESIS-SCOPE risk #6). Restricts
/// to published statuses, so seller-private lots never leak.
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
            .Select(l => new LotCardModel
            {
                Id = l.Id,
                Title = l.Title,
                CoverImageUrl = l.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty,
                CurrentPriceUahKopiykas = l.CurrentPriceUahKopiykas,
                BidCount = l.BidCount,
                EndsAt = l.EndsAt,
            })
            .ToListAsync(ct);

        return Result.Success(new Paginated<LotCardModel>(totalCount, items));
    }
}
