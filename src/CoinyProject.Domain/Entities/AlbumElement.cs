
namespace CoinyProject.Domain.Entities
{
    public class AlbumElement
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int  Rate { get; init; }
        public string ImageURL { get; set; }
        public Guid AlbumId { get; init; }
        public Guid? AuctionId { get; init; }

        public Album Album { get; init; }
        public Auction? Auction { get; init; }
        public ICollection<FavoriteAlbumElements> FavoriteAlbumElements { get; init; }
    }
}
