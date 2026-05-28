using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using MediatR;

namespace Coiny.Application.Features.Lots.GetLotById;

public record GetLotByIdRequest(Guid Id) : IRequest<Result<LotDetailModel>>;
