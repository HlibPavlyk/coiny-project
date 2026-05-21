namespace Coiny.Domain.Entities;

/// <summary>
/// Transactional outbox for Meilisearch search-index synchronization. Written in the same EF
/// transaction that mutates a lot; drained asynchronously by the search-index flush job. Named for
/// its consumer (search) to mirror <see cref="EmailOutboxEvent"/> (named for its consumer, email).
/// Retains the generic outbox envelope (AggregateType / EventType / Payload) for symmetry with the
/// email outbox; the search consumer reconciles by current lot state.
/// </summary>
public class SearchOutboxEvent
{
    public long Id { get; set; }

    /// <summary>Entity type that produced the event, e.g. "Lot".</summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>PK of the producing entity (e.g. Lot.Id).</summary>
    public Guid AggregateId { get; set; }

    /// <summary>Discriminator consumed by the search-index flush job: LotPublished, LotPriceChanged, LotEnded, LotSold, LotDeleted.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON document body for the Meilisearch indexer. Stored as jsonb; Domain holds the raw JSON string.</summary>
    public string Payload { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }
}
