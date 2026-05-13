using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.Models;
using Coiny.Application.Features.Shipments.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Shipments")]
[Route("api/v1/shipments")]
public class ShipmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Settlement autocomplete for the buyer's checkout-details form. Backed by Nova Poshta's
    /// <c>Address/searchSettlements</c> (vendor detail — the route name stays neutral).
    /// Empty <c>q</c> returns an empty list (no client-side error).
    /// </summary>
    [Authorize, HttpGet("cities/search")]
    public Task<Result<NpCitiesResponse>> SearchCities([FromQuery] string q, CancellationToken ct) =>
        mediator.Send(new SearchNpCitiesRequest(q ?? string.Empty), ct);

    /// <summary>
    /// Warehouses for a given city ref (obtained from <see cref="SearchCities"/>).
    /// Backed by Nova Poshta's <c>AddressGeneral/getWarehouses</c>.
    /// </summary>
    [Authorize, HttpGet("warehouses")]
    public Task<Result<NpWarehousesResponse>> GetWarehouses([FromQuery] string cityRef, CancellationToken ct) =>
        mediator.Send(new GetNpWarehousesRequest(cityRef ?? string.Empty), ct);

    /// <summary>
    /// Buyer or seller view of the shipment linked to a payment, with the full NP status timeline.
    /// </summary>
    [Authorize, HttpGet("{paymentId:guid}")]
    public Task<Result<ShipmentDetailModel>> GetByPaymentId(Guid paymentId, CancellationToken ct) =>
        mediator.Send(new GetShipmentByPaymentIdRequest(paymentId), ct);
}
