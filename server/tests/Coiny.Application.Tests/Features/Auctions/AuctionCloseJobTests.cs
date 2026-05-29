using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Tests.Fakes;
using Coiny.Application.Features.Auctions;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.Features.Auctions;

/// <summary>
/// Logic-level tests for the auction-close pipeline on EF in-memory. Race semantics with concurrent
/// PlaceBidHandler writers are deferred to sprint-4 integration tests with real Postgres.
/// </summary>
[Trait("Showcase", "true")]
public class AuctionCloseJobTests
{
    private static readonly DateTime Now = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _lotId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _bidderA = Guid.NewGuid();
    private readonly Guid _bidderB = Guid.NewGuid();

    [Fact]
    public async Task No_bids_transitions_to_EndedNoSale_and_writes_LotEnded_outbox()
    {
        using var ctx = NewDb();
        SeedCategoryAndUsers(ctx);
        SeedLot(ctx, currentPrice: 1_000_00, endsAt: Now.AddMinutes(-1), status: LotStatus.Active);
        await ctx.SaveChangesAsync();

        Fakes f = NewFakes();
        var job = NewJob(ctx, f);

        await job.RunAsync(_lotId, CancellationToken.None);

        Lot lot = await ctx.Lots.AsNoTracking().FirstAsync(l => l.Id == _lotId);
        lot.Status.Should().Be(LotStatus.EndedNoSale);
        lot.WinningBidId.Should().BeNull();
        lot.AuctionCloseJobId.Should().BeNull();

        ctx.SearchOutboxEvents.Should().ContainSingle(o => o.AggregateId == _lotId);
        ctx.EmailOutboxEvents.Should().BeEmpty();
        (await ctx.Categories.AsNoTracking().FirstAsync(c => c.Id == 1)).LotCountActive.Should().Be(0);
        f.Notifier.NotifyLotChangedCalls.Should().Be(1);
        f.Notifier.LastLotId.Should().Be(_lotId);
    }

    [Fact]
    public async Task Has_bids_transitions_to_Sold_sets_winner_and_writes_email_outbox()
    {
        using var ctx = NewDb();
        SeedCategoryAndUsers(ctx);
        SeedLot(ctx, currentPrice: 1_500_00, endsAt: Now.AddMinutes(-1), status: LotStatus.Active);
        SeedBid(ctx, _bidderA, 1_200_00, Now.AddMinutes(-30));
        Guid winningBidId = SeedBid(ctx, _bidderB, 1_500_00, Now.AddMinutes(-10)); // highest amount
        SeedBid(ctx, _bidderA, 1_300_00, Now.AddMinutes(-5)); // still lower than winner
        await ctx.SaveChangesAsync();

        Fakes f = NewFakes();
        var job = NewJob(ctx, f);

        await job.RunAsync(_lotId, CancellationToken.None);

        Lot lot = await ctx.Lots.AsNoTracking().FirstAsync(l => l.Id == _lotId);
        lot.Status.Should().Be(LotStatus.Sold);
        lot.WinningBidId.Should().Be(winningBidId);
        lot.AuctionCloseJobId.Should().BeNull();

        ctx.SearchOutboxEvents.Should().ContainSingle(o => o.AggregateId == _lotId);
        ctx.EmailOutboxEvents.Should().ContainSingle(e =>
            e.EventType == "AuctionWonPayWithin96h" && e.AggregateId == _bidderB);
        (await ctx.Categories.AsNoTracking().FirstAsync(c => c.Id == 1)).LotCountActive.Should().Be(0);

        // Option A: Payment row is pre-created at close-time with null Stripe intent id so the
        // 96h non-payment deadline is enforceable even for winners who never open the pay form.
        Payment payment = await ctx.Payments.AsNoTracking().FirstAsync(p => p.LotId == _lotId);
        payment.BuyerId.Should().Be(_bidderB);
        payment.SellerId.Should().Be(_sellerId);
        payment.AmountUahKopiykas.Should().Be(1_500_00);
        payment.Status.Should().Be(PaymentStatus.PendingAuthorization);
        payment.StripePaymentIntentId.Should().BeNull();
        payment.DueAt.Should().Be(lot.EndsAt.AddHours(96));

        f.Notifier.NotifyLotChangedCalls.Should().Be(1);
        f.Notifier.LastLotId.Should().Be(_lotId);
    }

    [Fact]
    public async Task No_bids_does_not_create_payment_row()
    {
        // Sanity: the pre-create-on-close path is gated on `topBid != null`. The no-sale branch
        // stays as it was — no Payment row, only the search outbox event.
        using var ctx = NewDb();
        SeedCategoryAndUsers(ctx);
        SeedLot(ctx, currentPrice: 1_000_00, endsAt: Now.AddMinutes(-1), status: LotStatus.Active);
        await ctx.SaveChangesAsync();

        Fakes f = NewFakes();
        var job = NewJob(ctx, f);

        await job.RunAsync(_lotId, CancellationToken.None);

        ctx.Payments.Should().BeEmpty();
    }

    [Fact]
    public async Task Already_terminal_lot_is_a_noop()
    {
        using var ctx = NewDb();
        SeedCategoryAndUsers(ctx);
        SeedLot(ctx, currentPrice: 1_500_00, endsAt: Now.AddMinutes(-1), status: LotStatus.Sold);
        await ctx.SaveChangesAsync();

        Fakes f = NewFakes();
        var job = NewJob(ctx, f);

        await job.RunAsync(_lotId, CancellationToken.None);

        Lot lot = await ctx.Lots.AsNoTracking().FirstAsync(l => l.Id == _lotId);
        lot.Status.Should().Be(LotStatus.Sold); // unchanged
        ctx.SearchOutboxEvents.Should().BeEmpty();
        ctx.EmailOutboxEvents.Should().BeEmpty();
        f.Notifier.NotifyLotChangedCalls.Should().Be(0);
        f.Scheduler.RescheduleCalls.Should().Be(0);
    }

