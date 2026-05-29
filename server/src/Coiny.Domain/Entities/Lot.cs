using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Lot
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public LotCondition Condition { get; set; }

    public long StartingPriceUahKopiykas { get; set; }

    public long CurrentPriceUahKopiykas { get; set; }

    public int BidCount { get; set; }

    public int ViewCount { get; set; }

    public LotStatus Status { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    /// <summary>FK to Bid.Id. Null until auction closes. Populated with the winning (highest) bid.</summary>
    public Guid? WinningBidId { get; set; }

    /// <summary>
    /// Hangfire job id of the scheduled <c>AuctionCloseJob</c>. Set by <c>PublishLotHandler</c> and
    /// refreshed by <c>PlaceBidHandler</c> on every anti-snipe extension. Null for Draft/closed lots.
    /// </summary>
    public string? AuctionCloseJobId { get; set; }

    /// <summary>Subcategory-specific attributes stored as JSON. Maps to a jsonb column (EF config in task 05).</summary>
    public string Attributes { get; set; } = "{}";

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? Seller { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<LotImage> Images { get; set; } = [];
}
