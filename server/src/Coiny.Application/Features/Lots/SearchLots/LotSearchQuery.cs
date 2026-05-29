using Coiny.Application.Common.Querying;

namespace Coiny.Application.Features.Lots.SearchLots;

/// <summary>
/// Structured, engine-agnostic lot search criteria. The handler builds this from request filters; the
/// Infrastructure adapter translates it into the concrete search-engine syntax. Keeping it structured
/// (not a raw Meilisearch filter string) preserves the <c>ISearchIndex</c> seam.
/// </summary>
public sealed record LotSearchQuery
{
    /// <summary>Free-text query (Meilisearch <c>q</c>). Empty string defers to filter/sort only.</summary>
    public string Text { get; init; } = "";

    /// <summary>Leaf category ids to match (already expanded from the requested category subtree).</summary>
    public IReadOnlyList<int> CategoryIds { get; init; } = [];

    /// <summary>Lot statuses to include — always a subset of the publicly visible set.</summary>
    public IReadOnlyList<string> Statuses { get; init; } = [];

    public IReadOnlyList<string> Conditions { get; init; } = [];
    public IReadOnlyList<string> Metals { get; init; } = [];
    public IReadOnlyList<string> Countries { get; init; } = [];

    public long? PriceFrom { get; init; }
    public long? PriceTo { get; init; }

    /// <summary>Upper bound on the auction end time, as Unix seconds (the indexed numeric form).</summary>
    public long? EndingBeforeUnix { get; init; }

    /// <summary>
    /// Ordered sort criteria; each column name is already a validated, index-matching sortable field.
    /// Empty defers to the engine's relevance ranking.
    /// </summary>
    public IReadOnlyList<SortByModel> Sort { get; init; } = [];

    public int Offset { get; init; }
    public int Limit { get; init; }
}
