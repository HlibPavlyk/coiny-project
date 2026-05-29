using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Lots.GetMyLots;

public record MyLotItemModel(
    Guid Id,
    string Title,
    string CoverImageUrl,
    long CurrentPriceUahKopiykas,
    int BidCount,
    DateTime EndsAt,
    LotStatus Status,
    DateTime? DeletedAt);
