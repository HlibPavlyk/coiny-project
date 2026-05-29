using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Demo;
using Coiny.Application.Features.Demo.CancelUnpaidNow;
using Coiny.Application.Features.Demo.CloseLotNow;
using Coiny.Application.Features.Demo.ForceShipmentDelivered;
using Coiny.Application.Features.Demo.ForceShipmentReturned;
using Coiny.Application.Features.Demo.SendPaymentReminderNow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Coiny.Api.Controllers;

/// <summary>
/// Demo-only control surface that short-circuits the time-based triggers of the production
/// workflow so a thesis defense can show the full lifecycle in minutes instead of days. Every
/// endpoint is keyed by lot id — after Option A there is exactly one Payment and one Shipment
/// per lot, so a single id is the natural handle for the operator.
///
/// <para>
/// Gated by <c>[Authorize(Roles = RoleNames.Admin)]</c> AND a feature flag
/// (<see cref="DemoModeOptions.Enabled"/>): when the flag is off, all endpoints respond with 404
/// even to authenticated admins.
/// </para>
/// </summary>
[ApiController]
[Route("api/v1/demo/lots/{lotId:guid}")]
[Tags("Demo")]
[Authorize(Roles = RoleNames.Admin)]
public class DemoController(IMediator mediator, IOptions<DemoModeOptions> demoMode) : ControllerBase
{
    /// <summary>
    /// Force-close an active auction immediately (mutates EndsAt → past). Needed even with
    /// 1-minute auctions because the 5-min AntiSnipeWindow in PlaceBidHandler extends EndsAt on
    /// every bid placed within the window — a short demo auction with any bid cannot end naturally
    /// within the demo timeframe.
    /// </summary>
    [HttpPost("close-now")]
    public Task<Result> CloseLotNow(Guid lotId, CancellationToken ct) =>
        Gate(() => mediator.Send(new CloseLotNowRequest(lotId), ct));

    /// <summary>Force-emit the 48h won-pay reminder for this lot's payment.</summary>
    [HttpPost("send-reminder")]
    public Task<Result> SendReminder(Guid lotId, CancellationToken ct) =>
        Gate(() => mediator.Send(new SendPaymentReminderNowRequest(lotId), ct));

    /// <summary>Force the 96h non-payment cancel path (Stripe void + lot → EndedNoSale + −10 TrustScore).</summary>
    [HttpPost("cancel-unpaid")]
    public Task<Result> CancelUnpaid(Guid lotId, CancellationToken ct) =>
        Gate(() => mediator.Send(new CancelUnpaidNowRequest(lotId), ct));

    /// <summary>Simulate NP "Delivered" — flips shipment + enqueues Stripe capture.</summary>
    [HttpPost("force-delivered")]
    public Task<Result> ForceDelivered(Guid lotId, CancellationToken ct) =>
        Gate(() => mediator.Send(new ForceShipmentDeliveredRequest(lotId), ct));

    /// <summary>Simulate NP "Returned" — flips shipment + enqueues Stripe void.</summary>
    [HttpPost("force-returned")]
    public Task<Result> ForceReturned(Guid lotId, CancellationToken ct) =>
        Gate(() => mediator.Send(new ForceShipmentReturnedRequest(lotId), ct));

    /// <summary>
    /// Per-endpoint feature-flag gate. Returns NotFound when demo mode is off so the surface looks
    /// non-existent — defence-in-depth against accidental enablement in production.
    /// </summary>
    private Task<Result> Gate(Func<Task<Result>> action) =>
        demoMode.Value.Enabled
            ? action()
            : Task.FromResult(Result.Failure(Error.NotFound("Demo.Disabled", "Demo mode is not enabled.")));
}
