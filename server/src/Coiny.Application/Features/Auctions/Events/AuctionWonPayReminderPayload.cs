using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Auctions.Events;

/// <summary>
/// Email-outbox payload for the 48h won-pay reminder. Same data shape as
/// <see cref="AuctionWonPayWithin96hPayload"/> — task 14 reuses the won-pay template with a
/// "REMINDER:" banner per <c>THESIS-SCOPE.md</c> §1 §15.
/// </summary>
public sealed record AuctionWonPayReminderPayload(
    Guid PaymentId,
    Guid LotId,
    string LotTitle,
    long AmountUahKopiykas,
    DateTime DueAt)
{
    public const string EventType = "AuctionWonPayReminder";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static AuctionWonPayReminderPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<AuctionWonPayReminderPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("AuctionWonPayReminderPayload deserialization returned null.");
}
