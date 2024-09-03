using System.Security.Authentication;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.DTO.Auth;
using CoinyProject.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using OutOfOfficeApp.Application.Services.Interfaces;

namespace CoinyProject.Application.Services;

public class AuthService(UserManager<User> userManager, ITokenService tokenService) : IAuthService
{
    public async Task RegisterUserAsync(RegisterDto registerDto)
    {
        var user = new User(registerDto.Username.ToLower(), registerDto.Email.ToLower());

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, "User");
            if (result.Succeeded)
            {
                return;
            }
        }
        
        throw new AuthenticationException("User creation failed");
    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto login)
    {
        var identityUser = await userManager.FindByEmailAsync(login.Email);

        if (identityUser != null)
        {
            var result = await userManager.CheckPasswordAsync(identityUser, login.Password);

            if (result)
            {
                var roles = await userManager.GetRolesAsync(identityUser);


                if (identityUser.Email != null && identityUser.UserName != null)
                {
                    var jwtToken = tokenService.CreateToken(identityUser.Email, roles);
                
                    return new LoginResponseDto
                    {
                        Username = identityUser.UserName,
                        Email = login.Email,
                        Roles = roles.ToList(),
                        Token = jwtToken
                    };
                }
            }
        }
        throw new AuthenticationException("Invalid email or password");
    }
}