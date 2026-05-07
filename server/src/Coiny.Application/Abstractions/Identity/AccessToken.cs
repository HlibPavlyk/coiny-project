namespace Coiny.Application.Abstractions.Identity;

/// <summary>An issued JWT access token paired with its absolute expiration timestamp (UTC).</summary>
public record AccessToken(string Token, DateTime ExpiresAt);
