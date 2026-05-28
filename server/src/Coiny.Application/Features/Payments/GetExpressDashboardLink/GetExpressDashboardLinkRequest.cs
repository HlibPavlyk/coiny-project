using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.GetExpressDashboardLink;

public record GetExpressDashboardLinkRequest : IRequest<Result<ExpressDashboardLinkResponse>>;
