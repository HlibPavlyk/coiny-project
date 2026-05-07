namespace Coiny.Application.Features.Lots.Models;

public record LotCardModel(
    Guid Id,
    string Title,
    string CoverImageUrl,
    long CurrentPriceUahKopiykas,
    int BidCount,
    DateTime EndsAt);
