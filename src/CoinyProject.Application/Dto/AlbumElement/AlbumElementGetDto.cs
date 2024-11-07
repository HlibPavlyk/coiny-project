
namespace CoinyProject.Application.Dto.AlbumElement
{
    public class AlbumElementGetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int  Rate { get; init; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
       
    }
}
