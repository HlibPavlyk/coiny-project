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
    bool IsCallerLeading,
    // Populated only for authenticated winning bidders on Sold lots. Drives the post-win CTA
    // in <c>BidPanel.ClosedState</c>: null → show "Complete checkout"; non-null → adapt the
    // copy and target to "Payment in progress" / "View order" so the buyer cannot re-enter
    // checkout for an already-paid lot.
    Guid? CallerPaymentId,
    PaymentStatus? CallerPaymentStatus);

public record LotCategoryBreadcrumbModel(int Id, string Slug, IReadOnlyList<string> NamePath);

public record LotImageModel(Guid Id, string PublicUrl, int DisplayOrder, int Width, int Height);

public record LotSellerModel(Guid Id, string DisplayName, int TrustScore);

public record LotWinningBidModel(Guid Id, string BidderDisplayName, long AmountUahKopiykas);
