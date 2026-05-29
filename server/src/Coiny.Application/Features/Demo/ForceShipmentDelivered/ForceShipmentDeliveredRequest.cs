using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Demo.ForceShipmentDelivered;

/// <summary>Demo-only: simulate NP "Delivered" status for the Shipment attached to <see cref="LotId"/>.</summary>
public record ForceShipmentDeliveredRequest(Guid LotId) : IRequest<Result>;
