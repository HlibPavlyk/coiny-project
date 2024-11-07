
namespace CoinyProject.Domain.Entities
{
    public class FavoriteAlbumElements
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid AlbumElementId { get; init; }

        public User User { get; init; }
        public AlbumElement AlbumElement { get; init; } 
    }
}
