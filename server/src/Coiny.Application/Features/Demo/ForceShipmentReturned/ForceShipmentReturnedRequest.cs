using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Demo.ForceShipmentReturned;

/// <summary>Demo-only: simulate NP "Returned" status for the Shipment attached to <see cref="LotId"/>.</summary>
public record ForceShipmentReturnedRequest(Guid LotId) : IRequest<Result>;
