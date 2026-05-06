using Microsoft.AspNetCore.Identity;

namespace Coiny.Domain.Entities;

public class User : IdentityUser<Guid>
{
    /// <summary>Flipped by the email-verification flow, or set true on first Google sign-in with email_verified=true.</summary>
    public bool EmailVerified { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? FullName { get; set; }

    /// <summary>Numeric trust score — no behavioral gates in this build (THESIS-SCOPE.md §9).</summary>
    public int TrustScore { get; set; }

    public bool IsBanned { get; set; }

    public DateTime? BannedAt { get; set; }

    public Guid? BannedByUserId { get; set; }

    public string? BanReason { get; set; }

    /// <summary>Stripe Connect account ID (acct_…). Null until Connect onboarding starts.</summary>
    public string? StripeAccountId { get; set; }

    /// <summary>True only after account.updated webhook reports details_submitted=true and charges_enabled=true.</summary>
    public bool StripeOnboarded { get; set; }

    /// <summary>OIDC sub claim from Google. Unique when not null (enforced by partial index).</summary>
    public string? GoogleSubject { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
