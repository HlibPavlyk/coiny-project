using Coiny.Application.Features.Bids.Pricing;
using FluentAssertions;
using Xunit;

namespace Coiny.Application.Tests.Features.Bids;

/// <summary>
/// Locks the BRD §6.1 increment table. Each tier is exercised at:
///   - the very first kopiyka inside the tier (low edge),
///   - one kopiyka before the next tier's boundary (high edge — same tier),
///   - the boundary value itself (must jump to the next tier).
/// Plus zero, top-tier saturation, and a near-MaxValue check.
/// </summary>
public class BidIncrementCalculatorTests
{
    [Theory]
    // Tier 1 (< 50 UAH → 1 UAH increment = 100 kop)
    [InlineData(0L, 100L)]
    [InlineData(1L, 100L)]
    [InlineData(4_999L, 100L)]
    // Tier 2 (50 UAH .. 199.99 UAH → 5 UAH = 500 kop)
    [InlineData(5_000L, 500L)]
    [InlineData(19_999L, 500L)]
    // Tier 3 (200 UAH .. 999.99 UAH → 10 UAH = 1 000 kop)
    [InlineData(20_000L, 1_000L)]
    [InlineData(99_999L, 1_000L)]
    // Tier 4 (1 000 UAH .. 4 999.99 UAH → 50 UAH = 5 000 kop)
    [InlineData(100_000L, 5_000L)]
    [InlineData(499_999L, 5_000L)]
    // Tier 5 (5 000 UAH .. 19 999.99 UAH → 100 UAH = 10 000 kop)
    [InlineData(500_000L, 10_000L)]
    [InlineData(1_999_999L, 10_000L)]
    // Tier 6 (20 000 UAH .. 99 999.99 UAH → 500 UAH = 50 000 kop)
    [InlineData(2_000_000L, 50_000L)]
    [InlineData(9_999_999L, 50_000L)]
    // Top tier (≥ 100 000 UAH → 1 000 UAH = 100 000 kop)
    [InlineData(10_000_000L, 100_000L)]
    [InlineData(1_000_000_000L, 100_000L)]
    [InlineData(long.MaxValue / 2, 100_000L)]
    public void MinIncrement_returns_correct_tier(long currentPriceKopiykas, long expectedIncrement)
    {
        BidIncrementCalculator
            .MinIncrement(currentPriceKopiykas)
            .Should()
            .Be(expectedIncrement);
    }
}
