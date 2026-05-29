namespace Coiny.Application.Abstractions.ExternalServices.Files;

/// <summary>Metadata returned by <see cref="IFileService.UploadImageAsync"/> after a successful upload.</summary>
public record UploadedImage(
    string StorageKey,
    string PublicUrl,
    int Width,
    int Height,
    int SizeBytes);
