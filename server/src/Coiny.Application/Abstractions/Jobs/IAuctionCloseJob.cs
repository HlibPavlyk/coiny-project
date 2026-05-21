namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Closes an auction at <c>EndsAt</c>: picks the winner, settles WinningBidId, transitions Status,
/// broadcasts SignalR, and writes a <c>SearchOutboxEvent</c> for search-index sync.
/// </summary>
public interface IAuctionCloseJob
{
    Task RunAsync(Guid lotId, CancellationToken ct);
}
