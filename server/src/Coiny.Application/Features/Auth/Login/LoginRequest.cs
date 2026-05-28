using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Shared;
using MediatR;

namespace Coiny.Application.Features.Auth.Login;

public record LoginRequest(string Email, string Password) : IRequest<Result<LoginSuccessModel>>;
