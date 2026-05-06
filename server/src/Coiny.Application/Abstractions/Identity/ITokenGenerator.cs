using Coiny.Domain.Entities;

namespace Coiny.Application.Abstractions.Identity;

/// <summary>Generates a signed JWT for the given user and roles. Concrete implementation is in Infrastructure.</summary>
public interface ITokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user, IEnumerable<string> roles);
}
