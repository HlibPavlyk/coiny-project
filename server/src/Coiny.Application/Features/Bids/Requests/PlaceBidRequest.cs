using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using MediatR;

namespace Coiny.Application.Features.Bids.Requests;

/// <summary>
/// Place a bid on an Active lot. <see cref="LotId"/> is filled by the controller from the route segment;
/// the JSON body carries only <see cref="AmountUahKopiykas"/>.
/// </summary>
public record PlaceBidRequest(Guid LotId, long AmountUahKopiykas) : IRequest<Result<PlaceBidModel>>;
