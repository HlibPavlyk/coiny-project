namespace Coiny.Application.Abstractions.Http;

/// <summary>Read-only access to the authenticated caller's identity, sourced from HttpContext.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
}
