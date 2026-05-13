using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Coiny.Infrastructure.ExternalServices.Stripe;

/// <summary>
/// Single entry point for incoming Stripe events. Verifies the signature, deduplicates by
/// <c>event.id</c> against the <c>StripeWebhookEvents</c> table, and dispatches to the
/// appropriate domain mutation. Dispatch failures are recorded on the row and retried hourly
/// by <c>RetryFailedWebhookJob</c> — the HTTP response is always 200 (or 400 only on bad signature).
/// </summary>
public class StripeWebhookProcessor(
    IApplicationDbContext db,
    IOptions<StripeOptions> options,
    IJobScheduler jobs,
    IDateTimeProvider clock,
    ILogger<StripeWebhookProcessor> logger)
{
    private readonly string _webhookSecret = options.Value.WebhookSecret;

    public async Task<Result> ProcessAsync(string rawBody, string signatureHeader, CancellationToken ct)
    {
        Event stripeEvent;
        try
        {
            // throwOnApiVersionMismatch: false — Stripe.net 47.x targets acacia; our dashboard
            // sends dahlia. The fields we read (event.type, data.object.id, details_submitted,
            // charges_enabled, amount_capturable) are stable across the diff.
            stripeEvent = EventUtility.ConstructEvent(rawBody, signatureHeader, _webhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "StripeWebhook: signature verification failed ({Message})", ex.Message);
            return Result.Failure(Error.Validation("Stripe.WebhookBadSignature", "Webhook signature verification failed."));
        }

        return await IngestVerifiedAsync(stripeEvent, rawBody, ct);
    }

    /// <summary>
    /// Same pipeline as <see cref="ProcessAsync"/> minus signature verification.
    /// Public so tests can drive dispatch directly without forging Stripe signatures.
    /// </summary>
    public async Task<Result> IngestVerifiedAsync(Event stripeEvent, string rawBody, CancellationToken ct)
    {
        // Pre-check is cheap (PK lookup) and gives us the same idempotency guarantee on retries.
        // The unique constraint on the column is the safety net for genuine concurrent writes.
        bool alreadySeen = await db.StripeWebhookEvents.AnyAsync(e => e.Id == stripeEvent.Id, ct);
        if (alreadySeen)
        {
            logger.LogInformation("StripeWebhook: duplicate event {EventId} ({EventType}) — skipping",
                stripeEvent.Id, stripeEvent.Type);
            return Result.Success();
        }

        StripeWebhookEvent row = new()
        {
            Id = stripeEvent.Id,
            EventType = stripeEvent.Type,
            ReceivedAt = clock.UtcNow,
            PayloadJson = rawBody,
            AttemptCount = 0,
        };

        db.StripeWebhookEvents.Add(row);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Lost a race with a concurrent webhook delivery for the same event id — no-op.
            logger.LogInformation("StripeWebhook: concurrent duplicate {EventId} ({EventType}) — skipping",
                stripeEvent.Id, stripeEvent.Type);
            return Result.Success();
        }

        await DispatchAndPersistAsync(row, stripeEvent, ct);
        return Result.Success();
    }

    /// <summary>
    /// Re-runs dispatch for a row already persisted in <c>StripeWebhookEvents</c>.
    /// Called by <c>RetryFailedWebhookJob</c>; does not insert.
    /// </summary>
    public Task ReDispatchAsync(StripeWebhookEvent row, CancellationToken ct)
    {
        Event stripeEvent = EventUtility.ParseEvent(row.PayloadJson);
        return DispatchAndPersistAsync(row, stripeEvent, ct);
    }

    private async Task DispatchAndPersistAsync(StripeWebhookEvent row, Event stripeEvent, CancellationToken ct)
    {
        try
        {
            await DispatchAsync(stripeEvent, ct);
            row.ProcessedAt = clock.UtcNow;
            row.ProcessingError = null;
        }
        catch (Exception ex)
        {
            row.AttemptCount++;
            row.ProcessingError = ex.Message;
            logger.LogError(ex, "StripeWebhook: dispatch failed for {EventId} ({EventType}), attempt {Attempt}",
                row.Id, row.EventType, row.AttemptCount);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task DispatchAsync(Event stripeEvent, CancellationToken ct)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.AccountUpdated:
                if (stripeEvent.Data?.Object is Account account)
                    await HandleAccountUpdatedAsync(account, ct);
                return;

            case EventTypes.PaymentIntentAmountCapturableUpdated:
                if (stripeEvent.Data?.Object is PaymentIntent authorizedIntent)
                    await HandlePaymentAuthorizedAsync(authorizedIntent, stripeEvent.Id, ct);
                return;

            case EventTypes.PaymentIntentCanceled:
                if (stripeEvent.Data?.Object is PaymentIntent cancelledIntent)
                    await HandlePaymentTerminalAsync(cancelledIntent, stripeEvent.Id, PaymentStatus.Cancelled, ct);
                return;

            case EventTypes.PaymentIntentSucceeded:
                if (stripeEvent.Data?.Object is PaymentIntent succeededIntent)
                    await HandlePaymentCapturedAsync(succeededIntent, stripeEvent.Id, ct);
                return;

            case EventTypes.PaymentIntentPaymentFailed:
                if (stripeEvent.Data?.Object is PaymentIntent failedIntent)
                    await HandlePaymentTerminalAsync(failedIntent, stripeEvent.Id, PaymentStatus.Failed, ct);
                return;

            case "charge.dispute.created":
            case "charge.dispute.updated":
            case "charge.dispute.closed":
            case "charge.dispute.funds_withdrawn":
            case "charge.dispute.funds_reinstated":
                // DESCRIBE-ONLY per THESIS-SCOPE.md §1 §7 — manual handling outside the app.
                logger.LogWarning("StripeWebhook: charge dispute event {EventType} ({EventId}) — logged only",
                    stripeEvent.Type, stripeEvent.Id);
                return;

            default:
                logger.LogInformation("StripeWebhook: unhandled event type {EventType} ({EventId}) — no-op",
                    stripeEvent.Type, stripeEvent.Id);
                return;
        }
    }

    private async Task HandleAccountUpdatedAsync(Account account, CancellationToken ct)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.StripeAccountId == account.Id, ct);
        if (user is null)
        {
            logger.LogWarning("StripeWebhook: account.updated for unknown account {AccountId}", account.Id);
            return;
        }

        bool onboardedNow = account.DetailsSubmitted && account.ChargesEnabled;
        if (user.StripeOnboarded != onboardedNow)
        {
            user.StripeOnboarded = onboardedNow;
            user.UpdatedAt = clock.UtcNow;
        }
    }

    private async Task HandlePaymentAuthorizedAsync(PaymentIntent intent, string eventId, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id, ct);
        if (payment is null)
        {
            logger.LogWarning("StripeWebhook: PaymentIntent {IntentId} not found locally", intent.Id);
            return;
        }

        if (payment.Status is PaymentStatus.Captured or PaymentStatus.Cancelled or PaymentStatus.Failed)
        {
            logger.LogWarning("StripeWebhook: late authorization for terminal payment {PaymentId} (status {Status}) — ignored",
                payment.Id, payment.Status);
            return;
        }

        if (payment.Status == PaymentStatus.PendingAuthorization)
            payment.Status = PaymentStatus.Authorized;

        payment.AuthorizedAt ??= clock.UtcNow;
        payment.UpdatedAt = clock.UtcNow;
        payment.LastWebhookEventId = eventId;

        // Hand off to the NP TTN creation job; the job is idempotent on paymentId.
        jobs.EnqueueCreateTtn(payment.Id);
    }

    private async Task HandlePaymentCapturedAsync(PaymentIntent intent, string eventId, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id, ct);
        if (payment is null)
        {
            logger.LogWarning("StripeWebhook: PaymentIntent {IntentId} not found locally", intent.Id);
            return;
        }

        if (payment.Status is PaymentStatus.Cancelled or PaymentStatus.Failed)
        {
            logger.LogWarning("StripeWebhook: capture event for terminal payment {PaymentId} (status {Status}) — ignored",
                payment.Id, payment.Status);
            return;
        }

        // Award trust score on the *transition* to Captured (THESIS-SCOPE.md §9: +5 per
        // successful sale). Guarded by the early-exit above for already-Captured payments,
        // so a webhook redelivery does not double-credit. The webhook dedupe table provides
        // the second line of defence.
        bool isFirstCapture = payment.Status != PaymentStatus.Captured;

        payment.Status = PaymentStatus.Captured;
        payment.CapturedAt ??= clock.UtcNow;
        payment.UpdatedAt = clock.UtcNow;
        payment.LastWebhookEventId = eventId;

        if (isFirstCapture)
        {
            User? seller = await db.Users.FirstOrDefaultAsync(u => u.Id == payment.SellerId, ct);
            if (seller is not null)
            {
                seller.TrustScore += 5;
                seller.UpdatedAt = clock.UtcNow;
            }
            else
            {
                logger.LogWarning(
                    "StripeWebhook: cannot credit trust score — seller {SellerId} for payment {PaymentId} not found",
                    payment.SellerId, payment.Id);
            }
        }
    }

    private async Task HandlePaymentTerminalAsync(
        PaymentIntent intent,
        string eventId,
        PaymentStatus terminalStatus,
        CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intent.Id, ct);
        if (payment is null)
        {
            logger.LogWarning("StripeWebhook: PaymentIntent {IntentId} not found locally", intent.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Captured)
        {
            logger.LogWarning("StripeWebhook: late {TerminalStatus} for captured payment {PaymentId} — ignored",
                terminalStatus, payment.Id);
            return;
        }

        payment.Status = terminalStatus;
        if (terminalStatus == PaymentStatus.Cancelled)
            payment.CancelledAt ??= clock.UtcNow;
        payment.UpdatedAt = clock.UtcNow;
        payment.LastWebhookEventId = eventId;
    }
}
