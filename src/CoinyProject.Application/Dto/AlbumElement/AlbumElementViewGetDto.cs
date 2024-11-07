
using CoinyProject.Application.Dto.Album;

namespace CoinyProject.Application.Dto.AlbumElement
{
    public class AlbumElementViewGetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int  Rate { get; init; }
        public string ImageUrl { get; set; } = string.Empty;
        public AlbumMinDetailsGetDTO Album { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
       
    }
}
