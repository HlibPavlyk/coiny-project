using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Auth.Requests;

public record LogoutRequest : IRequest<Result>;
