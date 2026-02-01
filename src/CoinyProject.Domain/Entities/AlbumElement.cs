using CoinyProject.Domain.Abstractions;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class AlbumElement : IUpdateable
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int  Rate { get; init; }
        public string ImageUrl { get; set; }
        public AlbumElementStatus Status { get; set; } = AlbumElementStatus.NotApproved;
        public Guid AlbumId { get; set; }
        public Guid? AuctionId { get; init; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public Album Album { get; init; }
        public Auction Auction { get; init; }
        public ICollection<FavoriteAlbumElements> FavoriteAlbumElements { get; init; }
    }
}
