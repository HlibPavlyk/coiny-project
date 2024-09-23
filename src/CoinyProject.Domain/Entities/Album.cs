using CoinyProject.Domain.Abstractions;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Album : IUpdateable
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public AlbumStatus Status { get; set; }
        public int Rate { get; set; }
        public Guid UserId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; init; }
        public ICollection<AlbumElement> Elements { get; set; }
    }
}
