namespace Coiny.Application.Abstractions.Infrastructure.Identity;

/// <summary>An issued JWT access token paired with its absolute expiration timestamp (UTC).</summary>
public record AccessToken(string Token, DateTime ExpiresAt);
