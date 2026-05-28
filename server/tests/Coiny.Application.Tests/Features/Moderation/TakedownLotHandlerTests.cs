using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.Handlers;
using Coiny.Application.Features.Moderation.Requests;
using Coiny.Application.Features.Lots.Events;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Moderation;

public class TakedownLotHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 22, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _adminId = Guid.NewGuid();

    [Fact]
    public async Task Non_admin_is_forbidden()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx, LotStatus.Active);
        await ctx.SaveChangesAsync();

        var handler = new TakedownLotHandler(ctx, NonAdmin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new TakedownLotRequest(lot), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
        (await ctx.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == lot)).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Soft_deletes_active_lot_decrements_category_and_emits_outbox()
    {
        using var ctx = NewDb();
        SeedCategory(ctx, lotCountActive: 3);
        Guid lot = SeedLot(ctx, LotStatus.Active);
        await ctx.SaveChangesAsync();

        var handler = new TakedownLotHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new TakedownLotRequest(lot), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        Lot stored = await ctx.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == lot);
        stored.IsDeleted.Should().BeTrue();
        stored.DeletedAt.Should().Be(BaseTime);
        stored.DeletedByUserId.Should().Be(_adminId);

        (await ctx.Categories.SingleAsync(c => c.Id == 1)).LotCountActive.Should().Be(2);

        SearchOutboxEvent evt = await ctx.SearchOutboxEvents.SingleAsync(e => e.AggregateId == lot);
        evt.EventType.Should().Be(LotDeletedPayload.EventType);
    }

    [Fact]
    public async Task Soft_deletes_sold_lot_without_touching_category_count()
    {
        using var ctx = NewDb();
        SeedCategory(ctx, lotCountActive: 5);
        Guid lot = SeedLot(ctx, LotStatus.Sold);
        await ctx.SaveChangesAsync();

        var handler = new TakedownLotHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new TakedownLotRequest(lot), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await ctx.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == lot)).IsDeleted.Should().BeTrue();
        // A Sold lot was never counted as active — the count must not move.
        (await ctx.Categories.SingleAsync(c => c.Id == 1)).LotCountActive.Should().Be(5);
        (await ctx.SearchOutboxEvents.CountAsync(e => e.AggregateId == lot)).Should().Be(1);
    }

    [Fact]
    public async Task Re_delete_returns_conflict()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx, LotStatus.Active, deleted: true);
        await ctx.SaveChangesAsync();

        var handler = new TakedownLotHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new TakedownLotRequest(lot), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lot.AlreadyDeleted");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Missing_lot_returns_not_found()
    {
        using var ctx = NewDb();
        var handler = new TakedownLotHandler(ctx, Admin(), new TestClock(BaseTime));

        Result result = await handler.Handle(new TakedownLotRequest(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
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

    private TestCurrentUser Admin() => new(_adminId, [RoleNames.Admin]);

    private static TestCurrentUser NonAdmin() => new(Guid.NewGuid(), [RoleNames.User]);

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

    private static Guid SeedLot(ApplicationDbContext ctx, LotStatus status, bool deleted = false)
    {
        Guid id = Guid.NewGuid();
        ctx.Lots.Add(new Lot
        {
            Id = id,
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = "Target lot",
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = 100_00,
            BidCount = 0,
            Status = status,
            StartsAt = BaseTime.AddHours(-2),
            EndsAt = BaseTime.AddHours(2),
            Attributes = "{}",
            IsDeleted = deleted,
            DeletedAt = deleted ? BaseTime.AddHours(-1) : null,
            CreatedAt = BaseTime.AddHours(-2),
            UpdatedAt = BaseTime.AddHours(-2),
        });
        return id;
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
