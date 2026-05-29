using System.Data;
using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Common.Currency;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Coiny.Application.Features.Auctions.Events;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Fires when an auction's <c>EndsAt</c> elapses. Picks the highest accepted bid, transitions the
/// lot to <c>Sold</c>/<c>EndedNoSale</c>, writes outbox events for Meilisearch sync and the won-pay
/// email, decrements the category's active count, and broadcasts <c>AuctionClosed</c> via SignalR.
///
/// Idempotent on every entry point — re-fire (Hangfire retry or dashboard "Trigger") is safe because
/// the early <c>Status != Active</c> guard short-circuits already-closed lots, and the EndsAt-future
/// branch reschedules instead of acting on a still-open auction.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public class AuctionCloseJob(
    IApplicationDbContext db,
    IJobScheduler jobScheduler,
    IAuctionNotifier notifier,
    IStripeClient stripe,
    IDateTimeProvider clock,
    ILogger<AuctionCloseJob> logger) : IAuctionCloseJob
{
    private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

    /// <summary>96h window the winning bidder has to pay (THESIS-SCOPE §B / §F).</summary>
    private static readonly TimeSpan _paymentWindow = TimeSpan.FromHours(96);

    public async Task RunAsync(Guid lotId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        Lot? lot = await LoadAndLockLot(lotId, ct);
        if (lot is null)
        {
            logger.LogWarning("AuctionCloseJob: lot {LotId} not found", lotId);
            return;
        }

        if (lot.Status != LotStatus.Active)
        {
            logger.LogInformation("AuctionCloseJob: lot {LotId} already terminal (status={Status}) — no-op",
                lotId, lot.Status);
            return;
        }

        DateTime now = clock.UtcNow;

        if (lot.EndsAt > now)
        {
            // Anti-snipe extended EndsAt after this job was scheduled. Re-arm and exit.
            lot.AuctionCloseJobId = jobScheduler.ReScheduleAuctionClose(lot.AuctionCloseJobId, lot.Id, lot.EndsAt);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("AuctionCloseJob: lot {LotId} EndsAt moved to {EndsAt} — rescheduled",
                lotId, lot.EndsAt);
            return;
        }

        // Pick winner: highest amount, earliest CreatedAt on tie.
        Bid? topBid = await db.Bids
            .AsNoTracking()
            .Where(b => b.LotId == lotId)
            .OrderByDescending(b => b.AmountUahKopiykas)
            .ThenBy(b => b.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (topBid is null)
        {
            lot.Status = LotStatus.EndedNoSale;
            lot.UpdatedAt = now;

            db.SearchOutboxEvents.Add(new SearchOutboxEvent
            {
                AggregateType = nameof(Lot),
                AggregateId = lot.Id,
                CreatedAt = now,
            });
        }
        else
        {
            lot.Status = LotStatus.Sold;
            lot.WinningBidId = topBid.Id;
            lot.UpdatedAt = now;

            db.SearchOutboxEvents.Add(new SearchOutboxEvent
            {
                AggregateType = nameof(Lot),
                AggregateId = lot.Id,
                CreatedAt = now,
            });

            // Pre-create the Payment row so the 96h non-payment deadline is enforceable even for
            // winners who never open the checkout. StripePaymentIntentId stays null until the
            // buyer first opens PayLotPage (CreatePaymentIntentHandler updates this row instead
            // of inserting a new one). DueAt is anchored to the close moment, not to when the
            // buyer eventually engages — fairness across procrastinators.
            decimal rate = stripe.UahPerUsdRate;
            long amountUsdCents = CurrencyConverter.UahKopiykasToUsdCents(topBid.AmountUahKopiykas, rate);
            DateTime dueAt = lot.EndsAt + _paymentWindow;

            db.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                LotId = lot.Id,
                BuyerId = topBid.BidderId,
                SellerId = lot.SellerId,
                AmountUahKopiykas = topBid.AmountUahKopiykas,
                AmountUsdCents = amountUsdCents,
                RateUsedUahPerUsd = rate,
                StripePaymentIntentId = null, // populated by CreatePaymentIntentHandler on first open
                Status = PaymentStatus.PendingAuthorization,
                DueAt = dueAt,
                CreatedAt = now,
                UpdatedAt = now,
            });

            db.EmailOutboxEvents.Add(new EmailOutboxEvent
            {
                AggregateType = nameof(User),
                AggregateId = topBid.BidderId,
                EventType = AuctionWonPayWithin96hPayload.EventType,
                Payload = new AuctionWonPayWithin96hPayload(
                    lot.Id, lot.Title, topBid.AmountUahKopiykas, dueAt).Serialize(),
                CreatedAt = now,
            });
        }

        await DecrementCategoryActiveCount(lot.CategoryId, ct);

        // The auction is no longer scheduled — clear the tracking field.
        lot.AuctionCloseJobId = null;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // SignalR broadcast is best-effort and lives outside the transaction. A failure logs but
        // doesn't undo the DB transition — the source of truth is the committed Lot row.
        try
        {
            await notifier.NotifyLotChangedAsync(lot.Id, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AuctionCloseJob: notifier.NotifyLotChangedAsync failed for lot {LotId}", lotId);
        }
    }

    /// <summary>
    /// On Postgres, takes a SELECT … FOR UPDATE row lock to serialize against concurrent
    /// <c>PlaceBidHandler</c> writers. On other providers (EF in-memory tests), falls back to
    /// plain LINQ — the lock semantics are exercised in sprint-4 integration tests with a real DB.
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

    private async Task DecrementCategoryActiveCount(int categoryId, CancellationToken ct)
    {
        // EF Core 10's in-memory provider rejects ExecuteUpdate/ExecuteUpdateAsync with
        // "The methods 'ExecuteUpdate' and 'ExecuteUpdateAsync' are not supported by the current
        // database provider". Postgres takes the fast set-based path; tests fall back to
        // load-modify-track and exercise the same final state.
        if (db.Database.ProviderName == NpgsqlProviderName)
        {
            await db.Categories
                .Where(c => c.Id == categoryId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.LotCountActive, c => c.LotCountActive - 1), ct);
            return;
        }

        Category? cat = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (cat is not null)
        {
            cat.LotCountActive -= 1;
        }
    }
}
