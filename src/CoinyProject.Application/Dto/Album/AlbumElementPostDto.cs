
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Dto.Album
{
    public record AlbumElementPostDto(string Name, string? Description, Guid AlbumId, IFormFile Photo);
}
