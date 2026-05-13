using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.Models;
using MediatR;

namespace Coiny.Application.Features.Shipments.Requests;

public record GetShipmentByPaymentIdRequest(Guid PaymentId) : IRequest<Result<ShipmentDetailModel>>;
