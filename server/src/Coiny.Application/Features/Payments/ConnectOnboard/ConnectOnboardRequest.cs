using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.ConnectOnboard;

public record ConnectOnboardRequest : IRequest<Result<ConnectOnboardResponse>>;
