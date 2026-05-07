using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using MediatR;

namespace Coiny.Application.Features.Auth.Requests;

public record LoginRequest(string Email, string Password) : IRequest<Result<LoginSuccessModel>>;
