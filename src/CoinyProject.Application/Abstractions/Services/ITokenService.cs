namespace OutOfOfficeApp.Application.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(string email, IEnumerable<string> roles);
}