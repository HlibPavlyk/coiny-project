using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Shared;
using MediatR;

namespace Coiny.Application.Features.Auth.Register;

public record RegisterRequest(string Email, string Password, string? DisplayName)
    : IRequest<Result<LoginSuccessModel>>;
