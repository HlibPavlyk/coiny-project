using Coiny.Application.Abstractions.Infrastructure.Identity;
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
        string displayName,
        CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        User? existing = await userManager.FindByEmailAsync(normalizedEmail);

        if (existing is not null)
        {
            if (existing.GoogleSubject is null)
            {
                existing.GoogleSubject = googleSubject;
                existing.UpdatedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(existing);
            }
            return Result.Success<(User, bool)>((existing, false));
        }

        DateTime now = DateTime.UtcNow;
        var newUser = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailVerified = true,
            GoogleSubject = googleSubject,
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? normalizedEmail.Split('@')[0]
                : displayName.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
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
