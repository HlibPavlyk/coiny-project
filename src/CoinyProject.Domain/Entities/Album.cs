using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Album
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public AlbumStatus Status { get; init; }
        public int Rate { get; set; }
        public Guid UserId { get; init; }

        public User User { get; init; }
        public ICollection<AlbumElement> Elements { get; init; }
    }
}
