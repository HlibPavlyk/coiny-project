using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record DeleteLotRequest(Guid Id) : IRequest<Result>;
