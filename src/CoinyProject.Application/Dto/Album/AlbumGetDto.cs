using CoinyProject.Application.Dto.User;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Application.Dto.Album;

public class AlbumGetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Rate { get; set; }
    public AlbumStatus Status { get; set; }
    public UserNameGetDto Author { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}