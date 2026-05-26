using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Lots.Events;

/// <summary>
/// Outbox payload for <c>LotPublished</c> events. Producer: <c>PublishLotHandler</c>.
/// Consumer: Meilisearch sync job (Sprint 4). Sprint-1 fields are minimal but search-friendly.
/// </summary>
public sealed record LotPublishedPayload(
    Guid Id,
    string Title,
    string Description,
    int CategoryId,
    string Condition,
    long CurrentPriceUahKopiykas,
    int BidCount,
    DateTime EndsAt,
    DateTime CreatedAt,
    JsonElement Attributes)
{
    public const string EventType = "LotPublished";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static LotPublishedPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<LotPublishedPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("LotPublishedPayload deserialization returned null.");
}
