namespace CoinyProject.Application.Features.Identity.Models;

public record LoginModel
{
    public string EmailOrUsername { get; set; }
    public string Password { get; set; }
}