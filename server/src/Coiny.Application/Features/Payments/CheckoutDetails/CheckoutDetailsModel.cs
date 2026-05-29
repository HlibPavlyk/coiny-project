namespace Coiny.Application.Features.Payments.CheckoutDetails;

/// <summary>
/// Wire body for <c>POST /api/v1/lots/{lotId}/checkout-details</c>. The lot id is bound from the
/// route, not the body — see <see cref="CheckoutDetailsRequest"/> which composes this with the route id.
/// </summary>
public record CheckoutDetailsModel(
    string RecipientCityRef,
    string RecipientCityLabel,
    string RecipientWarehouseRef,
    string RecipientWarehouseLabel,
    string RecipientName,
    string RecipientPhone);
