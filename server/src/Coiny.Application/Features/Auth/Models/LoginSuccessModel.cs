using Coiny.Application.Abstractions.Infrastructure.Identity;

namespace Coiny.Application.Features.Auth.Models;

/// <summary>
/// Internal handler-output for successful login. The controller (task 14) uses
/// <see cref="Token"/> to set the auth cookie and surfaces only <see cref="Me"/> to the wire.
/// </summary>
public record LoginSuccessModel(AccessToken Token, MeModel Me);
