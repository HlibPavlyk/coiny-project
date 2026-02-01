namespace CoinyProject.Application.Features.Albums.Models;

public record UpdateAlbumModel
{
    public string Name { get; set; }
    public string Description { get; set; }
}