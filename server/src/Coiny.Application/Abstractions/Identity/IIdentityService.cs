using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;

namespace Coiny.Application.Abstractions.Identity;

/// <summary>Identity operations exposed to Application handlers. Concrete implementation is in Infrastructure.</summary>
public interface IIdentityService
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(User user, string password);

    Task<IList<string>> GetRolesAsync(User user);

    Task<Result<User>> CreateAsync(User user, string password, CancellationToken cancellationToken = default);

    Task<Result> AssignRoleAsync(User user, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an existing user by email or creates one from Google OIDC claims.
    /// Returns (user, isNewUser). Concrete impl handles the "match by email" strategy (THESIS-SCOPE.md §3).
    /// </summary>
    Task<Result<(User User, bool IsNew)>> FindOrCreateGoogleUserAsync(
        string email,
        string googleSubject,
        string displayName,
        CancellationToken cancellationToken = default);
}
