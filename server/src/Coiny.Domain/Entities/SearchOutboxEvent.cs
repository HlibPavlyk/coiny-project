namespace Coiny.Domain.Entities;

/// <summary>
/// Transactional outbox for Meilisearch search-index synchronization. Written in the same EF
/// transaction that mutates an aggregate; drained asynchronously by the search-index flush job,
/// which reconciles by reading the current entity state from the DB (the payload is therefore
/// intentionally absent — the row is just a "something changed for this aggregate" signal).
/// <see cref="AggregateType"/> + <see cref="AggregateId"/> are kept as the generic envelope so the
/// search outbox can carry non-<c>Lot</c> aggregates without a schema change.
/// </summary>
public class SearchOutboxEvent
{
    public long Id { get; set; }

    /// <summary>Entity type that produced the event, e.g. <c>"Lot"</c>. Today always <c>"Lot"</c>.</summary>
    public string AggregateType { get; set; } = string.Empty;

    /// <summary>PK of the producing entity (e.g. <c>Lot.Id</c>).</summary>
    public Guid AggregateId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }
}
