using Coiny.Api.Hubs;
using Coiny.Application.Abstractions.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Coiny.Api.Realtime;

/// <summary>
/// SignalR-backed implementation of <see cref="IAuctionNotifier"/>. Each method pushes one event
/// to the lot's group (<c>lot:{lotId:N}</c>) — the same group name <see cref="AuctionHub.JoinLotGroup"/>
/// uses on subscribe.
///
/// Live broadcasts intentionally carry the real bidder display name. Anti-revelation lives in the
/// <c>GetBidHistoryHandler</c> (sprint 2 task 04), not on the live tick — the frontend toast can
/// decide whether to show the name or just the new price.
/// </summary>
public sealed class SignalRAuctionNotifier(IHubContext<AuctionHub> hub) : IAuctionNotifier
{
    public Task BidPlacedAsync(Guid lotId, long currentPriceUahKopiykas, int bidCount, string leaderDisplayName, CancellationToken ct) =>
        hub.Clients
            .Group(AuctionHub.GroupName(lotId))
            .SendAsync("BidPlaced", new
            {
                lotId,
                currentPriceUahKopiykas,
                bidCount,
                leaderDisplayName,
            }, ct);

    public Task AuctionExtendedAsync(Guid lotId, DateTime newEndsAtUtc, CancellationToken ct) =>
        hub.Clients
            .Group(AuctionHub.GroupName(lotId))
            .SendAsync("AuctionExtended", new
            {
                lotId,
                newEndsAt = newEndsAtUtc,
            }, ct);

    public Task AuctionClosedAsync(Guid lotId, long? finalPriceUahKopiykas, string? winnerDisplayName, CancellationToken ct) =>
        hub.Clients
            .Group(AuctionHub.GroupName(lotId))
            .SendAsync("AuctionClosed", new
            {
                lotId,
                finalPriceUahKopiykas,
                winnerDisplayName,
            }, ct);
}
