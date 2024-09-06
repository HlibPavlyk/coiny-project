
using CoinyProject.Application.Dto.Other;

namespace CoinyProject.Application.DTO.Album
{
    public record AlbumWithElementsGetDto(
        Guid Id,
        string Name,
        string? Description,
        int Rate,
        Guid UserId);

}
