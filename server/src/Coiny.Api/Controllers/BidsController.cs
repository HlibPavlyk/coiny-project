using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Tags("Bids")]
public class BidsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Place a bid on an Active lot. Body is a plain <see cref="PlaceBidBody"/>; the lot id
    /// comes from the route segment.
    /// </summary>
    [Authorize, HttpPost("api/v1/lots/{lotId:guid}/bids")]
    public Task<Result<PlaceBidModel>> PlaceBid(Guid lotId, [FromBody] PlaceBidBody body, CancellationToken ct) =>
        mediator.Send(new PlaceBidRequest(lotId, body.AmountUahKopiykas), ct);

    /// <summary>
    /// Public bid history for a lot. Body is a plain pagination envelope; the route's <c>lotId</c>
    /// wins. Bidder names are anonymized as <c>b****&lt;n&gt;</c> while the lot is Active and surface
    /// as full display names once the lot transitions to Sold / EndedNoSale / Cancelled.
    /// </summary>
    [HttpPost("api/v1/lots/{lotId:guid}/bids/list")]
    public Task<Result<Paginated<BidItemModel>>> ListBidHistory(Guid lotId, [FromBody] PageRequest paginate, CancellationToken ct) =>
        mediator.Send(new GetBidHistoryRequest { LotId = lotId, Paginate = paginate }, ct);

    /// <summary>Caller's own bid history with each bid's lot state and a leader flag.</summary>
    [Authorize, HttpPost("api/v1/bids/mine/list")]
    public Task<Result<Paginated<MyBidItemModel>>> ListMine([FromBody] GetMyBidsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);
}
