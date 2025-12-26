namespace CoinyProject.Application.Abstractions.Identity;

public interface ITokenGenerator
{
    string Generate(Guid userId, string username, string email, IEnumerable<string> roles);
}
