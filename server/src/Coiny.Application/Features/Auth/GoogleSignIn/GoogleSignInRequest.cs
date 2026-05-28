using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Shared;
using MediatR;

namespace Coiny.Application.Features.Auth.GoogleSignIn;

/// <summary>
/// Carried by the controller after the Google OIDC callback completes.
/// Claims are read from the external authentication ticket — never trusted from the wire directly.
/// </summary>
public record GoogleSignInRequest(
    string Sub,
    string Email,
    string Name,
    bool EmailVerified) : IRequest<Result<LoginSuccessModel>>;
