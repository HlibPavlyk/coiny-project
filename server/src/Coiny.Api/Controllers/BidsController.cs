using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.GetBidHistory;
using Coiny.Application.Features.Bids.PlaceBid;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/lots")]
[Tags("Bids")]
public class BidsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Place a bid on an Active lot. Body is a plain <see cref="PlaceBidBody"/>; the lot id
    /// comes from the route segment.
    /// </summary>
    [Authorize, HttpPost("{lotId:guid}/bids")]
    public Task<Result<PlaceBidModel>> PlaceBid(Guid lotId, [FromBody] PlaceBidBody body, CancellationToken ct) =>
        mediator.Send(new PlaceBidRequest(lotId, body.AmountUahKopiykas), ct);

    /// <summary>
    /// Public bid history for a lot. Body is a plain pagination envelope; the route's <c>lotId</c>
    /// wins. Bidder names are anonymized as <c>b****&lt;n&gt;</c> while the lot is Active and surface
    /// as full display names once the lot transitions to Sold / EndedNoSale / Cancelled.
    /// </summary>
    [HttpPost("{lotId:guid}/bids/list")]
    public Task<Result<Paginated<BidItemModel>>> ListBidHistory(Guid lotId, [FromBody] PageRequest paginate, CancellationToken ct) =>
        mediator.Send(new GetBidHistoryRequest { LotId = lotId, Paginate = paginate }, ct);
}
