using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Bids.GetMyBids;

/// <summary>One row of the caller's bid history with the lot's current state and leader flag.</summary>
public record MyBidItemModel(
    Guid BidId,
    long AmountUahKopiykas,
    DateTime CreatedAt,
    MyBidLotInfo Lot);

public record MyBidLotInfo(
    Guid Id,
    string Title,
    string CoverImageUrl,
    long CurrentPriceUahKopiykas,
    LotStatus Status,
    DateTime EndsAt,
    bool IsCallerLeading);
