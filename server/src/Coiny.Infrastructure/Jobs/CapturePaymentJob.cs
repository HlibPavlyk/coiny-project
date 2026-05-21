using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Payments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Enqueued by <c>NovaPoshtaPollingJob</c> the moment a shipment reaches Delivered — captured
/// immediately, with no time buffer (THESIS-SCOPE §B/§F: the NP-counter handover with optional
/// <c>перевірка вмісту</c> and camera record is the operational truth point; the buyer's signature
/// at pickup is acceptance). Captures the authorized Stripe PaymentIntent so funds move from the
/// buyer's card to the platform (and Stripe transfers the destination portion to the seller's
/// Connect account in the same call).
///
/// <para>
/// The job does NOT mutate <see cref="Payment.Status"/> itself — Stripe's <c>payment_intent.succeeded</c>
/// webhook is the single source of truth (handled by <c>StripeWebhookProcessor</c>). The job's only
/// responsibility is "ask Stripe to capture"; local state follows asynchronously.
/// </para>
///
/// Idempotent on every re-fire: the <see cref="PaymentStatus.Authorized"/> precondition short-circuits
/// once the webhook has already flipped the row to Captured (or any terminal state).
/// </summary>
[AutomaticRetry(Attempts = 5)]
public class CapturePaymentJob(
    IApplicationDbContext db,
    IStripeClient stripe,
    ILogger<CapturePaymentJob> logger) : ICapturePaymentJob
{
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
            // Race: polling job enqueued us, then NP reported a regression (Refused / Returned)
            // before we ran. Drop the capture — the refund path (CancelPaymentJob) handles those.
            logger.LogWarning(
                "CapturePaymentJob: shipment {ShipmentId} is in {Status}, not Delivered — capture aborted",
                shipment.Id, shipment.Status);
            return;
        }

        await stripe.CapturePaymentIntentAsync(payment.StripePaymentIntentId, ct);

        logger.LogInformation(
            "CapturePaymentJob: capture requested for payment {PaymentId} (pi={PiId}); webhook will flip status",
            paymentId, payment.StripePaymentIntentId);
    }
}
