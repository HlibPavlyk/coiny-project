using Coiny.Application.Common.Search;

namespace Coiny.Application.Features.Lots.Models;

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
    // Country is both full-text searchable and a sidebar facet (THESIS-SCOPE §11 facets).
    [Searchable, Filterable] public string? Country { get; init; }
    // Year is free-text in the lot's JSONB attributes (not guaranteed numeric), and it's a
    // searchable-only field — so it's stored as a string rather than parsed to int.
    [Searchable] public string? Year { get; init; }

    [Filterable] public required string Status { get; init; }
    [Filterable] public required int CategoryId { get; init; }
    [Filterable] public required string Condition { get; init; }
    // Mirrored from the lot's JSONB attributes; a sidebar facet (per the planned facet set).
    [Filterable] public string? Metal { get; init; }

    [Filterable, Sortable] public required long CurrentPriceUahKopiykas { get; init; }
    // Stored as Unix seconds — Meilisearch sorts/filters numerically. The field name matches the public
    // sort column (the "Unix" storage detail is an implementation note, not part of the field name).
    [Filterable, Sortable] public required long EndsAt { get; init; }

    [Sortable] public required long CreatedAt { get; init; }
    [Sortable] public required int BidCount { get; init; }

    // Rendering-only — returned with results, not searched/faceted/sorted.
    public required string CoverImageUrl { get; init; }
}
