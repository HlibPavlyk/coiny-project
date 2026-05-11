using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using MediatR;

namespace Coiny.Application.Features.Bids.Requests;

/// <summary>
/// Public bid-history page for a lot. <see cref="LotId"/> is filled by the controller from the route;
/// the body carries only the <c>Paginate</c> envelope.
/// </summary>
public record GetBidHistoryRequest : PageQuery, IRequest<Result<Paginated<BidItemModel>>>
{
    public Guid LotId { get; init; }
}
