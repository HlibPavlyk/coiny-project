namespace Coiny.Domain.Entities;

public class EmailOutboxEvent
{
    public long Id { get; set; }

    /// <summary>Entity type that produced the event, e.g. "User".</summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>PK of the producing entity (e.g. User.Id).</summary>
    public Guid AggregateId { get; set; }

    /// <summary>Discriminator consumed by EmailOutboxFlushJob: EmailVerificationRequested, AuctionWonPayWithin96h, ShipmentStatusChanged.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Per-event-type payload JSON (e.g. {userId, rawToken} for verification). Stored as jsonb.</summary>
    public string Payload { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }
}
