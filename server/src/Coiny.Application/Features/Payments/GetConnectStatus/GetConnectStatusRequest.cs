using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.GetConnectStatus;

public record GetConnectStatusRequest : IRequest<Result<ConnectStatusResponse>>;
