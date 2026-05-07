using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record GetMyLotsRequest : PageRequest, IRequest<Result<Paginated<MyLotItemModel>>>
{
    public GetMyLotsFilters Filters { get; init; } = new();
}

public record GetMyLotsFilters
{
    public LotStatus? Status { get; init; }
}
