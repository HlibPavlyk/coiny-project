using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.GetPublicLots;

/// <summary>
/// Public, paginated lot browse served from Postgres (immediate consistency). Filters by real columns
/// only — category subtree, seller, status. Free-text and faceted/attribute search live on the
/// Meilisearch-backed <see cref="SearchLots.SearchLotsRequest"/>. Visibility is always restricted to published
/// statuses (<c>Active</c>/<c>Sold</c>); the seller-owned counterpart is <see cref="GetMyLots.GetMyLotsRequest"/>.
/// </summary>
public record GetPublicLotsRequest : PageRequest, IRequest<Result<Paginated<LotCardModel>>>
{
    public GetPublicLotsFilters Filters { get; init; } = new();
}

public record GetPublicLotsFilters
{
    /// <summary>Restrict to a category and all of its leaf descendants. Null = any category.</summary>
    public int? CategoryId { get; init; }

    /// <summary>Restrict to a single seller. Null = any seller.</summary>
    public Guid? SellerId { get; init; }

    /// <summary>
    /// Narrow to one published status. Null falls back to all public statuses. Non-public values are
    /// rejected by <c>GetPublicLotsValidator</c>.
    /// </summary>
    public LotStatus? Status { get; init; }
}
