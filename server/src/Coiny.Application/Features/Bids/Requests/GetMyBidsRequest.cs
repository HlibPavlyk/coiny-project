using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using MediatR;

namespace Coiny.Application.Features.Bids.Requests;

/// <summary>Authenticated caller's bid history with each bid's lot state.</summary>
public record GetMyBidsRequest : PageRequest, IRequest<Result<Paginated<MyBidItemModel>>>;
