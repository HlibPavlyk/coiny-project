using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Abstractions.ExternalServices.Shipping;
using Coiny.Application.Features.Shipments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Coiny.Application.Features.Shipments.Events;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Fires when the Stripe webhook reports <c>payment_intent.amount_capturable_updated</c> (Authorized).
/// Pulls the staged <see cref="Shipment"/> (in <see cref="ShipmentStatus.PendingTtn"/>), calls Nova Poshta to
/// obtain a TTN, persists the result, and enqueues a <c>ShipmentStatusChanged</c> outbox event.
///
/// Idempotent — the <c>Status != PendingTtn</c> guard short-circuits Hangfire retries that re-fire
/// after a successful write. On NP exception the job throws so Hangfire retries (up to 5×).
/// </summary>
[AutomaticRetry(Attempts = 5)]
public class CreateTtnJob(
    IApplicationDbContext db,
    INovaPoshtaClient np,
    IDateTimeProvider clock,
    ILogger<CreateTtnJob> logger) : ICreateTtnJob
{
    public async Task RunAsync(Guid paymentId, CancellationToken ct)
    {
        Payment? payment = await db.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        if (payment is null)
        {
            logger.LogWarning("CreateTtnJob: payment {PaymentId} not found — webhook race?", paymentId);
            return;
        }

        Shipment? shipment = await db.Shipments
            .FirstOrDefaultAsync(s => s.PaymentId == paymentId, ct);
        if (shipment is null)
        {
            logger.LogWarning("CreateTtnJob: no shipment linked to payment {PaymentId} — buyer skipped checkout-details?",
                paymentId);
            return;
        }

        if (shipment.Status != ShipmentStatus.PendingTtn)
        {
            logger.LogInformation("CreateTtnJob: shipment {ShipmentId} already in {Status} — idempotent no-op",
                shipment.Id, shipment.Status);
            return;
        }

        // Sender fields are intentionally empty — HybridNovaPoshtaClient ignores them. A production
        // swap to the real NovaPoshtaClient would resolve sender refs from the seller's profile here.
        var request = new NpSaveDocumentRequest(
            SenderName: string.Empty,
            SenderPhone: string.Empty,
            SenderCityRef: string.Empty,
            SenderWarehouseRef: string.Empty,
            RecipientName: shipment.RecipientName,
            RecipientPhone: shipment.RecipientPhone,
            RecipientCityRef: shipment.RecipientCityRef,
            RecipientWarehouseRef: shipment.RecipientWarehouseRef,
            DeclaredValueUah: shipment.DeclaredValueUahKopiykas / 100m,
            Description: $"Lot {payment.LotId}");

        NpInternetDocument result = await np.SaveInternetDocumentAsync(request, ct);

        DateTime now = clock.UtcNow;
        string previousStatus = shipment.Status.ToString();

        shipment.NovaPoshtaTtn = result.Ttn;
        shipment.IntDocNumber = result.IntDocNumber;
        shipment.Status = ShipmentStatus.TtnCreated;
        shipment.UpdatedAt = now;

        db.EmailOutboxEvents.Add(new EmailOutboxEvent
        {
            AggregateType = nameof(Shipment),
            AggregateId = shipment.Id,
            EventType = ShipmentStatusChangedPayload.EventType,
            Payload = new ShipmentStatusChangedPayload(
                PaymentId: payment.Id,
                ShipmentId: shipment.Id,
                Ttn: result.Ttn,
                Status: nameof(ShipmentStatus.TtnCreated),
                PreviousStatus: previousStatus).Serialize(),
            CreatedAt = now,
            AttemptCount = 0,
        });

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "CreateTtnJob: shipment {ShipmentId} → TtnCreated, ttn={Ttn}, intDoc={IntDoc}",
            shipment.Id, result.Ttn, result.IntDocNumber);
    }
}
