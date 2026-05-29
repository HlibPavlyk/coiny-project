using Coiny.Application.Features.Bids.GetMyBids;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Bids;

public class GetMyBidsHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _callerId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();

    [Fact]
    public async Task Anonymous_caller_returns_unauthorized()
    {
        using var ctx = NewDb();
        var handler = new GetMyBidsHandler(ctx, new TestCurrentUser(null));

        Result<Paginated<MyBidItemModel>> result = await handler.Handle(
            new GetMyBidsRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.NotAuthenticated");
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Returns_only_callers_bids()
    {
        using var ctx = NewDb();
        Guid lot1 = SeedLot(ctx, currentPrice: 1_500_00);
        Guid lot2 = SeedLot(ctx, currentPrice: 2_000_00);
        SeedBid(ctx, lot1, _callerId, 1_500_00, BaseTime.AddMinutes(10));
        SeedBid(ctx, lot1, _otherId, 1_400_00, BaseTime.AddMinutes(5)); // someone else's bid — should not appear
        SeedBid(ctx, lot2, _callerId, 2_000_00, BaseTime.AddMinutes(20));
        await ctx.SaveChangesAsync();

        var handler = new GetMyBidsHandler(ctx, new TestCurrentUser(_callerId));
        Result<Paginated<MyBidItemModel>> result = await handler.Handle(
            new GetMyBidsRequest
            {
                SortBy = [new SortByModel("createdAt", SortDirection.Desc)],
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(i => i.Lot.Id).Should().BeEquivalentTo([lot1, lot2]);
    }

    [Fact]
    public async Task IsCallerLeading_is_true_when_bid_equals_lot_current_price()
    {
        using var ctx = NewDb();
        Guid leadingLot = SeedLot(ctx, currentPrice: 1_500_00);
        Guid losingLot = SeedLot(ctx, currentPrice: 3_000_00);

        // Caller's max on leadingLot = 1500 = lot.CurrentPrice → leader
        SeedBid(ctx, leadingLot, _callerId, 1_500_00, BaseTime.AddMinutes(10));
        // Caller has older bid on leadingLot at 1000, no longer leader
        SeedBid(ctx, leadingLot, _callerId, 1_000_00, BaseTime.AddMinutes(5));
        // Caller's bid on losingLot was outbid — currentPrice=3000, caller bid=2500 → not leader
        SeedBid(ctx, losingLot, _callerId, 2_500_00, BaseTime.AddMinutes(15));
        await ctx.SaveChangesAsync();

        var handler = new GetMyBidsHandler(ctx, new TestCurrentUser(_callerId));
        Result<Paginated<MyBidItemModel>> result = await handler.Handle(
            new GetMyBidsRequest
            {
                Count = 50,
                SortBy = [new SortByModel("createdAt", SortDirection.Desc)],
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var leadingRow = result.Value.Items.Single(i => i.AmountUahKopiykas == 1_500_00);
        var oldRow = result.Value.Items.Single(i => i.AmountUahKopiykas == 1_000_00);
        var outbidRow = result.Value.Items.Single(i => i.AmountUahKopiykas == 2_500_00);

        leadingRow.Lot.IsCallerLeading.Should().BeTrue();
        oldRow.Lot.IsCallerLeading.Should().BeFalse();
        outbidRow.Lot.IsCallerLeading.Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_sort_column_returns_validation_error()
    {
        using var ctx = NewDb();
        var handler = new GetMyBidsHandler(ctx, new TestCurrentUser(_callerId));

        Result<Paginated<MyBidItemModel>> result = await handler.Handle(
            new GetMyBidsRequest
            {
                SortBy = [new SortByModel("lotId", SortDirection.Desc)],
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sort.InvalidColumn");
        result.Error.Type.Should().Be(ErrorType.Validation);
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

    private static Guid SeedLot(ApplicationDbContext ctx, long currentPrice)
    {
        Guid id = Guid.NewGuid();
        ctx.Lots.Add(new Lot
        {
            Id = id,
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = $"Lot {id:N}".Substring(0, 12),
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = currentPrice,
            BidCount = 1,
            Status = LotStatus.Active,
            StartsAt = BaseTime.AddHours(-2),
            EndsAt = BaseTime.AddHours(2),
            Attributes = "{}",
            CreatedAt = BaseTime.AddHours(-2),
            UpdatedAt = BaseTime.AddHours(-2),
        });
        return id;
    }

    private static void SeedBid(ApplicationDbContext ctx, Guid lotId, Guid bidderId, long amount, DateTime createdAt)
    {
        ctx.Bids.Add(new Bid
        {
            Id = Guid.NewGuid(),
            LotId = lotId,
            BidderId = bidderId,
            AmountUahKopiykas = amount,
            CreatedAt = createdAt,
        });
    }

    private sealed class TestCurrentUser(Guid? userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = [];
    }
}
