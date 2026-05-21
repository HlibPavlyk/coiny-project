namespace Coiny.Application.Abstractions.Search;

/// <summary>
/// Flat projection of a Lot as stored in the Meilisearch <c>lots</c> index. The role of each field
/// (searchable / filterable / sortable) is declared inline via attributes; the search adapter reflects
/// them into the index configuration so the attribute lists can never drift from the document shape.
/// Field set is locked by THESIS-SCOPE §11. Dates are stored as Unix seconds (Meilisearch
/// sorts/filters on numbers, not ISO strings). Holds both Active and Sold lots.
/// </summary>
public sealed record LotSearchDocument
{
    /// <summary>Meilisearch primary key — the lot id as a string (GUIDs contain only [0-9a-f-], all allowed).</summary>
    public required string Id { get; init; }

    [Searchable] public required string Title { get; init; }
    [Searchable] public required string Description { get; init; }
    [Searchable] public required string CategoryPath { get; init; }
    [Searchable] public string? Country { get; init; }
    [Searchable] public int? Year { get; init; }

    [Filterable] public required string Status { get; init; }
    [Filterable] public required int CategoryId { get; init; }
    [Filterable] public required string Condition { get; init; }

    [Filterable, Sortable]public required long CurrentPriceUahKopiykas { get; init; }
    [Filterable, Sortable] public required long EndsAtUnix { get; init; }

    [Sortable] public required long CreatedAtUnix { get; init; }

    // Rendering-only — returned with results, not searched/faceted/sorted.
    public required string CoverImageUrl { get; init; }
    public required int BidCount { get; init; }
}
