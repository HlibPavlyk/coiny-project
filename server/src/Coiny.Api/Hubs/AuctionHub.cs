using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Coiny.Api.Hubs;

/// <summary>
/// Real-time hub for auction events. Connections are authenticated — anonymous clients are
/// rejected at connect time. Browsing a lot anonymously stays possible; those visitors don't
/// get live updates and the frontend falls back to TanStack Query polling.
///
/// Pure transport: the hub no longer validates lot existence on join. A subscriber to a
/// non-existent group simply receives no broadcasts — the source of truth is whatever
/// <see cref="Realtime.SignalRAuctionNotifier"/> sends to real lot groups after committed writes.
/// </summary>
[Authorize]
public class AuctionHub(ILogger<AuctionHub> logger) : Hub
{
    /// <summary>Subscribe the calling connection to <c>lot:{lotId:N}</c>.</summary>
    public async Task JoinLotGroup(Guid lotId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lotId), Context.ConnectionAborted);
        logger.LogDebug("AuctionHub: connection {Conn} joined {Group}", Context.ConnectionId, GroupName(lotId));
    }

    /// <summary>
    /// Unsubscribe the calling connection from <c>lot:{lotId:N}</c>. Idempotent — leaving a
    /// group the connection isn't in is a no-op.
    /// </summary>
    public async Task LeaveLotGroup(Guid lotId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(lotId), Context.ConnectionAborted);
        logger.LogDebug("AuctionHub: connection {Conn} left {Group}", Context.ConnectionId, GroupName(lotId));
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
            logger.LogInformation(exception, "AuctionHub: connection {Conn} disconnected with error", Context.ConnectionId);
        else
            logger.LogDebug("AuctionHub: connection {Conn} disconnected", Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>Canonical group name. Format mirrors <c>/docs/03-frontend-structure.md</c>.</summary>
    internal static string GroupName(Guid lotId) => $"lot:{lotId:N}";
}
