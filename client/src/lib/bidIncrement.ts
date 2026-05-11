/**
 * Minimum bid increment per BRD §6.1. Client-side mirror of the server's
 * Coiny.Application.Features.Bids.Pricing.BidIncrementCalculator — keep in sync.
 *
 * Values are kopiykas (1 UAH = 100 kopiykas). The lookup walks a sorted array of
 * (upperBoundExclusive, increment); prices ≥ the last threshold saturate to TopTier.
 */
const TIERS: readonly [upperBoundExclusive: number, increment: number][] = [
  [5_000, 100], // <    50 UAH → 1 UAH
  [20_000, 500], // <   200 UAH → 5 UAH
  [100_000, 1_000], // < 1 000 UAH → 10 UAH
  [500_000, 5_000], // < 5 000 UAH → 50 UAH
  [2_000_000, 10_000], // < 20 000 UAH → 100 UAH
  [10_000_000, 50_000], // < 100 000 UAH → 500 UAH
];

const TOP_TIER_INCREMENT = 100_000; // ≥ 100 000 UAH → 1 000 UAH

export function minIncrementKopiykas(currentPriceKopiykas: number): number {
  for (const [upperBoundExclusive, increment] of TIERS) {
    if (currentPriceKopiykas < upperBoundExclusive) return increment;
  }
  return TOP_TIER_INCREMENT;
}

export function minNextBidKopiykas(currentPriceKopiykas: number): number {
  return currentPriceKopiykas + minIncrementKopiykas(currentPriceKopiykas);
}
