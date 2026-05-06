using Coiny.Application.Abstractions.Identity;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Coiny.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        userManager.FindByEmailAsync(email);

    public Task<bool> CheckPasswordAsync(User user, string password) =>
        userManager.CheckPasswordAsync(user, password);

    public Task<IList<string>> GetRolesAsync(User user) =>
        userManager.GetRolesAsync(user);

    public async Task<Result<User>> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        IdentityResult result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure<User>(Error.Validation("Identity.Create", errors));
        }

        return Result.Success(user);
    }

    public async Task<Result> AssignRoleAsync(User user, string role, CancellationToken cancellationToken = default)
    {
        IdentityResult result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Identity.AssignRole", errors));
        }

        return Result.Success();
    }

    public async Task<Result<(User User, bool IsNew)>> FindOrCreateGoogleUserAsync(
        string email,
        string googleSubject,
        CancellationToken cancellationToken = default)
    {
        User? existing = await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            if (existing.GoogleSubject is null)
            {
                existing.GoogleSubject = googleSubject;
                await userManager.UpdateAsync(existing);
            }
            return Result.Success<(User, bool)>((existing, false));
        }

        var newUser = new User
        {
            UserName = email,
            Email = email,
            EmailVerified = true,
            GoogleSubject = googleSubject,
            DisplayName = email.Split('@')[0],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        IdentityResult createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<(User, bool)>(Error.Validation("Identity.GoogleCreate", errors));
        }

        return Result.Success<(User, bool)>((newUser, true));
    }
}
