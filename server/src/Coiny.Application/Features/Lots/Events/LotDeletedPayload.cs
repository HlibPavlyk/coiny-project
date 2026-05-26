using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Lots.Events;

/// <summary>
/// Outbox payload for <c>LotDeleted</c> events emitted on admin soft-delete. The Meilisearch sync
/// reconciles by the lot's <em>current</em> state, so a soft-deleted lot is dropped from the index;
/// this event exists to trigger that reconcile and to label the action in the outbox audit trail.
/// </summary>
public sealed record LotDeletedPayload(Guid Id)
{
    public const string EventType = "LotDeleted";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static LotDeletedPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<LotDeletedPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("LotDeletedPayload deserialization returned null.");
}
