namespace Coiny.Application.Abstractions.Presentation.Realtime;

/// <summary>
/// Real-time signal that something about a lot changed (new bid, anti-snipe extension, auction
/// close). Carries only the lot id — receivers re-fetch the authoritative state from REST.
/// Concrete impl is <c>SignalRAuctionNotifier</c> in <c>Coiny.Api/Realtime</c>.
/// </summary>
public interface IAuctionNotifier
{
    /// <summary>Broadcast a "lot changed" signal to subscribers of <c>lot:{lotId:N}</c>.</summary>
    Task NotifyLotChangedAsync(Guid lotId, CancellationToken ct);
}
