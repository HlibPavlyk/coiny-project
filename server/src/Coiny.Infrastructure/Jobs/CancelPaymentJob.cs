using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Enqueued by <c>NovaPoshtaPollingJob</c> when a shipment reaches Refused or Returned. Cancels the
/// authorized Stripe PaymentIntent so the hold is released back to the buyer — the lot never reached
/// them, so the escrowed funds must not settle to the seller (THESIS-SCOPE §B).
///
/// <para>
/// Like <c>CapturePaymentJob</c>, this job does NOT mutate <see cref="Payment.Status"/> — Stripe's
/// <c>payment_intent.canceled</c> webhook is the single source of truth (handled by
/// <c>StripeWebhookProcessor</c>). The job only asks Stripe to cancel.
/// </para>
///
/// Idempotent: the <see cref="PaymentStatus.Authorized"/> precondition short-circuits once the
/// webhook has flipped the row to Cancelled (or any other terminal state).
/// </summary>
[AutomaticRetry(Attempts = 5)]
public class CancelPaymentJob(
    IApplicationDbContext db,
    IStripeClient stripe,
    ILogger<CancelPaymentJob> logger) : ICancelPaymentJob
{
    public async Task RunAsync(Guid paymentId, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        if (payment is null)
        {
            logger.LogWarning("CancelPaymentJob: payment {PaymentId} not found", paymentId);
            return;
        }

        // Idempotent re-fire: only an authorized (held) intent can be cancelled for refund. If it
        // was already captured we must NOT silently swallow that — a Refused/Returned parcel whose
        // payment somehow captured needs manual review (Stripe Refund, not cancel).
        if (payment.Status == PaymentStatus.Captured)
        {
            logger.LogWarning(
                "CancelPaymentJob: payment {PaymentId} already Captured — cannot cancel; needs manual refund review",
                paymentId);
            return;
        }

        if (payment.Status != PaymentStatus.Authorized)
        {
            logger.LogInformation(
                "CancelPaymentJob: payment {PaymentId} in {Status} — nothing to cancel, no-op",
                paymentId, payment.Status);
            return;
        }

        await stripe.CancelPaymentIntentAsync(payment.StripePaymentIntentId, "requested_by_customer", ct);

        logger.LogInformation(
            "CancelPaymentJob: cancel requested for payment {PaymentId} (pi={PiId}); webhook will flip status to Cancelled",
            paymentId, payment.StripePaymentIntentId);
    }
}