    [Fact]
    public async Task EndsAt_in_future_reschedules_and_returns()
    {
        using var ctx = NewDb();
        SeedCategoryAndUsers(ctx);
        DateTime newEndsAt = Now.AddMinutes(10); // anti-snipe moved it forward
        SeedLot(ctx, currentPrice: 1_000_00, endsAt: newEndsAt, status: LotStatus.Active);
        await ctx.SaveChangesAsync();

        Lot lotEntity = await ctx.Lots.FirstAsync(l => l.Id == _lotId);
        lotEntity.AuctionCloseJobId = "old-job-id";
        await ctx.SaveChangesAsync();

        Fakes f = NewFakes();
        var job = NewJob(ctx, f);

        await job.RunAsync(_lotId, CancellationToken.None);

        Lot lot = await ctx.Lots.AsNoTracking().FirstAsync(l => l.Id == _lotId);
        lot.Status.Should().Be(LotStatus.Active); // unchanged
        lot.AuctionCloseJobId.Should().Be("rescheduled-job-id");

        f.Scheduler.RescheduleCalls.Should().Be(1);
        f.Scheduler.LastPreviousJobId.Should().Be("old-job-id");
        f.Scheduler.LastEndsAt.Should().Be(newEndsAt);
        f.Notifier.NotifyLotChangedCalls.Should().Be(0);
        ctx.SearchOutboxEvents.Should().BeEmpty();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private void SeedCategoryAndUsers(ApplicationDbContext ctx)
    {
        ctx.Categories.Add(new Category
        {
            Id = 1,
            Slug = "test",
            Name = "Test",
            Level = 0,
            IsLeaf = true,
            LotCountActive = 1, // pre-decrement state — close-job should drop to 0
            DisplaySort = 0,
        });
        ctx.Users.Add(new User
        {
            Id = _sellerId, EmailVerified = true, DisplayName = "seller",
            CreatedAt = Now.AddDays(-10), UpdatedAt = Now.AddDays(-10),
        });
        ctx.Users.Add(new User
        {
            Id = _bidderA, EmailVerified = true, DisplayName = "bidder-a",
            CreatedAt = Now.AddDays(-10), UpdatedAt = Now.AddDays(-10),
        });
        ctx.Users.Add(new User
        {
            Id = _bidderB, EmailVerified = true, DisplayName = "bidder-b",
            CreatedAt = Now.AddDays(-10), UpdatedAt = Now.AddDays(-10),
        });
    }

    private void SeedLot(ApplicationDbContext ctx, long currentPrice, DateTime endsAt, LotStatus status)
    {
        ctx.Lots.Add(new Lot
        {
            Id = _lotId,
            SellerId = _sellerId,
            CategoryId = 1,
            Title = "Test lot",
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 1_000_00,
            CurrentPriceUahKopiykas = currentPrice,
            BidCount = 0,
            Status = status,
            StartsAt = Now.AddHours(-2),
            EndsAt = endsAt,
            Attributes = "{}",
            CreatedAt = Now.AddHours(-2),
            UpdatedAt = Now.AddHours(-2),
        });
    }

    private Guid SeedBid(ApplicationDbContext ctx, Guid bidderId, long amount, DateTime createdAt)
    {
        Guid id = Guid.NewGuid();
        ctx.Bids.Add(new Bid
        {
            Id = id,
            LotId = _lotId,
            BidderId = bidderId,
            AmountUahKopiykas = amount,
            CreatedAt = createdAt,
        });
        return id;
    }

    private static AuctionCloseJob NewJob(ApplicationDbContext ctx, Fakes f) =>
        new(ctx, f.Scheduler, f.Notifier, f.Stripe, f.Clock, NullLogger<AuctionCloseJob>.Instance);

    private static Fakes NewFakes() => new(
        new TestScheduler(),
        new TestNotifier(),
        new FakeStripeClient { UahPerUsdRate = 41.5m, PublishableKey = "pk_test_xyz" },
        new TestClock(Now));

    private record Fakes(TestScheduler Scheduler, TestNotifier Notifier, FakeStripeClient Stripe, TestClock Clock);

    private sealed class TestClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class TestScheduler : IJobScheduler
    {
        public int RescheduleCalls { get; private set; }
        public string? LastPreviousJobId { get; private set; }
        public DateTime? LastEndsAt { get; private set; }

        public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) => "scheduled-job-id";

        public string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc)
        {
            RescheduleCalls++;
            LastPreviousJobId = previousJobId;
            LastEndsAt = endsAtUtc;
            return "rescheduled-job-id";
        }

        public string EnqueueCreateTtn(Guid paymentId) => "ttn-job-id";

        public string EnqueueCapture(Guid paymentId) => "capture-job-id";
        public string EnqueueCancelPayment(Guid paymentId) => "cancel-job-id";
        public string EnqueueAuctionCloseNow(Guid lotId) => "close-now-job-id";
        public void TriggerPaymentReminderSweep() { }
        public void TriggerNonPaymentCancelSweep() { }
    }

    private sealed class TestNotifier : IAuctionNotifier
    {
        public int NotifyLotChangedCalls { get; private set; }
        public Guid? LastLotId { get; private set; }

        public Task NotifyLotChangedAsync(Guid lotId, CancellationToken ct)
        {
            NotifyLotChangedCalls++;
            LastLotId = lotId;
            return Task.CompletedTask;
        }
    }
}
