using System.Text.Json;
using Coiny.Application.Common.Json;
using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Lots.Events;

/// <summary>
/// Outbox payload for <c>LotEnded</c> events emitted on cancel/end-no-sale. Sprint 4 Meilisearch sync removes the lot from the index.
/// </summary>
public sealed record LotEndedPayload(Guid Id, LotStatus Status)
{
    public const string EventType = "LotEnded";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static LotEndedPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<LotEndedPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("LotEndedPayload deserialization returned null.");
}
