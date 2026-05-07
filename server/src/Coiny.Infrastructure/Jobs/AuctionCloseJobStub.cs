using Coiny.Application.Abstractions.Jobs;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Sprint-1 no-op. Sprint 2 task 09 replaces this with the real auction-close pipeline:
/// pick the winning bid, transition Lot.Status, set WinningBidId, broadcast AuctionClosed via SignalR,
/// write OutboxEvent rows for Meilisearch and email-outbox.
/// </summary>
public class AuctionCloseJobStub(ILogger<AuctionCloseJobStub> logger) : IAuctionCloseJob
{
    public Task RunAsync(Guid lotId, CancellationToken ct)
    {
        logger.LogWarning("AuctionCloseJob: not yet implemented (Sprint 2). Lot={LotId}", lotId);
        return Task.CompletedTask;
    }
}
