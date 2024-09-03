using CoinyProject.Application.DTO.Auth;
using OutOfOfficeApp.Application;

namespace CoinyProject.Application.Abstractions.Services;

public interface IAuthService
{
    Task RegisterUserAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto login);
}
