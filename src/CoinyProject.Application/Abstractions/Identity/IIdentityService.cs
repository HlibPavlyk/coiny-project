using CoinyProject.Application.Common.Results;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Identity;

public interface IIdentityService
{
    Result<Guid> GetCurrentUserId();
    Task<User> FindByEmailOrUsernameAsync(string emailOrUsername);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<IList<string>> GetRolesAsync(User user);
    Task<Result<User>> CreateUserAsync(string username, string email, string password);
    Task<Result> AssignRoleAsync(User user, string role);
}
