using Coiny.Application.Features.Lots.Models;
using Coiny.Infrastructure.ExternalServices.Search;
using FluentAssertions;
using Xunit;

namespace Coiny.Application.Tests.ExternalServices.Search;

public class MeilisearchLotFilterTests
{
    [Fact]
    public void No_criteria_yields_null()
    {
        MeilisearchLotFilter.Build(new LotSearchQuery()).Should().BeNull();
    }

    [Fact]
    public void Builds_and_combined_clauses_with_in_lists_and_ranges()
    {
        string? filter = MeilisearchLotFilter.Build(new LotSearchQuery
        {
            CategoryIds = [10, 11],
            Statuses = ["Active", "Sold"],
            Conditions = ["VF"],
            Metals = ["Gold", "Silver"],
            Countries = ["Ukraine"],
            PriceFrom = 1_000,
            PriceTo = 5_000,
            EndingBeforeUnix = 1_750_000_000,
        });

        filter.Should().Be(
            "categoryId IN [10, 11] AND " +
            "status IN [\"Active\", \"Sold\"] AND " +
            "condition IN [\"VF\"] AND " +
            "metal IN [\"Gold\", \"Silver\"] AND " +
            "country IN [\"Ukraine\"] AND " +
            "currentPriceUahKopiykas >= 1000 AND " +
            "currentPriceUahKopiykas <= 5000 AND " +
            "endsAt <= 1750000000");
    }

    [Fact]
    public void Escapes_embedded_quotes_in_string_values()
    {
        string? filter = MeilisearchLotFilter.Build(new LotSearchQuery { Metals = ["18\" gold"] });

        filter.Should().Be("metal IN [\"18\\\" gold\"]");
    }
}
