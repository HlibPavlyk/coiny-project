using CoinyProject.Application.Common.Models;

namespace CoinyProject.Application.Abstractions.Data;

public interface IFileService
{
    Task<string> SaveImageAsync(FileStreamDataModel file);
    Task DeleteImageAsync(string fileName);
    Uri GetImageUrl(string fileName);
    string GetMimeType(string fileName);
    Uri GetStaticFileUrl(string filePath);
}