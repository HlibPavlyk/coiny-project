using CoinyProject.Application.Common.Models;

namespace CoinyProject.Application.Features.AlbumElements.Models;

public class AlbumElementModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int  Rate { get; init; }
    public Uri ImageUrl { get; set; }
    public BaseLink Album { get; set; }
    public BaseLink Author { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}