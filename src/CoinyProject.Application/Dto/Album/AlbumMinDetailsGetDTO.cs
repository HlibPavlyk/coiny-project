using CoinyProject.Application.Dto.User;

namespace CoinyProject.Application.Dto.Album;

public class AlbumMinDetailsGetDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public UserNameGetDto Author { get; set; }
}
