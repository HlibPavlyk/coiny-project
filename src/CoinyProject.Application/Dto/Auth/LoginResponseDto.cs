namespace CoinyProject.Application.DTO.Auth;

public sealed class LoginResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public IEnumerable<string> Roles { get; set; }
}