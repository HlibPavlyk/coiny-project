using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Auth.VerifyEmail;

public record VerifyEmailRequest(string Token) : IRequest<Result>;
