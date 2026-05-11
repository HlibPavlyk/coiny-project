using Coiny.Application.Abstractions.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Api.Hubs;

/// <summary>
/// Real-time hub for auction events. Connections are authenticated — anonymous clients are
/// rejected at connect time. Browsing a lot anonymously stays possible; those visitors don't
/// get live updates and the frontend falls back to TanStack Query polling.
/// </summary>
[Authorize]
public class AuctionHub(IApplicationDbContext db, ILogger<AuctionHub> logger) : Hub
{
    /// <summary>
    /// Subscribe the calling connection to <c>lot:{lotId:N}</c>. Returns nothing on success;
    /// throws <see cref="HubException"/> when the lot doesn't exist (the client surfaces this
    /// as a thrown promise rejection).
    /// </summary>
    public async Task JoinLotGroup(Guid lotId, CancellationToken ct)
    {
        bool exists = await db.Lots
            .AsNoTracking()
            .AnyAsync(l => l.Id == lotId && !l.IsDeleted, ct);

        if (!exists)
            throw new HubException($"Lot {lotId:N} not found.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lotId), ct);
        logger.LogDebug("AuctionHub: connection {Conn} joined {Group}", Context.ConnectionId, GroupName(lotId));
    }

    /// <summary>
    /// Unsubscribe the calling connection from <c>lot:{lotId:N}</c>. Idempotent — leaving a
    /// group the connection isn't in is a no-op.
    /// </summary>
    public async Task LeaveLotGroup(Guid lotId, CancellationToken ct)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(lotId), ct);
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
