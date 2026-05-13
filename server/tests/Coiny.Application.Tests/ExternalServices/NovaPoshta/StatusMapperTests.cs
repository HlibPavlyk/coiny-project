using Coiny.Domain.Enums;
using Coiny.Infrastructure.ExternalServices.NovaPoshta;
using FluentAssertions;
using Xunit;

namespace Coiny.Application.Tests.ExternalServices.NovaPoshta;

/// <summary>
/// Locks the NP code → ShipmentStatus table from /docs/01-data-model.md §5.
/// Each documented code maps to exactly one status; unknown codes return null
/// so the caller treats it as "no transition".
/// </summary>
public class StatusMapperTests
{
    [Theory]
    [InlineData(1, ShipmentStatus.TtnCreated)]
    [InlineData(2, ShipmentStatus.AcceptedByCarrier)]
    [InlineData(3, ShipmentStatus.AcceptedByCarrier)]
    [InlineData(4, ShipmentStatus.InTransit)]
    [InlineData(5, ShipmentStatus.InTransit)]
    [InlineData(6, ShipmentStatus.InTransit)]
    // Code 7 is intentionally the more advanced ArrivedAtDestination per §5 footnote.
    [InlineData(7, ShipmentStatus.ArrivedAtDestination)]
    [InlineData(8, ShipmentStatus.InTransit)]
    [InlineData(9, ShipmentStatus.Delivered)]
    [InlineData(102, ShipmentStatus.Refused)]
    [InlineData(103, ShipmentStatus.Returned)]
    [InlineData(111, ShipmentStatus.Lost)]
    public void Documented_codes_map_to_expected_status(int npCode, ShipmentStatus expected)
    {
        StatusMapper.TryMap(npCode).Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(200)]
    [InlineData(-1)]
    public void Unknown_codes_return_null(int npCode)
    {
        StatusMapper.TryMap(npCode).Should().BeNull();
    }
}
