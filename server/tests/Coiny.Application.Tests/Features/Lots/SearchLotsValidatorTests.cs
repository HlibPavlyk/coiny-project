using Coiny.Application.Features.Lots.SearchLots;
using Coiny.Domain.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Xunit;

namespace Coiny.Application.Tests.Features.Lots;

public class SearchLotsValidatorTests
{
    private readonly SearchLotsValidator _validator = new();

    [Fact]
    public void Inverted_price_range_fails()
    {
        ValidationResult result = _validator.Validate(new SearchLotsRequest
        {
            Filters = new SearchLotsFilters { PriceUahKopiykasFrom = 100_000, PriceUahKopiykasTo = 10_000 },
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Filters.PriceUahKopiykasFrom");
    }

    [Fact]
    public void Equal_price_bounds_pass() =>
        _validator.Validate(new SearchLotsRequest
        {
            Filters = new SearchLotsFilters { PriceUahKopiykasFrom = 50_000, PriceUahKopiykasTo = 50_000 },
        }).IsValid.Should().BeTrue();

    [Fact]
    public void One_sided_price_bound_passes() =>
        _validator.Validate(new SearchLotsRequest
        {
            Filters = new SearchLotsFilters { PriceUahKopiykasFrom = 50_000 },
        }).IsValid.Should().BeTrue();

    [Fact]
    public void Non_public_status_fails() =>
        _validator.Validate(new SearchLotsRequest
        {
            Filters = new SearchLotsFilters { Status = LotStatus.Draft },
        }).IsValid.Should().BeFalse();
}
