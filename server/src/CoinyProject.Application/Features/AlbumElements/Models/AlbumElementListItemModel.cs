using CoinyProject.Domain.Enums;

namespace CoinyProject.Application.Features.AlbumElements.Models;

public class AlbumElementListItemModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int  Rate { get; init; }
    public Uri ImageUrl { get; set; }
    public AlbumElementStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}