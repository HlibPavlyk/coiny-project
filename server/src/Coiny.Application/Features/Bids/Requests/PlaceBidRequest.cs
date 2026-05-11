using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using MediatR;

namespace Coiny.Application.Features.Bids.Requests;

/// <summary>
/// Wire body for <c>POST /api/v1/lots/{lotId}/bids</c>. Only the amount —
/// <c>lotId</c> is carried by the route, not the body.
/// </summary>
public record PlaceBidBody(long AmountUahKopiykas);

/// <summary>
/// Internal MediatR command. The controller composes <see cref="LotId"/> from the route segment
/// and <see cref="AmountUahKopiykas"/> from <see cref="PlaceBidBody"/> before dispatch.
/// </summary>
public record PlaceBidRequest(Guid LotId, long AmountUahKopiykas) : IRequest<Result<PlaceBidModel>>;
