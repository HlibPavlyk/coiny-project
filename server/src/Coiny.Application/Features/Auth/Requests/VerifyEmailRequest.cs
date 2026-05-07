using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Auth.Requests;

public record VerifyEmailRequest(string Token) : IRequest<Result>;
