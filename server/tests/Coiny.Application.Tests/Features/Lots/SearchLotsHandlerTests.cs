using Coiny.Application.Abstractions.ExternalServices.Search;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Handlers;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Lots;

/// <summary>
/// Covers <see cref="SearchLotsHandler"/>: the request → <see cref="LotSearchQuery"/> mapping, the
/// default-sort branches, and hit/facet projection. The search engine is faked so these are pure
/// handler-logic tests (no live Meilisearch).
/// </summary>
public class SearchLotsHandlerTests
{
    [Fact]
    public async Task Attribute_filters_map_into_the_search_query()
    {
        var index = new CapturingSearchIndex();
        var handler = new SearchLotsHandler(NewDb(), index);

        var request = new SearchLotsRequest
        {
            Filters = new SearchLotsFilters
            {
                SearchText = "hryvnia",
                Condition = ["VF", "XF"],
                Metal = ["Silver"],
                Country = ["Ukraine"],
                PriceUahKopiykasFrom = 10_000,
                PriceUahKopiykasTo = 100_000,
                EndingBefore = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            Offset = 20,
            Count = 10,
        };

        Result<FacetedPage<LotCardModel>> result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        LotSearchQuery q = index.LastQuery!;
        q.Text.Should().Be("hryvnia");
        q.Conditions.Should().Equal("VF", "XF");
        q.Metals.Should().Equal("Silver");
        q.Countries.Should().Equal("Ukraine");
        q.PriceFrom.Should().Be(10_000);
        q.PriceTo.Should().Be(100_000);
        q.EndingBeforeUnix.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
        q.Offset.Should().Be(20);
        q.Limit.Should().Be(10);
        q.Statuses.Should().BeEquivalentTo("Active", "Sold");
    }

    [Fact]
    public async Task Status_filter_narrows_to_a_single_status()
    {
        var index = new CapturingSearchIndex();
        var handler = new SearchLotsHandler(NewDb(), index);

        await handler.Handle(
            new SearchLotsRequest { Filters = new SearchLotsFilters { Metal = ["Gold"], Status = LotStatus.Sold } },
            CancellationToken.None);

        index.LastQuery!.Statuses.Should().Equal("Sold");
    }

    [Fact]
    public async Task Empty_query_and_empty_sort_default_to_endsAt_asc()
    {
        var index = new CapturingSearchIndex();
        var handler = new SearchLotsHandler(NewDb(), index);

        await handler.Handle(
            new SearchLotsRequest { Filters = new SearchLotsFilters { Metal = ["Silver"] } },
            CancellationToken.None);

        index.LastQuery!.Sort.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new SortByModel("endsAt", SortDirection.Asc));
    }

    [Fact]
    public async Task Text_query_and_empty_sort_defer_to_relevance()
    {
        var index = new CapturingSearchIndex();
        var handler = new SearchLotsHandler(NewDb(), index);

        await handler.Handle(
            new SearchLotsRequest { Filters = new SearchLotsFilters { SearchText = "coin" } },
            CancellationToken.None);

        index.LastQuery!.Sort.Should().BeEmpty();
    }

    [Fact]
    public async Task Explicit_sort_maps_to_allowlisted_fields()
    {
        var index = new CapturingSearchIndex();
        var handler = new SearchLotsHandler(NewDb(), index);

        await handler.Handle(
            new SearchLotsRequest
            {
                Filters = new SearchLotsFilters { SearchText = "coin" },
                SortBy = [new SortByModel("currentPriceUahKopiykas", SortDirection.Desc)],
            },
            CancellationToken.None);

        index.LastQuery!.Sort.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new SortByModel("currentPriceUahKopiykas", SortDirection.Desc));
    }

    [Fact]
    public async Task Unknown_sort_column_is_a_validation_error()
    {
        var handler = new SearchLotsHandler(NewDb(), new CapturingSearchIndex());

        Result<FacetedPage<LotCardModel>> result = await handler.Handle(
            new SearchLotsRequest
            {
                Filters = new SearchLotsFilters { SearchText = "coin" },
                SortBy = [new SortByModel("bogusColumn", SortDirection.Asc)],
            },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Sort.InvalidColumn");
    }

    [Fact]
    public async Task Hits_and_facets_are_projected_into_the_response()
    {
        Guid lotId = Guid.NewGuid();
        long endsAtUnix = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        long createdAtUnix = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();

        var index = new CapturingSearchIndex
        {
            Result = new FacetedPage<LotSearchDocument>(
                TotalCount: 42,
                Items:
                [
                    new LotSearchDocument
                    {
                        Id = lotId.ToString(),
                        Title = "Silver hryvnia",
                        Description = "x",
                        CategoryPath = "Coins",
                        Status = "Active",
                        CategoryId = 1,
                        Condition = "VF",
                        CurrentPriceUahKopiykas = 250_00,
                        EndsAt = endsAtUnix,
                        CreatedAt = createdAtUnix,
                        BidCount = 3,
                        CoverImageUrl = "https://img/cover.jpg",
                    },
                ],
                Facets: new Dictionary<string, IReadOnlyList<FacetValue>>
                {
                    ["metal"] = [new FacetValue("Silver", 3), new FacetValue("Gold", 1)],
                }),
        };

        var handler = new SearchLotsHandler(NewDb(), index);

        Result<FacetedPage<LotCardModel>> result = await handler.Handle(
            new SearchLotsRequest { Filters = new SearchLotsFilters { SearchText = "hryvnia" } },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        FacetedPage<LotCardModel> response = result.Value;
        response.TotalCount.Should().Be(42);
        response.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new LotCardModel
        {
            Id = lotId,
            Title = "Silver hryvnia",
            CoverImageUrl = "https://img/cover.jpg",
            CurrentPriceUahKopiykas = 250_00,
            BidCount = 3,
            EndsAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        response.Facets["metal"].Should().BeEquivalentTo(
            [new FacetValue("Silver", 3), new FacetValue("Gold", 1)]);
        response.Facets.Should().NotContainKey("country");
    }

    private static ApplicationDbContext NewDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    private sealed class CapturingSearchIndex : ISearchIndex
    {
        public LotSearchQuery? LastQuery { get; private set; }

        public FacetedPage<LotSearchDocument> Result { get; init; } =
            new(0, [], new Dictionary<string, IReadOnlyList<FacetValue>>());

        public Task<FacetedPage<LotSearchDocument>> SearchAsync(LotSearchQuery query, CancellationToken ct)
        {
            LastQuery = query;
            return Task.FromResult(Result);
        }

        public Task EnsureIndexAsync(CancellationToken ct) => Task.CompletedTask;
        public Task UpsertLotAsync(LotSearchDocument document, CancellationToken ct) => Task.CompletedTask;
        public Task UpsertLotsAsync(IReadOnlyList<LotSearchDocument> documents, CancellationToken ct) => Task.CompletedTask;
        public Task DeleteLotAsync(Guid lotId, CancellationToken ct) => Task.CompletedTask;
    }
}
