using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Search;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Categories.Services;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

/// <summary>
/// Public, Meilisearch-backed lot search. Builds an engine-agnostic <see cref="LotSearchQuery"/> from
/// the request filters (the category id is expanded to its leaf-id set in Postgres first), runs it
/// through the <see cref="ISearchIndex"/> seam, and projects the hit documents into lot cards (the facet
/// value lists ride through unchanged). Status is always constrained to published values so
/// seller-private lots never surface.
/// </summary>
public class SearchLotsHandler(IApplicationDbContext db, ISearchIndex search)
    : IRequestHandler<SearchLotsRequest, Result<FacetedPage<LotCardModel>>>
{
    // Sortable columns are defined by the response model (LotCardModel) — clients may only sort by what
    // they actually receive. Column name == index field name (kept aligned by design), so no mapping.
    private static readonly string[] _sortableColumns =
        SearchDocumentSchema.FieldsWith<LotCardModel, SortableAttribute>();

    public async Task<Result<FacetedPage<LotCardModel>>> Handle(SearchLotsRequest request, CancellationToken ct)
    {
        SearchLotsFilters filters = request.Filters;

        IReadOnlyList<int> categoryIds = [];
        if (filters.CategoryId is { } categoryId)
        {
            List<Category> all = await db.Categories.AsNoTracking().ToListAsync(ct);

            Category? root = all.FirstOrDefault(c => c.Id == categoryId);
            if (root is null)
                return Result.Failure<FacetedPage<LotCardModel>>(
                    Error.NotFound("Category.NotFound", $"Category {categoryId} does not exist."));

            categoryIds = CategoryHierarchy.LeafIdsUnder(root, all);
        }

        Result<IReadOnlyList<SortByModel>> sort = ResolveSort(request.SortBy, filters.SearchText);
        if (sort.IsFailure)
            return Result.Failure<FacetedPage<LotCardModel>>(sort.Error);

        IReadOnlyList<string> statuses = filters.Status is { } status
            ? [status.ToString()]
            : [nameof(LotStatus.Active), nameof(LotStatus.Sold)];

        var query = new LotSearchQuery
        {
            Text = filters.SearchText ?? string.Empty,
            CategoryIds = categoryIds,
            Statuses = statuses,
            Conditions = filters.Condition ?? [],
            Metals = filters.Metal ?? [],
            Countries = filters.Country ?? [],
            PriceFrom = filters.PriceUahKopiykasFrom,
            PriceTo = filters.PriceUahKopiykasTo,
            EndingBeforeUnix = filters.EndingBefore is { } endsBefore
                ? new DateTimeOffset(DateTime.SpecifyKind(endsBefore, DateTimeKind.Utc)).ToUnixTimeSeconds()
                : null,
            Sort = sort.Value,
            Offset = request.Offset,
            Limit = request.Count,
        };

        FacetedPage<LotSearchDocument> result = await search.SearchAsync(query, ct);

        var items = result.Items.Select(ToCard).ToList();

        // Facets are independent of the item type — carry them through while remapping documents → cards.
        return Result.Success(new FacetedPage<LotCardModel>(result.TotalCount, items, result.Facets));
    }

    // Empty sort + non-empty query → defer to relevance (no explicit sort). Empty sort + empty query →
    // default to endsAt asc. Otherwise validate each column against the sortable allowlist (400 on an
    // unknown column). Columns pass through unchanged — the name already matches the index field.
    private static Result<IReadOnlyList<SortByModel>> ResolveSort(SortByModel[]? sortBy, string? searchText)
    {
        if (sortBy is not { Length: > 0 })
            return string.IsNullOrWhiteSpace(searchText)
                ? Result.Success<IReadOnlyList<SortByModel>>([new SortByModel("endsAt", SortDirection.Asc)])
                : Result.Success<IReadOnlyList<SortByModel>>([]);

        foreach (SortByModel criterion in sortBy)
            if (!_sortableColumns.Contains(criterion.ColumnName, StringComparer.OrdinalIgnoreCase))
                return Error.Validation("Sort.InvalidColumn", $"Column '{criterion.ColumnName}' is not sortable.");

        return Result.Success<IReadOnlyList<SortByModel>>(sortBy);
    }

    private static LotCardModel ToCard(LotSearchDocument d) => new()
    {
        Id = Guid.Parse(d.Id),
        Title = d.Title,
        CoverImageUrl = d.CoverImageUrl,
        CurrentPriceUahKopiykas = d.CurrentPriceUahKopiykas,
        BidCount = d.BidCount,
        EndsAt = DateTimeOffset.FromUnixTimeSeconds(d.EndsAt).UtcDateTime,
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(d.CreatedAt).UtcDateTime,
    };
}
