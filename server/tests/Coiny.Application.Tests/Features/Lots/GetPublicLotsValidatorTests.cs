using Coiny.Application.Features.Lots.GetPublicLots;
using Coiny.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Coiny.Application.Tests.Features.Lots;

public class GetPublicLotsValidatorTests
{
    private readonly GetPublicLotsValidator _validator = new();

    [Theory]
    [InlineData(LotStatus.Active)]
    [InlineData(LotStatus.Sold)]
    public void Public_status_passes(LotStatus status) =>
        _validator.Validate(new GetPublicLotsRequest { Filters = new GetPublicLotsFilters { Status = status } })
            .IsValid.Should().BeTrue();

    [Theory]
    [InlineData(LotStatus.Draft)]
    [InlineData(LotStatus.EndedNoSale)]
    [InlineData(LotStatus.Cancelled)]
    public void Non_public_status_fails(LotStatus status) =>
        _validator.Validate(new GetPublicLotsRequest { Filters = new GetPublicLotsFilters { Status = status } })
            .IsValid.Should().BeFalse();

    [Fact]
    public void No_status_passes() =>
        _validator.Validate(new GetPublicLotsRequest()).IsValid.Should().BeTrue();
}
