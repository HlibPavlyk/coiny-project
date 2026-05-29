namespace Coiny.Application.Features.Auth.Shared;

/// <summary>Response shape for register/login/me endpoints. Mirrors /docs/02-api-contracts.md §1.</summary>
public record MeModel(
    Guid Id,
    string Email,
    bool EmailVerified,
    string DisplayName,
    int TrustScore,
    bool IsBanned,
    bool StripeOnboarded,
    IReadOnlyList<string> Roles);
