using System.Security.Cryptography;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Infrastructure.Services.Data;

public class FileService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : IFileService
{
    private const string ImageDirectory = "uploads";

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    public async Task<string> SaveImageAsync(FileStreamDataModel file)
    {
        if (file?.FileStream is null || file.FileStream.Length == 0)
            throw new ArgumentException("File stream is empty or null", nameof(file));

        var fileExtension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"File extension '{fileExtension}' is not allowed", nameof(file));

        var uploadPath = Path.Combine(webHostEnvironment.ContentRootPath, ImageDirectory);
        Directory.CreateDirectory(uploadPath);

        // Compute hash for content-addressable storage (deduplication)
        var fileGuid = await ComputeFileHashAsync(file.FileStream);

        var fileName = $"{fileGuid}{fileExtension}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Skip saving if file already exists (content-addressable deduplication)
        if (File.Exists(filePath))
            return fileName;

        // Reset stream position after hash computation
        if (file.FileStream.CanSeek)
            file.FileStream.Position = 0;

        await using var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.FileStream.CopyToAsync(outputStream);

        return fileName;
    }

    public Task DeleteImageAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Task.CompletedTask;

        var filePath = Path.Combine(webHostEnvironment.ContentRootPath, ImageDirectory, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Uri GetImageUrl(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        return GetStaticFileUrl($"photos/{fileName}");
    }

    public string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream",
        };
    }

    public Uri GetStaticFileUrl(string filePath)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available");

        var request = httpContext.Request;
        return new Uri($"{request.Scheme}://{request.Host}/{filePath}");
    }

    private static async Task<Guid> ComputeFileHashAsync(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        var hashBytes = await SHA256.HashDataAsync(stream);
        return new Guid(hashBytes.AsSpan(0, 16));
    }
}
