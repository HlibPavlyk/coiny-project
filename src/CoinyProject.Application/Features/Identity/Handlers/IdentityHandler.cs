using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Identity.Models;
using CoinyProject.Application.Features.Identity.Requests;
using MediatR;

namespace CoinyProject.Application.Features.Identity.Handlers;

public class IdentityHandler(IIdentityService identityService, ITokenGenerator tokenGenerator) :
    IRequestHandler<LoginRequest, Result<LoginResponseModel>>,
    IRequestHandler<RegisterRequest, Result<Guid>>
{
    public async Task<Result<LoginResponseModel>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailOrUsernameAsync(request.EmailOrUsername);
        if (user is null)
            return Result.Failure<LoginResponseModel>(Error.Unauthorized("Invalid credentials"));

        if (!await identityService.ValidatePasswordAsync(user, request.Password))
            return Result.Failure<LoginResponseModel>(Error.Unauthorized("Invalid credentials"));

        if (user.Email is null || user.UserName is null)
            return Result.Failure<LoginResponseModel>(Error.Validation("User data is incomplete"));

        var roles = await identityService.GetRolesAsync(user);
        var token = tokenGenerator.Generate(user.Id, user.UserName, user.Email, roles);

        return new LoginResponseModel
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Roles = roles.ToArray(),
            Token = token
        };
    }

    public async Task<Result<Guid>> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var userResult = await identityService.CreateUserAsync(request.Username, request.Email, request.Password);
        if (userResult.IsFailure)
            return Result.Failure<Guid>(userResult.Error);

        var roleResult = await identityService.AssignRoleAsync(userResult.Value, "User");
        if (roleResult.IsFailure)
            return Result.Failure<Guid>(roleResult.Error);

        return userResult.Value.Id;
    }
}
