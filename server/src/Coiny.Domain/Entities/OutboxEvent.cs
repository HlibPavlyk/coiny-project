namespace Coiny.Domain.Entities;

public class OutboxEvent
{
    public long Id { get; set; }

    /// <summary>Entity type that produced the event, e.g. "Lot".</summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>PK of the producing entity (e.g. Lot.Id).</summary>
    public Guid AggregateId { get; set; }

    /// <summary>Discriminator consumed by MeilisearchSyncJob: LotPublished, LotPriceChanged, LotEnded, LotSold, LotDeleted.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON document body for the Meilisearch indexer. Stored as jsonb; Domain holds the raw JSON string.</summary>
    public string Payload { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }
}
