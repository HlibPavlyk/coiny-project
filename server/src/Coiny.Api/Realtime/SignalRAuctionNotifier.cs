using Coiny.Application.Abstractions.Presentation.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Coiny.Api.Realtime;

/// <summary>
/// SignalR-backed implementation of <see cref="IAuctionNotifier"/>. Pushes a single
/// <c>LotChanged</c> event with only the lot id; receivers re-fetch the lot + bid history from
/// REST. Keeping the payload minimal eliminates duplicate state on the client and removes whole
/// classes of stale-payload race conditions.
/// </summary>
public sealed class SignalRAuctionNotifier(IHubContext<AuctionHub> hub) : IAuctionNotifier
{
    public Task NotifyLotChangedAsync(Guid lotId, CancellationToken ct) =>
        hub.Clients
            .Group(AuctionHub.GroupName(lotId))
            .SendAsync("LotChanged", new { lotId }, ct);
}
