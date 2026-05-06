namespace Coiny.Application.Abstractions.Files;

/// <summary>
/// Image storage abstraction. Concrete implementation is R2FileService in Infrastructure.
/// The returned public URL is stable and can be cached indefinitely.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Resizes, encodes, and uploads an image. Returns the public URL.
    /// <paramref name="keyPrefix"/> determines the R2 path prefix, e.g. "lots/{lotId}".
    /// </summary>
    Task<string> UploadImageAsync(
        Stream imageStream,
        string contentType,
        string keyPrefix,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes the object identified by its R2 storage key.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
