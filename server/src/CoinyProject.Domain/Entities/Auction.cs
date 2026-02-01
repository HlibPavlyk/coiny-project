using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Auction
    {
        public Guid Id { get; init; }
        public Guid AlbumElementId { get; init; }
        public decimal StartPrice { get; init; }
        public decimal BetDelta { get; init; } 
        public DateTime StartTime { get; init; }
        public DateTime ExpirationTime { get; init; }
        public AuctionStatus Status { get; init; }

        public AlbumElement AlbumElement { get; init; }
        public ICollection<AuctionBet> AuctionBets { get; init; }
    }
}
