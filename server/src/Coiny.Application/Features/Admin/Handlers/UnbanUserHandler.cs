using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Admin.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Admin.Handlers;

/// <summary>
/// Lifts a ban: clears <c>IsBanned</c> and the ban metadata. Lots cancelled and payments cancelled by
/// the original ban are terminal and are NOT restored. Unbanning a user who is not banned is a no-op
/// success (idempotent).
/// </summary>
public class UnbanUserHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<UnbanUserRequest, Result>
{
    public async Task<Result> Handle(UnbanUserRequest request, CancellationToken ct)
    {
        if (!currentUser.CanModerate())
            return Result.Failure(Error.Forbidden("Admin.Forbidden", "Moderator or Administrator role required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));

        if (!user.IsBanned)
            return Result.Success();

        user.IsBanned = false;
        user.BannedAt = null;
        user.BannedByUserId = null;
        user.BanReason = null;
        user.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
