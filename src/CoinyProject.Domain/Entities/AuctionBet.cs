using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class AuctionBet
    {
        public Guid Id { get; init; }
        public decimal Price { get; init; }
        public AuctionBetStatus Status { get; init; }
        public Guid UserId { get; init; }
        public Guid AuctionId { get; init; }

        public User User { get; init; }
        public Auction Auction { get; init; }
        
    }
}
