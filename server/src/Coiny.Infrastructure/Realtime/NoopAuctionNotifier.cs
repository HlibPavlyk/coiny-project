using Coiny.Application.Abstractions.Realtime;

namespace Coiny.Infrastructure.Realtime;

/// <summary>
/// No-op IAuctionNotifier. Wired by default in sprint 2 task 03; replaced by a SignalR-backed
/// implementation in task 10. Tests rely on this contract — every method must remain side-effect-free.
/// </summary>
public sealed class NoopAuctionNotifier : IAuctionNotifier
{
    public Task BidPlacedAsync(Guid lotId, long currentPriceUahKopiykas, int bidCount, string leaderDisplayName, CancellationToken ct) =>
        Task.CompletedTask;

    public Task AuctionExtendedAsync(Guid lotId, DateTime newEndsAtUtc, CancellationToken ct) =>
        Task.CompletedTask;

    public Task AuctionClosedAsync(Guid lotId, long? finalPriceUahKopiykas, string? winnerDisplayName, CancellationToken ct) =>
        Task.CompletedTask;
}
