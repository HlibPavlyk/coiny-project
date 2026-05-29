namespace Coiny.Application.Abstractions.ExternalServices.Files;

/// <summary>
/// Image storage abstraction. Concrete implementation is R2FileService in Infrastructure.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Resizes (max 1920px long edge), encodes as JPEG q=85, and uploads to R2.
    /// <paramref name="keyPrefix"/> determines the R2 path prefix, e.g. "lots/{lotId}".
    /// Accepts JPEG, PNG, and WebP inputs ≤ 10 MB.
    /// </summary>
    Task<UploadedImage> UploadImageAsync(
        Stream imageStream,
        string contentType,
        string keyPrefix,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes the object identified by its R2 storage key. Idempotent.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
