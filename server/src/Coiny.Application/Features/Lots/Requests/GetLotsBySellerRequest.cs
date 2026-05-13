using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

/// <summary>
/// Public seller-scoped lot listing for <c>PublicProfilePage</c>. Defaults to <c>Active</c>;
/// the profile's "Sold" tab passes <c>Status = Sold</c>. We deliberately do not expose Draft
/// or EndedNoSale here — those are seller-private states (covered by <see cref="GetMyLotsRequest"/>).
/// </summary>
public record GetLotsBySellerRequest : PageRequest, IRequest<Result<Paginated<LotCardModel>>>
{
    public Guid SellerId { get; init; }
    public LotStatus Status { get; init; } = LotStatus.Active;
}
