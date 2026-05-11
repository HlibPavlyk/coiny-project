namespace Coiny.Application.Features.Bids.Models;

public record PlaceBidModel(
    Guid Id,
    Guid LotId,
    long AmountUahKopiykas,
    long NewCurrentPriceUahKopiykas,
    int NewBidCount,
    DateTime NewEndsAt);
