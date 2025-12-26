using System.Security.Claims;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.Results;
using CoinyProject.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace CoinyProject.Infrastructure.Services.Identity;

public class IdentityService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor) : IIdentityService
{
    public Result<Guid> GetCurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is not { Identity.IsAuthenticated: true })
            return Result.Failure<Guid>(Error.Unauthorized("User is not authenticated"));

        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Result.Failure<Guid>(Error.Unauthorized("Invalid user id"));

        return userId;
    }

    public async Task<User> FindByEmailOrUsernameAsync(string emailOrUsername)
    {
        return await userManager.FindByEmailAsync(emailOrUsername)
               ?? await userManager.FindByNameAsync(emailOrUsername);
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        return await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<Result<User>> CreateUserAsync(string username, string email, string password)
    {
        var user = new User(username.Trim(), email.ToLower());
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return Result.Failure<User>(Error.Validation(string.Join(", ", result.Errors.Select(e => e.Description))));

        return user;
    }

    public async Task<Result> AssignRoleAsync(User user, string role)
    {
        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return Result.Failure(Error.Validation("Failed to assign role"));

        return Result.Success();
    }
}
