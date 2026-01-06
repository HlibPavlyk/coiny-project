namespace CoinyProject.Application.Features.AlbumElements.Models;

public record MoveAlbumElementModel
{
    public Guid TargetAlbumId { get; set; }
}