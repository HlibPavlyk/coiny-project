using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Payments;
using Coiny.Application.Abstractions.Providers;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Scheduled by <c>NovaPoshtaPollingJob</c> 24 hours after a shipment first reaches Delivered.
/// Captures the authorized Stripe PaymentIntent so funds move from buyer's card to the platform
/// (and Stripe transfers the destination portion to the seller's Connect account in the same call).
///
/// <para>
/// The job does NOT mutate <see cref="Payment.Status"/> itself — Stripe's <c>payment_intent.succeeded</c>
/// webhook is the single source of truth (handled by <c>StripeWebhookProcessor</c>, task 06). The job's
/// only responsibility is "ask Stripe to capture"; local state follows asynchronously.
/// </para>
///
/// Idempotent on every re-fire: the <see cref="PaymentStatus.Authorized"/> precondition short-circuits
/// once the webhook has already flipped the row to Captured (or any terminal state).
/// </summary>
[AutomaticRetry(Attempts = 5)]
public class CapturePaymentJob(
    IApplicationDbContext db,
    IStripeClient stripe,
    IDateTimeProvider clock,
    ILogger<CapturePaymentJob> logger) : ICapturePaymentJob
{
    private static readonly TimeSpan _captureBuffer = TimeSpan.FromHours(24);

    public async Task RunAsync(Guid paymentId, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        if (payment is null)
        {
            logger.LogWarning("CapturePaymentJob: payment {PaymentId} not found", paymentId);
            return;
        }

        // Idempotent re-fire: Captured (webhook already arrived), Cancelled, or Failed → nothing to do.
        if (payment.Status != PaymentStatus.Authorized)
        {
            logger.LogInformation(
                "CapturePaymentJob: payment {PaymentId} in {Status} — already settled, no-op",
                paymentId, payment.Status);
            return;
        }

        Shipment? shipment = await db.Shipments.FirstOrDefaultAsync(s => s.PaymentId == paymentId, ct);
        if (shipment is null)
        {
            logger.LogWarning(
                "CapturePaymentJob: no shipment for payment {PaymentId} — capture aborted",
                paymentId);
            return;
        }

        if (shipment.Status != ShipmentStatus.Delivered)
        {
            // Race: polling job scheduled us, then NP reported a regression (Refused / Returned)
            // before we ran. Drop the capture — the polling job will reschedule on the next true Delivered.
            logger.LogWarning(
                "CapturePaymentJob: shipment {ShipmentId} is in {Status}, not Delivered — capture aborted",
                shipment.Id, shipment.Status);
            return;
        }

        if (shipment.DeliveredAt is null)
        {
            logger.LogWarning(
                "CapturePaymentJob: shipment {ShipmentId} Delivered but DeliveredAt is null — capture aborted",
                shipment.Id);
            return;
        }

        DateTime now = clock.UtcNow;
        // DEMO: 24h dispute-window guard temporarily disabled so capture can be triggered manually
        // from the Hangfire dashboard immediately after Delivered. RESTORE before production:
        // if (shipment.DeliveredAt.Value + _captureBuffer > now)
        // {
        //     // Hangfire may fire slightly early on clock drift; the polling job's +24h is authoritative.
        //     logger.LogWarning(
        //         "CapturePaymentJob: scheduled too early (delivered={DeliveredAt}, now={Now}) — relying on poll-job rescheduling",
        //         shipment.DeliveredAt, now);
        //     return;
        // }

        await stripe.CapturePaymentIntentAsync(payment.StripePaymentIntentId, ct);

        logger.LogInformation(
            "CapturePaymentJob: capture requested for payment {PaymentId} (pi={PiId}); webhook will flip status",
            paymentId, payment.StripePaymentIntentId);
    }
}
