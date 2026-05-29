using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Demo.CloseLotNow;

/// <summary>
/// Demo handler that mutates the lot's <c>EndsAt</c> into the past so the production
/// <c>AuctionCloseJob</c>'s "not yet ended" guard passes, then enqueues the real job.
///
/// <para>
/// Why mutate state and reuse the real job, instead of executing close-logic inline?
/// Because the thesis demo must show the actual production workflow — same Bid-pick logic, same
/// outbox events, same SignalR broadcast. We only remove the time delay; everything else runs
/// unmodified.
/// </para>
///
/// <para>
/// Why keep this even with 1-minute auctions: the <c>AntiSnipeWindow</c> in <c>PlaceBidHandler</c>
/// is 5 minutes — any bid placed near the end pushes <c>EndsAt</c> to <c>now + 5min</c>, so a
/// short demo auction with even one bid can no longer end naturally within the demo timeframe.
/// This button forces close regardless of anti-snipe extensions.
/// </para>
///
/// Idempotent: rejected with 409 if the lot is already terminal (Sold/EndedNoSale/Cancelled).
/// </summary>
public class CloseLotNowHandler(
    IApplicationDbContext db,
    IJobScheduler jobScheduler,
    IDateTimeProvider clock)
    : IRequestHandler<CloseLotNowRequest, Result>
{
    public async Task<Result> Handle(CloseLotNowRequest request, CancellationToken ct)
    {
        Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == request.LotId, ct);
        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.Status != LotStatus.Active)
            return Result.Failure(Error.Conflict(
                "Demo.LotNotActive",
                $"Lot is in {lot.Status} — only Active lots can be force-closed."));

        // Mutate EndsAt into the past so AuctionCloseJob's `EndsAt > now` guard passes.
        // The job picks the highest bid, transitions to Sold/EndedNoSale, writes outbox events,
        // and broadcasts via SignalR — same code path as the natural time-triggered close.
        lot.EndsAt = clock.UtcNow.AddSeconds(-1);
        lot.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        jobScheduler.EnqueueAuctionCloseNow(lot.Id);
        return Result.Success();
    }
}
