using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;

namespace Coiny.Application.Features.Auth.Handlers;

public class GoogleSignInHandler(
    IIdentityService identityService,
    IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<GoogleSignInRequest, Result<LoginSuccessModel>>
{
    private const string DefaultRole = "User";

    public async Task<Result<LoginSuccessModel>> Handle(GoogleSignInRequest request, CancellationToken ct)
    {
        if (!request.EmailVerified)
            return Result.Failure<LoginSuccessModel>(Error.Unauthorized(
                "Auth.GoogleEmailNotVerified",
                "Google account email is not verified."));

        Result<(User User, bool IsNew)> findOrCreate = await identityService.FindOrCreateGoogleUserAsync(
            request.Email, request.Sub, request.Name, ct);

        if (findOrCreate.IsFailure)
            return Result.Failure<LoginSuccessModel>(findOrCreate.Error);

        (User user, bool isNew) = findOrCreate.Value;

        if (user.IsBanned)
            return Result.Failure<LoginSuccessModel>(
                Error.Forbidden("Auth.Banned", "This account is banned."));

        if (isNew)
        {
            Result assignResult = await identityService.AssignRoleAsync(user, DefaultRole, ct);
            if (assignResult.IsFailure)
                return Result.Failure<LoginSuccessModel>(assignResult.Error);
        }

        IList<string> roles = await identityService.GetRolesAsync(user);
        AccessToken accessToken = tokenGenerator.IssueToken(user, roles);

        MeModel me = new(
            user.Id,
            user.Email!,
            user.EmailVerified,
            user.DisplayName,
            user.TrustScore,
            user.IsBanned,
            user.StripeOnboarded,
            [.. roles]);

        return Result.Success(new LoginSuccessModel(accessToken, me));
    }
}
