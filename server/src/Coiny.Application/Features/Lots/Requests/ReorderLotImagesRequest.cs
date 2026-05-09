using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record ReorderLotImagesRequest(Guid LotId, Guid[] ImageIds) : IRequest<Result>;
