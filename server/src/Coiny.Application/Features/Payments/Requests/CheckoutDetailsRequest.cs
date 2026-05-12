using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.Requests;

public record CheckoutDetailsRequest(
    Guid LotId,
    string RecipientCityRef,
    string RecipientWarehouseRef,
    string RecipientName,
    string RecipientPhone) : IRequest<Result>;
