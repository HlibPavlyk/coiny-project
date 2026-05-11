namespace Coiny.Application.Features.Bids.Models;

/// <summary>
/// One row of a lot's bid history. <see cref="BidderDisplay"/> is anonymized as
/// <c>b****&lt;n&gt;</c> while the lot is still Active; once Sold/EndedNoSale/Cancelled
/// it carries the bidder's full display name.
/// </summary>
public record BidItemModel(
    Guid Id,
    long AmountUahKopiykas,
    string BidderDisplay,
    DateTime CreatedAt);
