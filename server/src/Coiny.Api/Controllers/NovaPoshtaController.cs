using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.Models;
using Coiny.Application.Features.Shipments.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Nova Poshta")]
[Route("api/v1/np")]
public class NovaPoshtaController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Settlement autocomplete for the buyer's checkout-details form. Empty <c>q</c>
    /// returns an empty list (no client-side error). Backed by NP's <c>Address/searchSettlements</c>.
    /// </summary>
    [Authorize, HttpGet("cities/search")]
    public Task<Result<NpCitiesResponse>> SearchCities([FromQuery] string? q, CancellationToken ct) =>
        mediator.Send(new SearchNpCitiesRequest(q ?? string.Empty), ct);

    /// <summary>
    /// Warehouses for a given city <c>cityRef</c> (obtained from <see cref="SearchCities"/>).
    /// Backed by NP's <c>AddressGeneral/getWarehouses</c>.
    /// </summary>
    [Authorize, HttpGet("warehouses")]
    public Task<Result<NpWarehousesResponse>> GetWarehouses([FromQuery] string? cityRef, CancellationToken ct) =>
        mediator.Send(new GetNpWarehousesRequest(cityRef ?? string.Empty), ct);
}
