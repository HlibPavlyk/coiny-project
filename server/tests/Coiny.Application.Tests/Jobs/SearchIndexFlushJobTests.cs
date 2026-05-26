using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Search;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Coiny.Application.Tests.Jobs;

public class SearchIndexFlushJobTests
{
    private static readonly DateTime Now = new(2026, 5, 21, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(LotStatus.Active)]
    [InlineData(LotStatus.Sold)]
    public async Task Active_or_sold_lot_is_upserted_and_event_marked_processed(LotStatus status)
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, status);
        SeedOutbox(ctx, lot.Id);
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        index.Upserts.Should().ContainSingle(d => d.Id == lot.Id.ToString());
        index.Deletes.Should().BeEmpty();

        SearchOutboxEvent row = await ctx.SearchOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().Be(Now);
        row.AttemptCount.Should().Be(0);
    }

    [Fact]
    public async Task Projects_country_year_metal_and_category_path_from_jsonb()
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, LotStatus.Active);
        SeedOutbox(ctx, lot.Id);
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        LotSearchDocument doc = index.Upserts.Single();
        doc.Country.Should().Be("Ukraine");
        doc.Year.Should().Be("1992");
        doc.Metal.Should().Be("Silver");
        doc.CategoryPath.Should().Be("Coins");
        doc.BidCount.Should().Be(lot.BidCount);
    }

    [Theory]
    [InlineData(LotStatus.EndedNoSale)]
    [InlineData(LotStatus.Cancelled)]
    [InlineData(LotStatus.Draft)]
    public async Task Non_indexable_status_deletes_from_index(LotStatus status)
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, status);
        SeedOutbox(ctx, lot.Id);
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        index.Deletes.Should().ContainSingle(id => id == lot.Id);
        index.Upserts.Should().BeEmpty();
        (await ctx.SearchOutboxEvents.SingleAsync()).ProcessedAt.Should().Be(Now);
    }

    [Fact]
    public async Task Missing_lot_deletes_from_index()
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Guid ghostLotId = Guid.NewGuid();
        SeedOutbox(ctx, ghostLotId); // no Lot row for this id
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        index.Deletes.Should().ContainSingle(id => id == ghostLotId);
        index.Upserts.Should().BeEmpty();
    }

    [Fact]
    public async Task Multiple_events_for_same_lot_reconcile_once_and_all_processed()
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, LotStatus.Active);
        SeedOutbox(ctx, lot.Id, "LotPublished");
        SeedOutbox(ctx, lot.Id, "LotPriceChanged");
        SeedOutbox(ctx, lot.Id, "LotPriceChanged");
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        // Reconciled once against current state, not three times.
        index.Upserts.Should().HaveCount(1);
        // But all three outbox rows are marked processed.
        (await ctx.SearchOutboxEvents.CountAsync(e => e.ProcessedAt != null)).Should().Be(3);
    }

    [Fact]
    public async Task Already_processed_rows_are_skipped()
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, LotStatus.Active);
        SearchOutboxEvent done = SeedOutbox(ctx, lot.Id);
        done.ProcessedAt = Now.AddMinutes(-5);
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex();
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        index.Upserts.Should().BeEmpty();
        index.Deletes.Should().BeEmpty();
    }

    [Fact]
    public async Task Index_failure_leaves_row_unprocessed_with_error()
    {
        using var ctx = NewDb();
        SeedCategory(ctx);
        Lot lot = SeedLot(ctx, LotStatus.Active);
        SeedOutbox(ctx, lot.Id);
        await ctx.SaveChangesAsync();

        var index = new RecordingSearchIndex { ThrowOnUpsert = true };
        await NewJob(ctx, index).RunAsync(CancellationToken.None);

        SearchOutboxEvent row = await ctx.SearchOutboxEvents.SingleAsync();
        row.ProcessedAt.Should().BeNull();
        row.AttemptCount.Should().Be(1);
        row.LastError.Should().NotBeNull();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static SearchIndexFlushJob NewJob(ApplicationDbContext ctx, ISearchIndex index) =>
        new(ctx, index, new FixedClock(Now), NullLogger<SearchIndexFlushJob>.Instance);

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedCategory(ApplicationDbContext ctx) =>
        ctx.Categories.Add(new Category
        {
            Id = 1, Slug = "coins", Name = "Coins", Level = 0, IsLeaf = true, DisplaySort = 0,
        });

    private static Lot SeedLot(ApplicationDbContext ctx, LotStatus status)
    {
        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = "1 hryvnia 1992",
            Description = "A coin",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = 150_00,
            Status = status,
            StartsAt = Now.AddDays(-2),
            EndsAt = Now.AddDays(-1),
            Attributes = """{"country":"Ukraine","year":"1992","metal":"Silver"}""",
            CreatedAt = Now.AddDays(-2),
            UpdatedAt = Now.AddDays(-1),
        };
        ctx.Lots.Add(lot);
        return lot;
    }

    private static SearchOutboxEvent SeedOutbox(ApplicationDbContext ctx, Guid lotId, string eventType = "LotPublished")
    {
        var evt = new SearchOutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lotId,
            EventType = eventType,
            Payload = "{}",
            CreatedAt = Now,
        };
        ctx.SearchOutboxEvents.Add(evt);
        return evt;
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class RecordingSearchIndex : ISearchIndex
    {
        public List<LotSearchDocument> Upserts { get; } = [];
        public List<Guid> Deletes { get; } = [];
        public bool ThrowOnUpsert { get; init; }

        public Task EnsureIndexAsync(CancellationToken ct) => Task.CompletedTask;

        public Task UpsertLotAsync(LotSearchDocument document, CancellationToken ct)
        {
            if (ThrowOnUpsert)
                throw new InvalidOperationException("meili down");
            Upserts.Add(document);
            return Task.CompletedTask;
        }

        public Task UpsertLotsAsync(IReadOnlyList<LotSearchDocument> documents, CancellationToken ct)
        {
            Upserts.AddRange(documents);
            return Task.CompletedTask;
        }

        public Task DeleteLotAsync(Guid lotId, CancellationToken ct)
        {
            Deletes.Add(lotId);
            return Task.CompletedTask;
        }

        public Task<FacetedPage<LotSearchDocument>> SearchAsync(
            LotSearchQuery query, CancellationToken ct) =>
            Task.FromResult(new FacetedPage<LotSearchDocument>(
                0, [], new Dictionary<string, IReadOnlyList<FacetValue>>()));
    }
}
