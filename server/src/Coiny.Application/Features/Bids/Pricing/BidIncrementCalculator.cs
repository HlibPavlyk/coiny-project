namespace Coiny.Application.Features.Bids.Pricing;

/// <summary>
/// Pure lookup of the minimum bid increment per BRD §6.1.
/// All values are kopiykas (1 UAH = 100 kopiykas).
/// Locked tiers — see <see cref="Tiers"/>; any change must be mirrored in BRD §6.1.
/// </summary>
public static class BidIncrementCalculator
{
    // (upperBoundExclusive, increment) — the price is mapped to the first tier where
    // `currentPrice < upperBoundExclusive`. Anything ≥ the last threshold falls into TopTier.
    // UAH ranges from BRD §6.1, × 100 for kopiykas.
    private static readonly (long UpperBoundExclusive, long Increment)[] Tiers =
    [
        (5_000,       100),     // <    50 UAH → 1 UAH
        (20_000,      500),     // <   200 UAH → 5 UAH
        (100_000,     1_000),   // < 1 000 UAH → 10 UAH
        (500_000,     5_000),   // < 5 000 UAH → 50 UAH
        (2_000_000,   10_000),  // < 20 000 UAH → 100 UAH
        (10_000_000,  50_000),  // < 100 000 UAH → 500 UAH
    ];

    private const long TopTierIncrement = 100_000; // ≥ 100 000 UAH → 1 000 UAH increment

    /// <summary>
    /// Returns the minimum next-bid increment for the given current price.
    /// Negative inputs collapse to the smallest tier (defensive — handler should never pass them).
    /// </summary>
    public static long MinIncrement(long currentPriceUahKopiykas)
    {
        foreach ((long upperBoundExclusive, long increment) in Tiers)
        {
            if (currentPriceUahKopiykas < upperBoundExclusive)
            {
                return increment;
            }
        }
        return TopTierIncrement;
    }
}
