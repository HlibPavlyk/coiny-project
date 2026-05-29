using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.BanUser;
using Coiny.Application.Features.Moderation.UnbanUser;
using Coiny.Application.Tests.Fakes;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.Features.Moderation;

public class BanUserHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 23, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _targetId = Guid.NewGuid();

    // ── BanUserHandler ─────────────────────────────────────────────────────

    [Fact]
    public async Task Non_admin_is_forbidden()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        await ctx.SaveChangesAsync();

        Result result = await Ban(ctx, NonAdmin()).Handle(new BanUserRequest(_targetId, "spam"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Already_banned_returns_conflict()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId, banned: true);
        await ctx.SaveChangesAsync();

        Result result = await Ban(ctx).Handle(new BanUserRequest(_targetId, "spam"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.AlreadyBanned");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Missing_user_returns_not_found()
    {
        using var ctx = NewDb();

        Result result = await Ban(ctx).Handle(new BanUserRequest(Guid.NewGuid(), "spam"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Ban_with_no_in_flight_state_just_flags_the_user()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        var notifier = new TestNotifier();
        Result result = await Ban(ctx, Admin(), stripe, notifier).Handle(
            new BanUserRequest(_targetId, "spam"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        User stored = await ctx.Users.SingleAsync(u => u.Id == _targetId);
        stored.IsBanned.Should().BeTrue();
        stored.BannedAt.Should().Be(BaseTime);
        stored.BannedByUserId.Should().Be(_adminId);
        stored.BanReason.Should().Be("spam");
        stripe.CancelPaymentIntentCalls.Should().Be(0);
        notifier.Notified.Should().BeEmpty();
    }

    [Fact]
    public async Task Ban_by_moderator_succeeds()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        await ctx.SaveChangesAsync();

        var moderator = new TestCurrentUser(Guid.NewGuid(), [RoleNames.Moderator]);
        Result result = await Ban(ctx, moderator).Handle(new BanUserRequest(_targetId, "spam"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await ctx.Users.SingleAsync(u => u.Id == _targetId)).IsBanned.Should().BeTrue();
    }

    [Fact]
    public async Task Ban_cancels_active_lots_decrements_category_emits_outbox_and_notifies()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        SeedCategory(ctx, lotCountActive: 2);
        Guid lot1 = SeedLot(ctx, _targetId, LotStatus.Active);
        Guid lot2 = SeedLot(ctx, _targetId, LotStatus.Active);
        SeedLot(ctx, _targetId, LotStatus.Sold); // already terminal — must not be touched
        await ctx.SaveChangesAsync();

        var notifier = new TestNotifier();
        Result result = await Ban(ctx, Admin(), new FakeStripeClient(), notifier).Handle(
            new BanUserRequest(_targetId, "fraud"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await ctx.Lots.SingleAsync(l => l.Id == lot1)).Status.Should().Be(LotStatus.Cancelled);
        (await ctx.Lots.SingleAsync(l => l.Id == lot2)).Status.Should().Be(LotStatus.Cancelled);
        (await ctx.Categories.SingleAsync(c => c.Id == 1)).LotCountActive.Should().Be(0);

        var ended = await ctx.SearchOutboxEvents.ToListAsync();
        ended.Select(e => e.AggregateId).Should().BeEquivalentTo([lot1, lot2]);
        notifier.Notified.Should().BeEquivalentTo([lot1, lot2]);
    }

    [Fact]
    public async Task Ban_cancels_buyer_side_in_flight_payment_via_stripe()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        SeedPayment(ctx, buyerId: _targetId, sellerId: Guid.NewGuid(), PaymentStatus.Authorized, "pi_buyer");
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        Result result = await Ban(ctx, Admin(), stripe, new TestNotifier()).Handle(
            new BanUserRequest(_targetId, "fraud"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stripe.CancelPaymentIntentCalls.Should().Be(1);
        stripe.LastCancelledPaymentIntentId.Should().Be("pi_buyer");
        stripe.LastCancelReason.Should().Be("user_banned");
    }

    [Fact]
    public async Task Ban_cancels_seller_side_in_flight_payment_but_skips_terminal_ones()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        SeedPayment(ctx, buyerId: Guid.NewGuid(), sellerId: _targetId, PaymentStatus.PendingAuthorization, "pi_seller");
        SeedPayment(ctx, buyerId: Guid.NewGuid(), sellerId: _targetId, PaymentStatus.Captured, "pi_done"); // terminal
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient();
        Result result = await Ban(ctx, Admin(), stripe, new TestNotifier()).Handle(
            new BanUserRequest(_targetId, "fraud"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stripe.CancelPaymentIntentCalls.Should().Be(1);
        stripe.LastCancelledPaymentIntentId.Should().Be("pi_seller");
    }

    [Fact]
    public async Task Stripe_failure_still_bans_but_returns_502()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        SeedPayment(ctx, buyerId: _targetId, sellerId: Guid.NewGuid(), PaymentStatus.Authorized, "pi_x");
        await ctx.SaveChangesAsync();

        var stripe = new FakeStripeClient { ThrowOnCancel = true };
        Result result = await Ban(ctx, Admin(), stripe, new TestNotifier()).Handle(
            new BanUserRequest(_targetId, "fraud"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Ban.StripeCancelFailed");
        result.Error.Type.Should().Be(ErrorType.ExternalService);
        // The ban itself committed before the Stripe call.
        (await ctx.Users.SingleAsync(u => u.Id == _targetId)).IsBanned.Should().BeTrue();
    }

    // ── UnbanUserHandler ───────────────────────────────────────────────────

    [Fact]
    public async Task Unban_clears_flag_without_restoring_cancelled_lots()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId, banned: true);
        Guid lot = SeedLot(ctx, _targetId, LotStatus.Cancelled);
        await ctx.SaveChangesAsync();

        var handler = new UnbanUserHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new UnbanUserRequest(_targetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        User stored = await ctx.Users.SingleAsync(u => u.Id == _targetId);
        stored.IsBanned.Should().BeFalse();
        stored.BannedAt.Should().BeNull();
        stored.BanReason.Should().BeNull();
        // Cancelled lot stays cancelled — terminal.
        (await ctx.Lots.SingleAsync(l => l.Id == lot)).Status.Should().Be(LotStatus.Cancelled);
    }

    [Fact]
    public async Task Unban_not_banned_user_is_noop_success()
    {
        using var ctx = NewDb();
        SeedUser(ctx, _targetId);
        await ctx.SaveChangesAsync();

        var handler = new UnbanUserHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new UnbanUserRequest(_targetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
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

    private BanUserHandler Ban(
        ApplicationDbContext ctx,
        ICurrentUserService? caller = null,
        FakeStripeClient? stripe = null,
        IAuctionNotifier? notifier = null) =>
        new(ctx, caller ?? Admin(), stripe ?? new FakeStripeClient(), notifier ?? new TestNotifier(),
            new TestClock(BaseTime), NullLogger<BanUserHandler>.Instance);

    private TestCurrentUser Admin() => new(_adminId, [RoleNames.Admin]);

    private static TestCurrentUser NonAdmin() => new(Guid.NewGuid(), [RoleNames.User]);

    private static void SeedUser(ApplicationDbContext ctx, Guid id, bool banned = false)
    {
        ctx.Users.Add(new User
        {
            Id = id,
            DisplayName = "Target",
            UserName = $"{id:N}@example.com",
            Email = $"{id:N}@example.com",
            IsBanned = banned,
            BannedAt = banned ? new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc) : null,
            BanReason = banned ? "earlier" : null,
        });
    }

    private static void SeedCategory(ApplicationDbContext ctx, int lotCountActive)
    {
        ctx.Categories.Add(new Category
        {
            Id = 1,
            Slug = "test",
            Name = "Test",
            Level = 0,
            IsLeaf = true,
            LotCountActive = lotCountActive,
            DisplaySort = 0,
        });
    }

    private static Guid SeedLot(ApplicationDbContext ctx, Guid sellerId, LotStatus status)
    {
        Guid id = Guid.NewGuid();
        ctx.Lots.Add(new Lot
        {
            Id = id,
            SellerId = sellerId,
            CategoryId = 1,
            Title = "Lot",
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = 100_00,
            BidCount = 0,
            Status = status,
            StartsAt = BaseTime.AddHours(-2),
            EndsAt = BaseTime.AddHours(2),
            Attributes = "{}",
            CreatedAt = BaseTime.AddHours(-2),
            UpdatedAt = BaseTime.AddHours(-2),
        });
        return id;
    }

    private static void SeedPayment(
        ApplicationDbContext ctx, Guid buyerId, Guid sellerId, PaymentStatus status, string paymentIntentId)
    {
        ctx.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(),
            LotId = Guid.NewGuid(),
            BuyerId = buyerId,
            SellerId = sellerId,
            AmountUahKopiykas = 1_000_00,
            AmountUsdCents = 2400,
            RateUsedUahPerUsd = 41.5m,
            StripePaymentIntentId = paymentIntentId,
            Status = status,
            DueAt = BaseTime.AddHours(96),
            CreatedAt = BaseTime.AddHours(-1),
            UpdatedAt = BaseTime.AddHours(-1),
        });
    }

    private sealed class TestNotifier : IAuctionNotifier
    {
        public List<Guid> Notified { get; } = [];

        public Task NotifyLotChangedAsync(Guid lotId, CancellationToken ct)
        {
            Notified.Add(lotId);
            return Task.CompletedTask;
        }
    }

    private sealed class TestCurrentUser(Guid? userId, IReadOnlyList<string> roles) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = roles;
    }

    private sealed class TestClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = now;
    }
}
