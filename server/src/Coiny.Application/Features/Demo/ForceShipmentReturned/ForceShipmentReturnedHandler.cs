using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Demo.ForceShipmentReturned;

/// <summary>
/// Demo handler that mirrors NP polling's "package returned to sender" path: flips
/// <c>Shipment.Status</c> to <see cref="ShipmentStatus.Returned"/> and enqueues
/// <c>CancelPaymentJob</c> to void the authorized hold. Stripe webhook then drives
/// <c>Payment.Status → Cancelled</c>; production logic determines what happens to the lot.
/// </summary>
public class ForceShipmentReturnedHandler(
    IApplicationDbContext db,
    IJobScheduler jobScheduler)
    : IRequestHandler<ForceShipmentReturnedRequest, Result>
{
    public async Task<Result> Handle(ForceShipmentReturnedRequest request, CancellationToken ct)
    {
        Shipment? shipment = await db.Shipments.FirstOrDefaultAsync(s => s.LotId == request.LotId, ct);
        if (shipment is null)
            return Result.Failure(Error.NotFound(
                "Shipment.NotFound",
                "No shipment exists for this lot — the buyer must complete checkout details first."));

        if (shipment.Status is ShipmentStatus.Returned)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentAlreadyReturned",
                "Shipment is already Returned."));

        if (shipment.Status is ShipmentStatus.Delivered or ShipmentStatus.Refused or ShipmentStatus.Lost)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentTerminal",
                $"Shipment is in terminal {shipment.Status} — cannot be force-returned."));

        if (shipment.PaymentId is not { } paymentId)
            return Result.Failure(Error.Conflict(
                "Demo.ShipmentHasNoPayment",
                "Shipment has no linked payment — cannot void."));

        shipment.Status = ShipmentStatus.Returned;
        await db.SaveChangesAsync(ct);

        jobScheduler.EnqueueCancelPayment(paymentId);
        return Result.Success();
    }
}
