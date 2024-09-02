
namespace CoinyProject.Domain.Entities
{
    public class AlbumElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int  Rate { get; set; }
        public string ImageURL { get; set; }
        public Guid AlbumId { get; set; }

        public Album? Album { get; set; }
        public Auction? Auction { get; set; }
    }
}
