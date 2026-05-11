namespace Coiny.Application.Abstractions.Realtime;

/// <summary>
/// Real-time broadcaster for auction events. SignalR-backed in production
/// (<c>SignalRAuctionNotifier</c>, task 10); a no-op stub is wired in sprint 2 task 03
/// so handlers can call this without depending on SignalR yet.
/// </summary>
public interface IAuctionNotifier
{
    /// <summary>Broadcast <c>BidPlaced</c> to subscribers of <c>lot:{lotId}</c>.</summary>
    Task BidPlacedAsync(Guid lotId, long currentPriceUahKopiykas, int bidCount, string leaderDisplayName, CancellationToken ct);

    /// <summary>Broadcast <c>AuctionExtended</c> after an anti-snipe extension moved <c>EndsAt</c>.</summary>
    Task AuctionExtendedAsync(Guid lotId, DateTime newEndsAtUtc, CancellationToken ct);

    /// <summary>Broadcast <c>AuctionClosed</c> with the final price + winner (if any).</summary>
    Task AuctionClosedAsync(Guid lotId, long? finalPriceUahKopiykas, string? winnerDisplayName, CancellationToken ct);
}
