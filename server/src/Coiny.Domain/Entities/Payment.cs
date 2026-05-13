using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public Guid LotId { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    /// <summary>Winning bid amount in kopiykas — what the buyer owes locally.</summary>
    public long AmountUahKopiykas { get; set; }

    /// <summary>What we send to Stripe (USD test mode per <c>THESIS-SCOPE.md</c> §B).</summary>
    public long AmountUsdCents { get; set; }

    /// <summary>The <c>Stripe__UahPerUsd</c> value at intent creation. Audit field — locks the rate.</summary>
    public decimal RateUsedUahPerUsd { get; set; }

    /// <summary>Stripe PaymentIntent id (<c>pi_…</c>). Unique.</summary>
    public string StripePaymentIntentId { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    /// <summary>UTC. <c>auctionClosedAt + 96h</c>. Drives the non-payment cancel job.</summary>
    public DateTime DueAt { get; set; }

    /// <summary>UTC. Set when <c>payment_intent.amount_capturable_updated</c> arrives.</summary>
    public DateTime? AuthorizedAt { get; set; }

    /// <summary>UTC. Set on capture confirmation.</summary>
    public DateTime? CapturedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    /// <summary>UTC. Set by <c>PaymentReminderJob</c> when the 48h won-pay reminder is emitted.
    /// Guards against duplicate emails when the job re-fires in the same hourly window.</summary>
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>Most recent Stripe <c>event.id</c> that mutated this row. Debug aid.</summary>
    public string? LastWebhookEventId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>1:1 navigation to <see cref="Shipment"/>. Null until TTN is created.</summary>
    public virtual Shipment? Shipment { get; set; }
}
