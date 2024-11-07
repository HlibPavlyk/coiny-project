using CoinyProject.Application.DTO.Auth;

namespace CoinyProject.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Guid> RegisterUserAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto login);
}
