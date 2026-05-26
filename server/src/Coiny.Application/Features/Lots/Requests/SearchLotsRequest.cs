using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

/// <summary>
/// Public, Meilisearch-backed full-text + faceted lot search. Returns a page of lot cards plus the facet
/// value lists that populate the filter pickers. Restricted to published statuses (<c>Active</c>/<c>Sold</c>).
/// Plain real-column browse belongs on the Postgres-backed <see cref="GetPublicLotsRequest"/>.
/// </summary>
public record SearchLotsRequest : PageRequest, IRequest<Result<FacetedPage<LotCardModel>>>
{
    public SearchLotsFilters Filters { get; init; } = new();
}

public record SearchLotsFilters
{
    /// <summary>Restrict to a category and all of its leaf descendants. Null = any category.</summary>
    public int? CategoryId { get; init; }

    /// <summary>Narrow to one published status. Null = all public statuses. Validated against the whitelist.</summary>
    public LotStatus? Status { get; init; }

    /// <summary>Free-text query (Meilisearch <c>q</c>). Null/empty defers to filters and ranking.</summary>
    public string? SearchText { get; init; }

    /// <summary>Lot condition codes; OR within the list.</summary>
    public IReadOnlyList<string>? Condition { get; init; }

    /// <summary>Metal facet values (from JSONB attributes); OR within.</summary>
    public IReadOnlyList<string>? Metal { get; init; }

    /// <summary>Country facet values (from JSONB attributes); OR within.</summary>
    public IReadOnlyList<string>? Country { get; init; }

    public long? PriceUahKopiykasFrom { get; init; }
    public long? PriceUahKopiykasTo { get; init; }

    /// <summary>Upper bound (inclusive) on the auction end time.</summary>
    public DateTime? EndingBefore { get; init; }
}
