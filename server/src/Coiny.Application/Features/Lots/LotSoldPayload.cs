using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Lots;

/// <summary>
/// Outbox payload for <c>LotSold</c> events emitted by <c>AuctionCloseJob</c> when a winner exists.
/// Consumer: Sprint 4 Meilisearch sync removes the lot from the index and freezes its final price.
/// </summary>
public sealed record LotSoldPayload(Guid LotId, Guid WinningBidId, long AmountUahKopiykas)
{
    public const string EventType = "LotSold";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static LotSoldPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<LotSoldPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("LotSoldPayload deserialization returned null.");
}
