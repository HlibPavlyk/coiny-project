using CoinyProject.Application.Dto.User;

namespace CoinyProject.Application.Abstractions.Services;

public interface IUserService
{
    Task<UserStatsGetDto> GetUserStatsAsync(Guid userId);
    Task<UserStatsGetDto> GetCurrentUserStatsAsync();
    Task<UserNameGetDto> GetUserNameAsync(Guid userId);
}