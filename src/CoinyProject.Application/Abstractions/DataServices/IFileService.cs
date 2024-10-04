using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Abstractions.DataServices;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile file);
    string GetImageUrl(string fileName);
    string GetMimeType(string fileName);
}