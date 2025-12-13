
using System.ComponentModel.DataAnnotations;

namespace CoinyProject.Application.DTO.Auth;

public sealed class RegisterDto
{
    public string Username { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
}