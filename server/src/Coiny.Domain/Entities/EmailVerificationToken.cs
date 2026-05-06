namespace Coiny.Domain.Entities;

public class EmailVerificationToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token sent in the email link. The raw token is never stored.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Null until the token is consumed. Single-use — once set, the token is rejected on re-use.</summary>
    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
