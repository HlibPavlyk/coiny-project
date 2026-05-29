using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Demo.ForceShipmentDelivered;

/// <summary>
/// Demo handler that mirrors what <c>NovaPoshtaPollingJob</c> would do when NP reports the buyer
/// has picked up the package: flips <c>Shipment.Status</c> to <see cref="ShipmentStatus.Delivered"/>,
/// stamps <c>DeliveredAt</c>, and enqueues <c>CapturePaymentJob</c> — same single line the polling
/// job uses. From there, real Stripe capture + webhook + SignalR run unmodified.
/// </summary>
public class ForceShipmentDeliveredHandler(
    IApplicationDbContext db,
    IJobScheduler jobScheduler,
    IDateTimeProvider clock)
    : IRequestHandler<ForceShipmentDeliveredRequest, Result>
{
    public async Task<Result> Handle(ForceShipmentDeliveredRequest request, CancellationToken ct)
    {
        Shipment? shipment = await db.Shipments.FirstOrDefaultAsync(s => s.LotId == request.LotId, ct);
        if (shipment is null)
            return Result.Failure(Error.NotFound(
                "Shipment.NotFound",
                "No shipment exists for this lot — the buyer must complete checkout details first."));

        if (shipment.Status == ShipmentStatus.Delivered)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentAlreadyDelivered",
                "Shipment is already Delivered."));

        if (shipment.Status is ShipmentStatus.Refused or ShipmentStatus.Returned or ShipmentStatus.Lost)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentTerminal",
                $"Shipment is in terminal {shipment.Status} — cannot be force-delivered."));

        if (shipment.PaymentId is not { } paymentId)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentHasNoPayment",
                "Shipment has no linked payment — cannot capture."));

        shipment.Status = ShipmentStatus.Delivered;
        shipment.DeliveredAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        jobScheduler.EnqueueCapture(paymentId);
        return Result.Success();
    }
}
