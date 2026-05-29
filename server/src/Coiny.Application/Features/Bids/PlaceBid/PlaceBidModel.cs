namespace Coiny.Application.Features.Bids.PlaceBid;

public record PlaceBidModel(
    Guid Id,
    Guid LotId,
    long AmountUahKopiykas,
    long NewCurrentPriceUahKopiykas,
    int NewBidCount,
    DateTime NewEndsAt);
