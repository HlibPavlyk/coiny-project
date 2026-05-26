using Coiny.Application.Common.Querying;

namespace Coiny.Application.Common.Search;

/// <summary>
/// A <see cref="Paginated{T}"/> page extended with the facet distribution of a faceted search — so
/// <c>totalCount</c>, <c>items</c> and <c>facets</c> sit at one level. <see cref="Facets"/> maps each
/// faceted field name (e.g. <c>"metal"</c>, <c>"country"</c>) to its present values + counts; consumers
/// use these lists to populate filter pickers. The field set is whatever the search adapter is
/// configured to facet on, so adding/removing a facet needs no change here.
/// </summary>
public sealed record FacetedPage<T>(
    int TotalCount,
    IReadOnlyList<T> Items,
    IReadOnlyDictionary<string, IReadOnlyList<FacetValue>> Facets)
    : Paginated<T>(TotalCount, Items);
