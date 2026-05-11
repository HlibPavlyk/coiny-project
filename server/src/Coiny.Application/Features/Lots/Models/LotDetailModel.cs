using System.Text.Json;
using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Lots.Models;

public record LotDetailModel(
    Guid Id,
    string Title,
    string Description,
    LotCategoryBreadcrumbModel Category,
    LotCondition Condition,
    long StartingPriceUahKopiykas,
    long CurrentPriceUahKopiykas,
    int BidCount,
    int ViewCount,
    LotStatus Status,
    DateTime? StartsAt,
    DateTime EndsAt,
    JsonElement Attributes,
    IReadOnlyList<LotImageModel> Images,
    LotSellerModel Seller,
    LotWinningBidModel? WinningBid,
    bool IsCallerLeading);

public record LotCategoryBreadcrumbModel(int Id, string Slug, IReadOnlyList<string> NamePath);

public record LotImageModel(Guid Id, string PublicUrl, int DisplayOrder, int Width, int Height);

public record LotSellerModel(Guid Id, string DisplayName, int TrustScore);

public record LotWinningBidModel(Guid Id, string BidderDisplayName, long AmountUahKopiykas);
