namespace CoinyProject.Application.Features.Identity.Models;

public record RegisterModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
