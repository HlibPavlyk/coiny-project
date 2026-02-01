using CoinyProject.Application.Common.Models;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Application.Features.Albums.Models;

public class AlbumModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Rate { get; set; }
    public AlbumStatus Status { get; set; }
    public BaseLink Author { get; set; }
    public AlbumElementLink[] Images { get; set; }

    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}