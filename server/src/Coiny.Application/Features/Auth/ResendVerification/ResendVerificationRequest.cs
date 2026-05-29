using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Auth.ResendVerification;

public record ResendVerificationRequest : IRequest<Result>;
