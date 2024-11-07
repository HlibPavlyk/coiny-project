
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Dto.AlbumElement
{
    public record AlbumElementPostDto(string Name, string? Description, Guid AlbumId, IFormFile Photo);
}
