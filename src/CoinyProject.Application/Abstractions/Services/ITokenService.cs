namespace CoinyProject.Application.Abstractions.Services;

public interface ITokenService
{
    string CreateToken(Guid id, string username, string email, IEnumerable<string> roles);
}