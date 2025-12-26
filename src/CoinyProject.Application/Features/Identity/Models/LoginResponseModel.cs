namespace CoinyProject.Application.Features.Identity.Models;

public class LoginResponseModel
{
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public string[] Roles { get; init; } = [];
    public string Token { get; init; }
}
