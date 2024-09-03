
using System.ComponentModel.DataAnnotations;

namespace CoinyProject.Application.DTO.Auth;

public sealed class RegisterDto
{
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username must be a single word without spaces.")]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; }
}