using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Auth.Handlers;

public class MeHandler(
    IApplicationDbContext db,
    IIdentityService identityService,
    ICurrentUserService currentUser)
    : IRequestHandler<MeRequest, Result<MeModel>>
{
    public async Task<Result<MeModel>> Handle(MeRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<MeModel>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result.Failure<MeModel>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        IList<string> roles = await identityService.GetRolesAsync(user);

        return Result.Success(new MeModel(
            user.Id,
            user.Email!,
            user.EmailVerified,
            user.DisplayName,
            user.TrustScore,
            user.IsBanned,
            user.StripeOnboarded,
            [.. roles]));
    }
}
