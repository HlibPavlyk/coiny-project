using Coiny.Domain.Entities;

namespace Coiny.Application.Abstractions.Identity;

/// <summary>Issues signed JWT access tokens for authenticated users. Concrete impl lives in Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    AccessToken IssueToken(User user, IList<string> roles);
}
