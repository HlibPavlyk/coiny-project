using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Handlers;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Bids;

public class GetBidHistoryHandlerTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _lotId = Guid.NewGuid();
    private readonly Guid _bidderA = Guid.NewGuid();
    private readonly Guid _bidderB = Guid.NewGuid();
    private readonly Guid _bidderC = Guid.NewGuid();

    [Fact]
    public async Task Active_lot_anonymizes_bidders_in_first_bid_order()
    {
        using var ctx = NewDb();
        SeedLot(ctx, LotStatus.Active);
        SeedUser(ctx, _bidderA, "alice");
        SeedUser(ctx, _bidderB, "bob");
        SeedUser(ctx, _bidderC, "carol");

        // bidder C first, then A, then B → C='b****1', A='b****2', B='b****3'
        SeedBid(ctx, _bidderC, 1_000_00, BaseTime.AddMinutes(1));
        SeedBid(ctx, _bidderA, 1_100_00, BaseTime.AddMinutes(2));
        SeedBid(ctx, _bidderB, 1_200_00, BaseTime.AddMinutes(3));
        SeedBid(ctx, _bidderC, 1_300_00, BaseTime.AddMinutes(4)); // re-bid: index stays at 1
        await ctx.SaveChangesAsync();

        var handler = new GetBidHistoryHandler(ctx);
        Result<Paginated<BidItemModel>> result = await handler.Handle(
            new GetBidHistoryRequest { LotId = _lotId, Paginate = new PageRequest { Count = 50 } },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(4);

        Dictionary<long, string> byAmount = result.Value.Items.ToDictionary(b => b.AmountUahKopiykas, b => b.BidderDisplay);
        byAmount[1_000_00].Should().Be("b****1"); // first bidder = C
        byAmount[1_100_00].Should().Be("b****2"); // second bidder = A
        byAmount[1_200_00].Should().Be("b****3"); // third bidder = B
        byAmount[1_300_00].Should().Be("b****1"); // C re-bidding retains its index
    }

    [Fact]
    public async Task Sold_lot_returns_full_display_names()
    {
        using var ctx = NewDb();
        SeedLot(ctx, LotStatus.Sold);
        SeedUser(ctx, _bidderA, "alice");
        SeedUser(ctx, _bidderB, "bob");
        SeedBid(ctx, _bidderA, 1_000_00, BaseTime.AddMinutes(1));
        SeedBid(ctx, _bidderB, 1_100_00, BaseTime.AddMinutes(2));
        await ctx.SaveChangesAsync();

        var handler = new GetBidHistoryHandler(ctx);
        Result<Paginated<BidItemModel>> result = await handler.Handle(
            new GetBidHistoryRequest { LotId = _lotId, Paginate = new PageRequest { Count = 50 } },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Select(i => i.BidderDisplay).Should().BeEquivalentTo(["alice", "bob"]);
    }

    [Fact]
    public async Task Explicit_amount_descending_sort_orders_items_top_bid_first()
    {
        using var ctx = NewDb();
        SeedLot(ctx, LotStatus.Active);
        SeedUser(ctx, _bidderA, "alice");
        SeedBid(ctx, _bidderA, 1_000_00, BaseTime.AddMinutes(1));
        SeedBid(ctx, _bidderA, 1_300_00, BaseTime.AddMinutes(2));
        SeedBid(ctx, _bidderA, 1_200_00, BaseTime.AddMinutes(3));
        await ctx.SaveChangesAsync();

        var handler = new GetBidHistoryHandler(ctx);
        Result<Paginated<BidItemModel>> result = await handler.Handle(
            new GetBidHistoryRequest
            {
                LotId = _lotId,
                Paginate = new PageRequest
                {
                    SortBy = [new SortByModel("amountUahKopiykas", SortDirection.Desc)],
                },
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Select(i => i.AmountUahKopiykas)
            .Should().ContainInOrder(1_300_00L, 1_200_00L, 1_000_00L);
    }

    [Fact]
    public async Task Unknown_sort_column_returns_validation_error()
    {
        using var ctx = NewDb();
        SeedLot(ctx, LotStatus.Active);
        await ctx.SaveChangesAsync();

        var handler = new GetBidHistoryHandler(ctx);
        Result<Paginated<BidItemModel>> result = await handler.Handle(
            new GetBidHistoryRequest
            {
                LotId = _lotId,
                Paginate = new PageRequest
                {
                    SortBy = [new SortByModel("bidderId", SortDirection.Asc)],
                },
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sort.InvalidColumn");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Missing_lot_returns_not_found()
    {
        using var ctx = NewDb();
        var handler = new GetBidHistoryHandler(ctx);

        Result<Paginated<BidItemModel>> result = await handler.Handle(
            new GetBidHistoryRequest { LotId = Guid.NewGuid(), Paginate = new PageRequest() },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Lot.NotFound");
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

    private void SeedLot(ApplicationDbContext ctx, LotStatus status)
    {
        ctx.Lots.Add(new Lot
        {
            Id = _lotId,
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = "Test lot",
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
    }

    private static void SeedUser(ApplicationDbContext ctx, Guid id, string displayName)
    {
        ctx.Users.Add(new User
        {
            Id = id,
            EmailVerified = true,
            DisplayName = displayName,
            CreatedAt = BaseTime.AddDays(-30),
            UpdatedAt = BaseTime.AddDays(-30),
        });
    }

    private void SeedBid(ApplicationDbContext ctx, Guid bidderId, long amount, DateTime createdAt)
    {
        ctx.Bids.Add(new Bid
        {
            Id = Guid.NewGuid(),
            LotId = _lotId,
            BidderId = bidderId,
            AmountUahKopiykas = amount,
            CreatedAt = createdAt,
        });
    }
}
