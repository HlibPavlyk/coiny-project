using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.ExternalServices.Email;
using Coiny.Application.Features.Auctions;
using Coiny.Application.Features.Auth;
using Coiny.Application.Features.Shipments;
using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Coiny.Application.Features.Auctions.Events;
using Coiny.Application.Features.Auth.Shared;
using Coiny.Application.Features.Shipments.Shared;

namespace Coiny.Infrastructure.Jobs;

public class EmailOutboxFlushJob(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<EmailOutboxFlushJob> logger)
{
    private const int _batchSize = 100;

    // Only these shipment transitions reach the buyer's inbox per THESIS-SCOPE.md §1 §15.
    private static readonly HashSet<string> _notifiableShipmentStatuses =
        new(StringComparer.Ordinal) { "InTransit", "Delivered" };

    public async Task RunAsync(CancellationToken ct)
    {
        List<EmailOutboxEvent> pending = await db.EmailOutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.Id)
            .Take(_batchSize)
            .ToListAsync(ct);

        foreach (EmailOutboxEvent evt in pending)
        {
            try
            {
                bool sent = await DispatchAsync(evt, ct);
                if (sent)
                    evt.ProcessedAt = DateTime.UtcNow;
                // If DispatchAsync returned false, the handler already decided how to mark the row
                // (e.g. skip-and-mark for non-notifying shipment statuses).
            }
            catch (Exception ex)
            {
                evt.AttemptCount++;
                evt.LastError = ex.Message;
                logger.LogWarning(ex, "EmailOutboxFlushJob: failed to process event {Id} ({EventType}), attempt {Attempt}",
                    evt.Id, evt.EventType, evt.AttemptCount);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Returns <c>true</c> if the caller should mark <see cref="EmailOutboxEvent.ProcessedAt"/>.
    /// Handlers that want bespoke marking semantics (e.g., "skip and mark processed without sending")
    /// can mutate the row themselves and return <c>false</c>.
    /// </summary>
    private async Task<bool> DispatchAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        switch (evt.EventType)
        {
            case EmailVerificationPayload.EventType:
                await HandleVerificationAsync(evt, ct);
                return true;

            case AuctionWonPayWithin96hPayload.EventType:
                await HandleWonPayAsync(evt, reminder: false, ct);
                return true;

            case AuctionWonPayReminderPayload.EventType:
                await HandleWonPayReminderAsync(evt, ct);
                return true;

            case ShipmentStatusChangedPayload.EventType:
                return await HandleShipmentStatusAsync(evt, ct);

            default:
                logger.LogWarning("EmailOutboxFlushJob: unknown EventType={EventType} (id={Id}) — skipping",
                    evt.EventType, evt.Id);
                evt.ProcessedAt = DateTime.UtcNow; // park the row so it doesn't loop forever
                return false;
        }
    }

    private async Task HandleVerificationAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        var payload = EmailVerificationPayload.Deserialize(evt.Payload);
        string verificationUrl = $"{FrontendBase()}/verify-email?token={Uri.EscapeDataString(payload.Token)}";
        await emailSender.SendVerificationEmailAsync(payload.ToAddress, verificationUrl, ct);
    }

    private async Task HandleWonPayAsync(EmailOutboxEvent evt, bool reminder, CancellationToken ct)
    {
        var payload = AuctionWonPayWithin96hPayload.Deserialize(evt.Payload);

        // The "you won — pay within 96h" email is emitted in the SAME transaction as AuctionCloseJob,
        // BEFORE any Payment row exists (Payment is created later when the buyer fills checkout
        // details and mints a Stripe intent). The buyer id is already on the outbox row's
        // AggregateId — no Payment lookup needed. Going through Payment here would silently lose
        // every initial "you won" email forever (legacy bug).
        if (evt.AggregateId == Guid.Empty)
        {
            logger.LogWarning("EmailOutboxFlushJob: AuctionWonPayWithin96h event {Id} has no AggregateId (buyer)", evt.Id);
            return;
        }

        string toAddress = await ResolveBuyerEmailAsync(evt.AggregateId, ct);
        if (toAddress is "")
            return;

        // Route the buyer to the lot-id-based pay page (the route that exists in router.tsx).
        // PayLotPage state-probes on mount and renders the right step depending on what's already
        // saved — works for both fresh winners and returning buyers.
        string paymentUrl = $"{FrontendBase()}/my-purchases/{payload.LotId}/pay";
        if (reminder)
        {
            await emailSender.SendWonPayReminderEmailAsync(
                toAddress, payload.LotTitle, payload.AmountUahKopiykas, payload.DueAt, paymentUrl, ct);
        }
        else
        {
            await emailSender.SendWonPayEmailAsync(
                toAddress, payload.LotTitle, payload.AmountUahKopiykas, payload.DueAt, paymentUrl, ct);
        }
    }

    private async Task HandleWonPayReminderAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        var payload = AuctionWonPayReminderPayload.Deserialize(evt.Payload);
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == payload.PaymentId, ct);
        if (payment is null)
        {
            logger.LogWarning("EmailOutboxFlushJob: payment {PaymentId} not found for AuctionWonPayReminder event {Id}",
                payload.PaymentId, evt.Id);
            return;
        }

        string toAddress = await ResolveBuyerEmailAsync(payment.BuyerId, ct);
        if (toAddress is "")
            return;

        // Same lot-id-based route as the initial won-pay email — PayLotPage handles step routing.
        string paymentUrl = $"{FrontendBase()}/my-purchases/{payment.LotId}/pay";
        await emailSender.SendWonPayReminderEmailAsync(
            toAddress, payload.LotTitle, payload.AmountUahKopiykas, payload.DueAt, paymentUrl, ct);
    }

    private async Task<bool> HandleShipmentStatusAsync(EmailOutboxEvent evt, CancellationToken ct)
    {
        var payload = ShipmentStatusChangedPayload.Deserialize(evt.Payload);

        if (!_notifiableShipmentStatuses.Contains(payload.Status))
        {
            logger.LogInformation(
                "EmailOutboxFlushJob: ShipmentStatusChanged status={Status} is not a notifying transition — marking processed without send",
                payload.Status);
            evt.ProcessedAt = DateTime.UtcNow;
            return false;
        }

        Shipment? shipment = await db.Shipments.FirstOrDefaultAsync(s => s.Id == payload.ShipmentId, ct);
        if (shipment is null)
        {
            logger.LogWarning("EmailOutboxFlushJob: shipment {ShipmentId} not found for ShipmentStatusChanged event {Id}",
                payload.ShipmentId, evt.Id);
            return true; // mark processed; nothing to do
        }

        Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == shipment.LotId, ct);
        string toAddress = await ResolveBuyerEmailAsync(shipment.BuyerId, ct);
        if (toAddress is "")
            return true;

        await emailSender.SendShipmentStatusEmailAsync(
            toAddress,
            lotTitle: lot?.Title ?? "(unknown lot)",
            ttn: payload.Ttn,
            status: payload.Status,
            ct);

        return true;
    }

    private string FrontendBase() =>
        configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Frontend:BaseUrl is not configured.");

    private async Task<string> ResolveBuyerEmailAsync(Guid buyerId, CancellationToken ct)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == buyerId, ct);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogWarning("EmailOutboxFlushJob: buyer {BuyerId} has no email address", buyerId);
            return string.Empty;
        }

        return user.Email;
    }
}
