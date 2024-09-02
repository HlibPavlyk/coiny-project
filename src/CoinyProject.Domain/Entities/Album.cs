
using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Album
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public AlbumStatus Status { get; set; }
        public int Rate { get; set; }
        public Guid? UserId { get; set; }

        public User? User { get; set; }
        public ICollection<AlbumElement>? Elements { get; set; }
        public ICollection<FavoriteAlbumElements>? FavoriteAlbums { get; set; }
    }
}
