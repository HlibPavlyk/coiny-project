using Coiny.Application.Abstractions.Http;

namespace Coiny.Application.Common.Authorization;

/// <summary>Role-check helpers for the defense-in-depth guards inside handlers.</summary>
public static class CurrentUserRoleExtensions
{
    /// <summary>True if the caller holds at least one of the given roles.</summary>
    public static bool IsInAnyRole(this ICurrentUserService currentUser, params string[] roles) =>
        roles.Any(currentUser.Roles.Contains);

    /// <summary>True if the caller may moderate (Moderator or Admin).</summary>
    public static bool CanModerate(this ICurrentUserService currentUser) =>
        currentUser.IsInAnyRole(RoleNames.Moderator, RoleNames.Admin);
}
