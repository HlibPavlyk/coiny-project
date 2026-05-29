using Coiny.Application.Common.Search;

namespace Coiny.Application.Features.Lots.Models;

/// <summary>
/// A lot as shown in listing/search results. The <c>[Sortable]</c> properties define the public sort
/// contract — clients may sort only by columns they actually receive here (column name = camelCase
/// property name).
/// </summary>
public record LotCardModel
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string CoverImageUrl { get; init; }

    [Sortable] public required long CurrentPriceUahKopiykas { get; init; }
    [Sortable] public required int BidCount { get; init; }
    [Sortable] public required DateTime EndsAt { get; init; }

    // Carried for the "Newest" sort; not shown on the card itself.
    [Sortable] public required DateTime CreatedAt { get; init; }
}
