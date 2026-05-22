using Coiny.Application.Common.Results;
using Coiny.Application.Features.Users.Models;
using Coiny.Application.Features.Users.Requests;
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
}
