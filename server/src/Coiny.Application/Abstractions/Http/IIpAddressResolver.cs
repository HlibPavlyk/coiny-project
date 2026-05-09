namespace Coiny.Application.Abstractions.Http;

/// <summary>
/// Resolves the caller's client IP for rate-limiting and audit. Concrete impl reads
/// <c>X-Forwarded-For</c> when behind a Coolify reverse proxy; otherwise the direct connection peer.
/// </summary>
public interface IIpAddressResolver
{
    /// <summary>Returns the resolved client IP, or <c>null</c> when unavailable.</summary>
    string? Resolve();
}
