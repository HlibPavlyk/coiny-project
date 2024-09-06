
namespace CoinyProject.Application.Dto.AlbumElement
{
    public class AlbumElementGetDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string ImageUrl { get; set; } =string.Empty;
    }
}
