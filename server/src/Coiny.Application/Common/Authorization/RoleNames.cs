namespace Coiny.Application.Common.Authorization;

/// <summary>
/// Canonical role names, mirroring the seeds in <c>Coiny.Infrastructure.Identity.RoleSeed</c>.
/// Shared by controller <c>[Authorize(Roles = …)]</c> attributes and handler defense-in-depth checks
/// so the magic string lives in exactly one place.
/// </summary>
public static class RoleNames
{
    public const string User = "User";
    public const string Moderator = "Moderator";
    public const string Admin = "Admin";
}
