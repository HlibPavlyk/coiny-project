using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Config")]
[Route("api/v1/config")]
public class ConfigController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Anonymous bootstrap config consumed by the SPA on first load:
    /// Stripe publishable key, SignalR hub URL, current UAH→USD display rate.
    /// </summary>
    [AllowAnonymous, HttpGet("public")]
    public Task<Result<PublicConfigModel>> GetPublic(CancellationToken ct) =>
        mediator.Send(new GetPublicConfigRequest(), ct);
}
