using System.Security.Cryptography;
using CoinyProject.Application.Abstractions.DataServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Infrastructure.DataService;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _imageDirectory = "uploads";

    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null");
        }

        var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, _imageDirectory);

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        Guid fileGuid;
        using (var sha256 = SHA256.Create())
        {
            await using (var fileStream = file.OpenReadStream())
            {
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                fileGuid = new Guid(hashBytes.Take(16).ToArray());
            }
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = fileGuid + fileExtension;
        var filePath = Path.Combine(uploadPath, fileName);

        if (File.Exists(filePath))
        {
            return fileName;
        }

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public string GetImageUrl(string fileName)
    {
        return $"https://localhost:7218/photos/{fileName}";
    }

    public async Task<byte[]> DownloadImageAsync(string fileName)
    {
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, _imageDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", fileName);
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream",
        };
    }
}