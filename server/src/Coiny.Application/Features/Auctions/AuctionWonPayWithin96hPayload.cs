using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Auctions;

/// <summary>
/// Email-outbox payload for <c>AuctionWonPayWithin96h</c> emitted by <c>AuctionCloseJob</c>.
/// Consumer: <c>EmailOutboxFlushJob</c> (sprint 3 task 14 wires the Resend send;
/// for now the dispatcher logs it as a TODO and leaves the row unprocessed).
/// </summary>
public sealed record AuctionWonPayWithin96hPayload(
    Guid LotId,
    string LotTitle,
    long AmountUahKopiykas,
    DateTime DueAt)
{
    public const string EventType = "AuctionWonPayWithin96h";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static AuctionWonPayWithin96hPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<AuctionWonPayWithin96hPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("AuctionWonPayWithin96hPayload deserialization returned null.");
}
