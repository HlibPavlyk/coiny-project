using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Application.Features.Users.Models;
using Coiny.Application.Features.Users.Requests;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Public read-only seller profile. No auth required. Returns hero (displayName, trust score,
    /// member-since) and aggregate stats (lots sold, active lots, average sale price). Banned users
    /// surface as 404 to avoid leaking moderation state.
    /// </summary>
    [HttpGet("{userId:guid}/public")]
    public Task<Result<PublicProfileModel>> GetPublic(Guid userId, CancellationToken ct) =>
        mediator.Send(new GetPublicProfileRequest(userId), ct);

    /// <summary>
    /// Paginated lot listing for a public seller profile. <c>status=Active</c> (default) backs the
    /// "Active lots" tab; <c>status=Sold</c> backs the "Sold lots" tab. Draft / EndedNoSale /
    /// Cancelled are rejected with 400 — only seller-owned views may see those.
    /// </summary>
    [Tags("Lots")]
    [HttpPost("{userId:guid}/lots/search")]
    public Task<Result<Paginated<LotCardModel>>> SearchLots(
        Guid userId,
        [FromQuery] LotStatus status,
        [FromBody] PageRequest paginate,
        CancellationToken ct) =>
        mediator.Send(new GetLotsBySellerRequest
        {
            SellerId = userId,
            Status = status,
            Offset = paginate.Offset,
            Count = paginate.Count,
            SortBy = paginate.SortBy,
        }, ct);
}
