using Coiny.Application.Features.Bids.PlaceBid;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Bids;

/// <summary>
/// Logic-level tests for PlaceBidHandler over EF in-memory. Race semantics (SELECT FOR UPDATE)
/// cannot be exercised here — they move to integration tests in sprint 4 backed by real Postgres.
/// </summary>
public class PlaceBidHandlerTests
{
    private static readonly DateTime Now = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _bidderId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _lotId = Guid.NewGuid();

    [Fact]
    public async Task Happy_path_returns_new_bid_response()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LotId.Should().Be(_lotId);
        result.Value.AmountUahKopiykas.Should().Be(1_050_00);
        result.Value.NewCurrentPriceUahKopiykas.Should().Be(1_050_00);
        result.Value.NewBidCount.Should().Be(1);

        ctx.Bids.Should().HaveCount(1);
        ctx.SearchOutboxEvents.Should().ContainSingle(o => o.AggregateId == _lotId);
        m.Notifier.NotifyLotChangedCalls.Should().Be(1);
        m.Notifier.LastLotId.Should().Be(_lotId);
        m.Scheduler.RescheduleCalls.Should().Be(0);
    }

    [Fact]
    public async Task Below_minimum_increment_returns_conflict_outbid()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_010_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Bid.Outbid");
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Description.Should().Be("Outbid while you were typing");
        ctx.Bids.Should().BeEmpty();
    }

    [Fact]
    public async Task Self_bid_returns_forbidden()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_sellerId); // caller IS the seller
        ctx.Users.Add(new User { Id = _sellerId, EmailVerified = true, DisplayName = "seller", CreatedAt = Now, UpdatedAt = Now });
        await ctx.SaveChangesAsync();

        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Bid.SelfBid");
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Banned_bidder_returns_forbidden()
    {
        using var ctx = NewDb();
        ctx.Users.Add(new User
        {
            Id = _bidderId,
            EmailVerified = true,
            IsBanned = true,
            DisplayName = "banned",
            CreatedAt = Now,
            UpdatedAt = Now,
        });
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Bid.Banned");
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Unverified_email_returns_unauthorized()
    {
        using var ctx = NewDb();
        ctx.Users.Add(new User
        {
            Id = _bidderId,
            EmailVerified = false,
            DisplayName = "unverified",
            CreatedAt = Now,
            UpdatedAt = Now,
        });
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Bid.EmailNotVerified");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Lot_not_active_returns_validation()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromHours(2), status: LotStatus.Draft);
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lot.NotActive");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Lot_already_ended_returns_validation()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromMinutes(-1));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lot.AlreadyEnded");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Anti_snipe_extends_endsAt_and_reschedules_when_within_5_minutes()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOffset: TimeSpan.FromMinutes(2));
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NewEndsAt.Should().Be(Now.AddMinutes(5));
        m.Scheduler.RescheduleCalls.Should().Be(1);
        // Extension shares the single NotifyLotChanged broadcast — clients refetch and see the
        // new EndsAt from REST. No separate "extension" event in the thin-push model.
        m.Notifier.NotifyLotChangedCalls.Should().Be(1);
    }

    [Fact]
    public async Task No_extension_when_more_than_5_minutes_remaining()
    {
        using var ctx = NewDb();
        SeedBidder(ctx);
        DateTime originalEndsAt = Now.AddMinutes(30);
        SeedLot(ctx, currentPriceUahKopiykas: 1_000_00, endsAtOverride: originalEndsAt);
        await ctx.SaveChangesAsync();

        Mocks m = MocksFor(_bidderId);
        var handler = new PlaceBidHandler(ctx, m.User, m.Scheduler, m.Notifier, m.Clock);

        Result<PlaceBidModel> result = await handler.Handle(
            new PlaceBidRequest(_lotId, 1_050_00), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NewEndsAt.Should().Be(originalEndsAt);
        m.Scheduler.RescheduleCalls.Should().Be(0);
        m.Notifier.NotifyLotChangedCalls.Should().Be(1);
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

    private void SeedBidder(ApplicationDbContext ctx)
    {
        ctx.Users.Add(new User
        {
            Id = _bidderId,
            EmailVerified = true,
            IsBanned = false,
            DisplayName = "bidder",
            CreatedAt = Now,
            UpdatedAt = Now,
        });
    }

    private void SeedLot(
        ApplicationDbContext ctx,
        long currentPriceUahKopiykas,
        TimeSpan? endsAtOffset = null,
        DateTime? endsAtOverride = null,
        LotStatus status = LotStatus.Active)
    {
        DateTime endsAt = endsAtOverride ?? (Now + (endsAtOffset ?? TimeSpan.FromHours(2)));
        ctx.Lots.Add(new Lot
        {
            Id = _lotId,
            SellerId = _sellerId,
            CategoryId = 1,
            Title = "Test lot",
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 1_000_00,
            CurrentPriceUahKopiykas = currentPriceUahKopiykas,
            BidCount = 0,
            Status = status,
            StartsAt = Now.AddHours(-1),
            EndsAt = endsAt,
            Attributes = "{}",
            CreatedAt = Now.AddHours(-1),
            UpdatedAt = Now.AddHours(-1),
        });
    }

    private static Mocks MocksFor(Guid userId) => new(
        new TestCurrentUser(userId),
        new TestJobScheduler(),
        new TestAuctionNotifier(),
        new TestClock(Now));

    private record Mocks(
        TestCurrentUser User,
        TestJobScheduler Scheduler,
        TestAuctionNotifier Notifier,
        TestClock Clock);

    private sealed class TestCurrentUser(Guid userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = [];
    }

    private sealed class TestClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class TestJobScheduler : IJobScheduler
    {
        public int ScheduleCalls { get; private set; }
        public int RescheduleCalls { get; private set; }
        public string? LastPreviousJobId { get; private set; }

        public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc)
        {
            ScheduleCalls++;
            return "test-job-id";
        }

        public string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc)
        {
            RescheduleCalls++;
            LastPreviousJobId = previousJobId;
            return "test-rescheduled-job-id";
        }

        public string EnqueueCreateTtn(Guid paymentId) => "test-ttn-job-id";

        public string EnqueueCapture(Guid paymentId) => "test-capture-job-id";
        public string EnqueueCancelPayment(Guid paymentId) => "test-cancel-job-id";
    }

    private sealed class TestAuctionNotifier : IAuctionNotifier
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
