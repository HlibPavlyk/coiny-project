using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Lots.Events;

/// <summary>
/// Outbox payload for <c>LotPriceChanged</c> events. Producer: <c>PlaceBidHandler</c>.
/// Consumer: Meilisearch sync job (Sprint 4) — re-indexes current price, bid count, and ends-at.
/// </summary>
public sealed record LotPriceChangedPayload(
    Guid LotId,
    long CurrentPriceUahKopiykas,
    int BidCount,
    DateTime EndsAt)
{
    public const string EventType = "LotPriceChanged";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static LotPriceChangedPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<LotPriceChangedPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("LotPriceChangedPayload deserialization returned null.");
}
