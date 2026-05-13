using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Shipments.Models;
using Coiny.Application.Features.Shipments.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Shipments.Handlers;

public class GetShipmentByPaymentIdHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetShipmentByPaymentIdRequest, Result<ShipmentDetailModel>>
{
    public async Task<Result<ShipmentDetailModel>> Handle(GetShipmentByPaymentIdRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<ShipmentDetailModel>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Shipment? shipment = await db.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.PaymentId == request.PaymentId, ct);

        if (shipment is null)
            return Result.Failure<ShipmentDetailModel>(
                Error.NotFound("Shipment.NotFound", $"No shipment for payment {request.PaymentId}."));

        if (shipment.BuyerId != userId && shipment.SellerId != userId)
            return Result.Failure<ShipmentDetailModel>(
                Error.Forbidden("Shipment.NotParty", "Only the buyer or seller can view this shipment."));

        // Load events separately so the timeline is ordered deterministically (oldest first).
        List<ShipmentEventModel> events = await db.ShipmentEvents
            .AsNoTracking()
            .Where(e => e.ShipmentId == shipment.Id)
            .OrderBy(e => e.ObservedAt)
            .ThenBy(e => e.Id)
            .Select(e => new ShipmentEventModel(
                e.Id,
                e.Status,
                e.NpStatusCode,
                e.Description,
                e.ObservedAt))
            .ToListAsync(ct);

        var model = new ShipmentDetailModel(
            shipment.Id,
            shipment.PaymentId,
            shipment.LotId,
            shipment.BuyerId,
            shipment.SellerId,
            shipment.NovaPoshtaTtn,
            shipment.IntDocNumber,
            shipment.RecipientCityRef,
            shipment.RecipientCityLabel,
            shipment.RecipientWarehouseRef,
            shipment.RecipientWarehouseLabel,
            shipment.RecipientName,
            shipment.RecipientPhone,
            shipment.DeclaredValueUahKopiykas,
            shipment.Status,
            shipment.LastNpStatusCode,
            shipment.DeliveredAt,
            shipment.LastPolledAt,
            events,
            shipment.CreatedAt,
            shipment.UpdatedAt);

        return Result.Success(model);
    }
}
