using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Shipping;
using Coiny.Application.Features.Shipments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.ExternalServices.NovaPoshta;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Polls Nova Poshta every 15 minutes for status updates on every active shipment. Records each
/// observation as an append-only <see cref="ShipmentEvent"/>, transitions <see cref="Shipment.Status"/>
/// when NP reports progress, schedules <see cref="ICapturePaymentJob"/> 24h after Delivered, and
/// enqueues <c>ShipmentStatusChanged</c> outbox events for the user-visible transitions (InTransit,
/// Delivered).
///
/// Idempotent — the unique tuple <c>(ShipmentId, NpStatusCode, ObservedAt)</c> on ShipmentEvent
/// dedupes accidental duplicate writes; the <c>Status != newStatus</c> guard ensures no double
/// transitions or duplicate outbox rows.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public class NovaPoshtaPollingJob(
    IApplicationDbContext db,
    INovaPoshtaClient np,
    IJobScheduler jobs,
    IDateTimeProvider clock,
    ILogger<NovaPoshtaPollingJob> logger)
{
    private const int _batchSize = 100;
    private static readonly TimeSpan _captureDelay = TimeSpan.FromHours(24);

    public async Task RunAsync(CancellationToken ct)
    {
        // Polled = anything not in PendingTtn and not terminal. Order by LastPolledAt so the oldest
        // observations refresh first, which keeps the lookup distribution fair as the dataset grows.
        List<Shipment> active = await db.Shipments
            .Where(s => s.Status != ShipmentStatus.PendingTtn
                     && s.Status != ShipmentStatus.Delivered
                     && s.Status != ShipmentStatus.Refused
                     && s.Status != ShipmentStatus.Returned
                     && s.Status != ShipmentStatus.Lost
                     && s.NovaPoshtaTtn != null)
            .OrderBy(s => s.LastPolledAt)
            .Take(_batchSize)
            .ToListAsync(ct);

        if (active.Count == 0)
            return;

        string[] ttns = active.Select(s => s.NovaPoshtaTtn!).ToArray();
        IReadOnlyList<NpTrackingStatus> npResponses;
        try
        {
            npResponses = await np.GetStatusDocumentsAsync(ttns, ct);
        }
        catch (Exception ex)
        {
            // Let Hangfire retry; the policy on the class will kick in.
            logger.LogWarning(ex, "NovaPoshtaPollingJob: NP call failed for batch of {Count}", active.Count);
            throw;
        }

        DateTime now = clock.UtcNow;
        var byTtn = npResponses.ToDictionary(r => r.Ttn);

        foreach (Shipment shipment in active)
        {
            if (!byTtn.TryGetValue(shipment.NovaPoshtaTtn!, out NpTrackingStatus? np_row))
                continue; // NP didn't echo this TTN — skip silently, next cycle picks it up

            ShipmentStatus? mapped = StatusMapper.TryMap(np_row.StatusCode, logger);
            shipment.LastPolledAt = now;

            if (mapped is null)
                continue; // unknown code — log already emitted; keep status untouched

            // Only record an event when NP reports a NEW status code. Polling on a 15-minute
            // cadence (or, during the demo, hammered manually from the Hangfire dashboard) would
            // otherwise produce duplicate timeline entries for every wait window the synthetic
            // state machine sits in. The dedupe is per-shipment so a genuine state regression
            // (e.g., a returned parcel re-entering InTransit) still produces a fresh row.
            if (np_row.StatusCode == shipment.LastNpStatusCode)
                continue;

            db.ShipmentEvents.Add(new ShipmentEvent
            {
                ShipmentId = shipment.Id,
                Status = mapped.Value,
                NpStatusCode = np_row.StatusCode,
                Description = np_row.Description,
                ObservedAt = now,
            });

            shipment.LastNpStatusCode = np_row.StatusCode;

            if (mapped.Value == shipment.Status)
                continue; // mapped status unchanged (e.g. codes 4 → 5 both → InTransit); audit row recorded above

            ShipmentStatus previousStatus = shipment.Status;
            shipment.Status = mapped.Value;
            shipment.UpdatedAt = now;

            // First Delivered → set DeliveredAt + schedule capture.
            if (mapped.Value == ShipmentStatus.Delivered && shipment.DeliveredAt is null)
            {
                shipment.DeliveredAt = now;
                if (shipment.PaymentId is { } paymentId)
                {
                    jobs.ScheduleCapture(paymentId, _captureDelay);
                    logger.LogInformation(
                        "NovaPoshtaPolling: shipment {ShipmentId} Delivered — capture scheduled in {Delay}",
                        shipment.Id, _captureDelay);
                }
            }

            // Outbox row only for user-visible transitions per THESIS-SCOPE.md §1 §15.
            if (mapped.Value == ShipmentStatus.InTransit || mapped.Value == ShipmentStatus.Delivered)
            {
                db.EmailOutboxEvents.Add(new EmailOutboxEvent
                {
                    AggregateType = nameof(Shipment),
                    AggregateId = shipment.Id,
                    EventType = ShipmentStatusChangedPayload.EventType,
                    Payload = new ShipmentStatusChangedPayload(
                        PaymentId: shipment.PaymentId ?? Guid.Empty,
                        ShipmentId: shipment.Id,
                        Ttn: shipment.NovaPoshtaTtn!,
                        Status: mapped.Value.ToString(),
                        PreviousStatus: previousStatus.ToString()).Serialize(),
                    CreatedAt = now,
                    AttemptCount = 0,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
