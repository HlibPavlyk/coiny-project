using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using MediatR;

namespace Coiny.Application.Features.Auth.Requests;

public record RegisterRequest(string Email, string Password, string? DisplayName)
    : IRequest<Result<LoginSuccessModel>>;
