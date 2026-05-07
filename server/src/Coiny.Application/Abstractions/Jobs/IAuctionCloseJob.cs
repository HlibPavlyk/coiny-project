namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Closes an auction at <c>EndsAt</c>. Sprint 1 ships a no-op stub; Sprint 2 task 09 wires the real logic
/// (pick winner, settle WinningBidId, transition Status, broadcast SignalR, write OutboxEvent).
/// </summary>
public interface IAuctionCloseJob
{
    Task RunAsync(Guid lotId, CancellationToken ct);
}
