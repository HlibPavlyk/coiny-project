using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record PublishLotRequest(Guid Id) : IRequest<Result<PublishedLotModel>>;
