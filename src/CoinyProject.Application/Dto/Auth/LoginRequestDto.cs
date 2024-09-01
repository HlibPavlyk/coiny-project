namespace CoinyProject.Application.DTO.Auth;

public sealed class LoginRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}