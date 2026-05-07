using Amazon.S3;
using Amazon.S3.Model;
using Coiny.Application.Abstractions.Files;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Coiny.Infrastructure.Files;

public class R2FileService(IAmazonS3 s3, IOptions<R2Options> options) : IFileService
{
    private const long MaxBytes = 10L * 1024 * 1024;
    private const int MaxLongEdge = 1920;
    private const int JpegQuality = 85;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
    };

    public async Task<UploadedImage> UploadImageAsync(
        Stream imageStream,
        string contentType,
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException($"Unsupported content type: {contentType}.", nameof(contentType));

        if (imageStream is { CanSeek: true, Length: > MaxBytes })
            throw new ArgumentOutOfRangeException(nameof(imageStream), "Image exceeds the 10 MB upload limit.");

        using Image image = await Image.LoadAsync(imageStream, cancellationToken);

        image.Mutate(x => x
            .AutoOrient()
            .Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxLongEdge, MaxLongEdge),
            }));

        await using MemoryStream encoded = new();
        await image.SaveAsJpegAsync(encoded, new JpegEncoder { Quality = JpegQuality }, cancellationToken);
        encoded.Position = 0;

        var key = $"{keyPrefix.Trim('/')}/{Guid.NewGuid():N}.jpg";

        PutObjectRequest putRequest = new()
        {
            BucketName = options.Value.BucketName,
            Key = key,
            InputStream = encoded,
            ContentType = "image/jpeg",
        };
        putRequest.Headers.CacheControl = "public, max-age=31536000, immutable";

        await s3.PutObjectAsync(putRequest, cancellationToken);

        string publicUrl = $"{options.Value.PublicUrlBase.TrimEnd('/')}/{key}";

        return new UploadedImage(
            StorageKey: key,
            PublicUrl: publicUrl,
            Width: image.Width,
            Height: image.Height,
            SizeBytes: (int)encoded.Length);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = options.Value.BucketName,
            Key = storageKey,
        }, cancellationToken);
    }
}
