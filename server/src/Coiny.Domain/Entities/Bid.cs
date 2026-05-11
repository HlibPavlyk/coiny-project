namespace Coiny.Domain.Entities;

public class Bid
{
    public Guid Id { get; set; }

    public Guid LotId { get; set; }

    public Guid BidderId { get; set; }

    public long AmountUahKopiykas { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Lot? Lot { get; set; }

    public virtual User? Bidder { get; set; }
}
