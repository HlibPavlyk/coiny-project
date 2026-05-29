namespace Coiny.Domain.Entities;

/// <summary>
/// Idempotency + replay log for Stripe webhooks. Id is the Stripe <c>event.id</c> (<c>evt_…</c>);
/// the webhook handler short-circuits when it sees an id it has already processed.
/// </summary>
public class StripeWebhookEvent
{
    /// <summary>Stripe <c>event.id</c> (<c>evt_…</c>). PK.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>e.g. <c>payment_intent.amount_capturable_updated</c>.</summary>
    public string EventType { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    /// <summary>UTC; null while unprocessed. Set when the handler completes successfully.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Full event payload, kept for replay/debugging. Stored as jsonb (EF config).</summary>
    public string PayloadJson { get; set; } = "{}";

    public string? ProcessingError { get; set; }

    public int AttemptCount { get; set; }
}
