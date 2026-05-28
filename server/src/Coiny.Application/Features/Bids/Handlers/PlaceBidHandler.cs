using System.Data;
using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Pricing;
using Coiny.Application.Features.Bids.Requests;
using Coiny.Application.Features.Lots;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Coiny.Application.Features.Lots.Events;

namespace Coiny.Application.Features.Bids.Handlers;

public class PlaceBidHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IJobScheduler jobScheduler,
    IAuctionNotifier notifier,
    IDateTimeProvider clock)
    : IRequestHandler<PlaceBidRequest, Result<PlaceBidModel>>
{
    private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private static readonly TimeSpan AntiSnipeWindow = TimeSpan.FromMinutes(5);

    public async Task<Result<PlaceBidModel>> Handle(PlaceBidRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } bidderId)
            return Result.Failure<PlaceBidModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        User? bidder = await db.Users.FirstOrDefaultAsync(u => u.Id == bidderId, ct);
        if (bidder is null)
            return Result.Failure<PlaceBidModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        if (!bidder.EmailVerified)
            return Result.Failure<PlaceBidModel>(Error.Unauthorized("Bid.EmailNotVerified", "Verify your email before placing bids."));

        if (bidder.IsBanned)
            return Result.Failure<PlaceBidModel>(Error.Forbidden("Bid.Banned", "Your account is banned from bidding."));

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        Lot? lot = await LoadAndLockLot(request.LotId, ct);
        if (lot is null)
            return Result.Failure<PlaceBidModel>(Error.NotFound("Lot.NotFound", "Lot not found."));

        DateTime now = clock.UtcNow;

        if (lot.Status != LotStatus.Active)
            return Result.Failure<PlaceBidModel>(Error.Validation("Lot.NotActive", "Bids are only accepted on active lots."));

        if (lot.EndsAt <= now)
            return Result.Failure<PlaceBidModel>(Error.Validation("Lot.AlreadyEnded", "Bidding window has closed."));

        if (lot.StartsAt > now)
            return Result.Failure<PlaceBidModel>(Error.Validation("Lot.NotStarted", "Auction has not started yet."));

        if (lot.SellerId == bidderId)
            return Result.Failure<PlaceBidModel>(Error.Forbidden("Bid.SelfBid", "Sellers cannot bid on their own lots."));

        long minIncrement = BidIncrementCalculator.MinIncrement(lot.CurrentPriceUahKopiykas);
        long minBid = lot.CurrentPriceUahKopiykas + minIncrement;

        if (request.AmountUahKopiykas < minBid)
            return Result.Failure<PlaceBidModel>(Error.Conflict("Bid.Outbid", "Outbid while you were typing"));

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            LotId = lot.Id,
            BidderId = bidderId,
            AmountUahKopiykas = request.AmountUahKopiykas,
            CreatedAt = now,
        };
        db.Bids.Add(bid);

        lot.CurrentPriceUahKopiykas = request.AmountUahKopiykas;
        lot.BidCount += 1;
        lot.UpdatedAt = now;

        bool extended = lot.EndsAt - now < AntiSnipeWindow;
        if (extended)
        {
            lot.EndsAt = now + AntiSnipeWindow;
            lot.AuctionCloseJobId = jobScheduler.ReScheduleAuctionClose(lot.AuctionCloseJobId, lot.Id, lot.EndsAt);
        }

        var payload = new LotPriceChangedPayload(lot.Id, lot.CurrentPriceUahKopiykas, lot.BidCount, lot.EndsAt);
        db.SearchOutboxEvents.Add(new SearchOutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lot.Id,
            EventType = LotPriceChangedPayload.EventType,
            Payload = payload.Serialize(),
            CreatedAt = now,
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // Signal subscribers that the lot changed — they re-fetch authoritative state from REST.
        await notifier.NotifyLotChangedAsync(lot.Id, ct);

        return Result.Success(new PlaceBidModel(
            bid.Id,
            lot.Id,
            bid.AmountUahKopiykas,
            lot.CurrentPriceUahKopiykas,
            lot.BidCount,
            lot.EndsAt));
    }

    /// <summary>
    /// On Postgres: <c>SELECT … FOR UPDATE</c> the row to serialize concurrent bidders.
    /// On other providers (EF in-memory tests): plain LINQ — race coverage moves to integration tests
    /// (sprint 4) where a real Postgres backs the test.
    /// </summary>
    private async Task<Lot?> LoadAndLockLot(Guid lotId, CancellationToken ct)
    {
        if (db.Database.ProviderName == NpgsqlProviderName)
        {
            return await db.Lots
                .FromSqlInterpolated($"SELECT * FROM \"Lots\" WHERE \"Id\" = {lotId} AND \"IsDeleted\" = false FOR UPDATE")
                .FirstOrDefaultAsync(ct);
        }
        return await db.Lots.FirstOrDefaultAsync(l => l.Id == lotId && !l.IsDeleted, ct);
    }
}
