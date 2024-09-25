namespace CoinyProject.Application.DTO.Auth;

public sealed class LoginRequestDto
{
    public string EmailOrUsername { get; set; }
    public string Password { get; set; }
}