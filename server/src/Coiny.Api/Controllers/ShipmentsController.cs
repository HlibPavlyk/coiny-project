using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.GetNpWarehouses;
using Coiny.Application.Features.Shipments.GetShipmentByPaymentId;
using Coiny.Application.Features.Shipments.SearchNpCities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Shipments")]
[Route("api/v1/shipments")]
[Authorize]
public class ShipmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Settlement autocomplete for the buyer's checkout-details form. Backed by Nova Poshta's
    /// <c>Address/searchSettlements</c> (vendor detail — the route name stays neutral).
    /// Empty <c>q</c> returns an empty list (no client-side error).
    /// </summary>
    [HttpGet("cities/search")]
    public Task<Result<NpCitiesResponse>> SearchCities([FromQuery] string? q, CancellationToken ct) =>
        mediator.Send(new SearchNpCitiesRequest(q ?? string.Empty), ct);

    /// <summary>
    /// Warehouses for a given city ref (obtained from <see cref="SearchCities"/>).
    /// Backed by Nova Poshta's <c>AddressGeneral/getWarehouses</c>.
    /// </summary>
    [HttpGet("warehouses")]
    public Task<Result<NpWarehousesResponse>> GetWarehouses([FromQuery] string? cityRef, CancellationToken ct) =>
        mediator.Send(new GetNpWarehousesRequest(cityRef ?? string.Empty), ct);

    /// <summary>
    /// The shipment for a payment (1:1), with the full Nova Poshta status timeline. Buyer or seller of
    /// the payment only. 404 until the shipment is created at checkout-details. Nested under the payment
    /// for identity (1:1), but tagged Shipments — it's the fulfillment/tracking capability.
    /// </summary>
    [HttpGet("/api/v1/payments/{paymentId:guid}/shipment")]
    public Task<Result<ShipmentDetailModel>> GetByPaymentId(Guid paymentId, CancellationToken ct) =>
        mediator.Send(new GetShipmentByPaymentIdRequest(paymentId), ct);
}
