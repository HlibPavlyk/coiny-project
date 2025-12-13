using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoinyProject.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoinyProject.Application.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string CreateToken(Guid id, string username, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        };
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                                                                  ?? throw new InvalidOperationException("Jwt:Key is missing in appsettings.json")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token); 
    }

}