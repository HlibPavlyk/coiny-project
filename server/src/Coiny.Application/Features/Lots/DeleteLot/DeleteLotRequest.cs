using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Lots.DeleteLot;

public record DeleteLotRequest(Guid Id) : IRequest<Result>;
