using System.Security.Authentication;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.DTO.Auth;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using OutOfOfficeApp.Application;
using OutOfOfficeApp.Application.Services.Interfaces;

namespace CoinyProject.Application.AlbumServices.Services;

public class AuthService(UserManager<User> userManager, ITokenService tokenService, IUnitOfWork unitOfWork) : IAuthService
{
    private const string defaultPassword = "password";
    private const string defaultEmailDomain = "@example.com";

    public async Task CreateUserByEmployee(RegisterDto registerDto)
    {
        var email = registerDto.FullName.ToLower().Replace(" ", "") + defaultEmailDomain;
        var user = new User
        {
            UserName = email,
            Email = email,
            EmployeeId = registerDto.EmployeeId
        };

        var result = await userManager.CreateAsync(user, defaultPassword);

        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, registerDto.Position.ToString());
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
            var relatedEmployee = await unitOfWork.Employees.GetByIdAsync(identityUser.EmployeeId);
            if (relatedEmployee == null || relatedEmployee.Status == ActiveStatus.Inactive)
            {
                throw new AuthenticationException("User is inactive");
            }

            var result = await userManager.CheckPasswordAsync(identityUser, login.Password);

            if (result)
            {
                var roles = await userManager.GetRolesAsync(identityUser);


                if (identityUser.Email != null)
                {
                    var jwtToken = tokenService.CreateToken(identityUser.Email, roles);
                
                    return new LoginResponseDto
                    {
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