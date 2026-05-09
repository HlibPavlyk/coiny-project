using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record DeleteLotImageRequest(Guid LotId, Guid ImageId) : IRequest<Result>;
