using CoinyProject.Application.DTO.Auth;
using OutOfOfficeApp.Application;

namespace CoinyProject.Application.Abstractions.Services;

public interface IAuthService
{
    Task CreateUserByEmployee(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto login);
}
