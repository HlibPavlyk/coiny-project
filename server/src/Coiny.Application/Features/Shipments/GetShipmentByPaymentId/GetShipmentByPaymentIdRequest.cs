using Coiny.Application.Common.Results;
using MediatR;

namespace Coiny.Application.Features.Shipments.GetShipmentByPaymentId;

public record GetShipmentByPaymentIdRequest(Guid PaymentId) : IRequest<Result<ShipmentDetailModel>>;
