namespace Coiny.Domain.Entities;

public class LotImage
{
    public Guid Id { get; set; }

    public Guid LotId { get; set; }

    /// <summary>R2 object key, e.g. lots/{lotId}/{guid}.jpg. Used to build delete requests to R2.</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>Full public URL (R2 public domain + key). Rendered as-is by the frontend.</summary>
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>0-based display order; index 0 is the cover image shown in listings.</summary>
    public int DisplayOrder { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int SizeBytes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Lot? Lot { get; set; }
}
