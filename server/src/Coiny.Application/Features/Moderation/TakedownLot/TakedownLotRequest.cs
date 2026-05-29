using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Moderation.TakedownLot;

/// <summary>
/// Admin soft-delete of a lot. <see cref="LotId"/> comes from the route; the deletion reason is
/// captured on the originating <c>Report.ResolutionNote</c> (task 06), not duplicated here.
/// </summary>
public record TakedownLotRequest(Guid LotId) : IRequest<Result>;
