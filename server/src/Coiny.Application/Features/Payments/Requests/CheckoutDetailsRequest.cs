using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Payments.Requests;

public record CheckoutDetailsRequest(
    Guid LotId,
    string RecipientCityRef,
    string RecipientCityLabel,
    string RecipientWarehouseRef,
    string RecipientWarehouseLabel,
    string RecipientName,
    string RecipientPhone) : IRequest<Result>;
