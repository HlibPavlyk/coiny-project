using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.CheckoutDetails;

public record CheckoutDetailsRequest(Guid LotId, CheckoutDetailsModel Model) : IRequest<Result>;
