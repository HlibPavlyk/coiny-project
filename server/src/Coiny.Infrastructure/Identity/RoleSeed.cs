using Coiny.Domain.Identity;

namespace Coiny.Infrastructure.Identity;

/// <summary>Stable role seeds — IDs must never change once migrated.</summary>
public static class RoleSeed
{
    public static readonly ApplicationRole UserRole = new()
    {
        Id = new Guid("00000000-0000-0000-0000-000000000001"),
        Name = "User",
        NormalizedName = "USER",
        ConcurrencyStamp = "00000000-0000-0000-0000-000000000001"
    };

    public static readonly ApplicationRole ModeratorRole = new()
    {
        Id = new Guid("00000000-0000-0000-0000-000000000002"),
        Name = "Moderator",
        NormalizedName = "MODERATOR",
        ConcurrencyStamp = "00000000-0000-0000-0000-000000000002"
    };

    public static readonly ApplicationRole AdminRole = new()
    {
        Id = new Guid("00000000-0000-0000-0000-000000000003"),
        Name = "Admin",
        NormalizedName = "ADMIN",
        ConcurrencyStamp = "00000000-0000-0000-0000-000000000003"
    };

    public static readonly ApplicationRole[] All = [UserRole, ModeratorRole, AdminRole];
}
