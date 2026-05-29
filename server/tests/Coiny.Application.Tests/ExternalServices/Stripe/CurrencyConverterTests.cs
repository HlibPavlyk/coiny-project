using Coiny.Application.Common.Currency;
using FluentAssertions;
using Xunit;

namespace Coiny.Application.Tests.ExternalServices.Stripe;

/// <summary>
/// Locks docs/06-open-questions.md A8: USD rounding at the Stripe boundary uses
/// MidpointRounding.AwayFromZero (NOT banker's rounding). These cases stress the
/// half-cent boundaries where AwayFromZero and ToEven would diverge.
/// </summary>
[Trait("Showcase", "true")]
public class CurrencyConverterTests
{
    [Theory]
    // Zero: nothing to convert.
    [InlineData(0L, "41.5", 0L)]
    // Sub-cent residual (well below half) rounds down to 0 cents.
    [InlineData(10L, "41.5", 0L)]
    // Exact match: 41.5 UAH at rate 41.5 = $1.00.
    [InlineData(4_150L, "41.5", 100L)]
    // Half-cent: 0.005 USD rounds up to 0.01 USD (AwayFromZero).
    // Banker's rounding would give 0.00. Critical to lock the mode.
    [InlineData(50L, "100", 1L)]
    // 50.5-cent stress: 0.505 USD rounds up to 0.51 USD (AwayFromZero).
    // Banker's rounding would give 0.50.
    [InlineData(505L, "10", 51L)]
    // Large value with non-trivial residual.
    // 10 000 000 UAH / 41.5 = 240 963.8554… → 240 963.86 USD → 24 096 386 cents.
    [InlineData(1_000_000_000L, "41.5", 24_096_386L)]
    public void UahKopiykasToUsdCents_rounds_AwayFromZero(
        long uahKopiykas,
        string uahPerUsd,
        long expectedUsdCents)
    {
        decimal rate = decimal.Parse(uahPerUsd, System.Globalization.CultureInfo.InvariantCulture);

        CurrencyConverter
            .UahKopiykasToUsdCents(uahKopiykas, rate)
            .Should()
            .Be(expectedUsdCents);
    }

    [Theory]
    [InlineData(0L, "41.5", 0L)]
    [InlineData(100L, "41.5", 4_150L)]
    public void UsdCentsToUahKopiykas_round_trip_at_exact_values(
        long usdCents,
        string uahPerUsd,
        long expectedUahKopiykas)
    {
        decimal rate = decimal.Parse(uahPerUsd, System.Globalization.CultureInfo.InvariantCulture);

        CurrencyConverter
            .UsdCentsToUahKopiykas(usdCents, rate)
            .Should()
            .Be(expectedUahKopiykas);
    }

    [Fact]
    public void UahKopiykasToUsdCents_throws_on_non_positive_rate()
    {
        Action act = () => CurrencyConverter.UahKopiykasToUsdCents(100L, 0m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
