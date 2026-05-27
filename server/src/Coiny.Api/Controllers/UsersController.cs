using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Auth.Models;
using Coiny.Application.Features.Auth.Requests;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Requests;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Application.Features.Users.Models;
using Coiny.Application.Features.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Tags("Profile")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>The authenticated caller's own profile.</summary>
    [HttpGet("me")]
    public Task<Result<MeModel>> Me(CancellationToken ct) =>
        mediator.Send(new MeRequest(), ct);

    /// <summary>Caller's own lots in any status (soft-deleted included with deletedAt).</summary>
    [HttpPost("me/lots/list")]
    public Task<Result<Paginated<MyLotItemModel>>> MyLots([FromBody] GetMyLotsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Caller's own bid history with each bid's lot state and a leader flag.</summary>
    [HttpPost("me/bids/list")]
    public Task<Result<Paginated<MyBidItemModel>>> MyBids([FromBody] GetMyBidsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Caller's purchase history (payments where caller is the buyer) with lot + shipment.</summary>
    [HttpPost("me/payments/list")]
    public Task<Result<Paginated<MyPurchaseItemModel>>> MyPurchases([FromBody] GetMyPurchasesRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>
    /// Public read-only seller profile. No auth required. Returns hero (displayName, trust score,
    /// member-since) and aggregate stats (lots sold, active lots, average sale price). Banned users
    /// surface as 404 to avoid leaking moderation state.
    /// </summary>
    [AllowAnonymous, HttpGet("{id:guid}/public")]
    public Task<Result<PublicProfileModel>> GetPublic(Guid id, CancellationToken ct) =>
        mediator.Send(new GetPublicProfileRequest(id), ct);
}
