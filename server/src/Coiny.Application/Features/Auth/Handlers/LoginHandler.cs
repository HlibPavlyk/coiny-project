using Coiny.Application.Abstractions.Infrastructure.Identity;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Domain.Entities;
using MediatR;

namespace Coiny.Application.Features.Auth.Handlers;

public class LoginHandler(
    IIdentityService identityService,
    IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<LoginRequest, Result<LoginSuccessModel>>
{
    public async Task<Result<LoginSuccessModel>> Handle(LoginRequest request, CancellationToken ct)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        User? user = await identityService.FindByEmailAsync(normalizedEmail, ct);
        if (user is null)
            return Result.Failure<LoginSuccessModel>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        bool passwordOk = await identityService.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return Result.Failure<LoginSuccessModel>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        if (user.IsBanned)
            return Result.Failure<LoginSuccessModel>(
                Error.Forbidden("Auth.Banned", "This account is banned."));

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
