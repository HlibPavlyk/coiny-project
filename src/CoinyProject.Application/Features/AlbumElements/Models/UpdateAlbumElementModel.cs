namespace CoinyProject.Application.Features.AlbumElements.Models;

public record UpdateAlbumElementModel
{
    public string Name { get; set; }
    public string Description { get; set; }
}