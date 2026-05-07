using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record GetLotsByCategoryRequest : PageQuery, IRequest<Result<Paginated<LotCardModel>>>
{
    public int CategoryId { get; init; }
}
