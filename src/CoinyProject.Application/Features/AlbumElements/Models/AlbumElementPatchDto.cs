using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Dto.AlbumElement;

public record AlbumElementPatchDto(string? Name, string? Description, IFormFile? Photo);