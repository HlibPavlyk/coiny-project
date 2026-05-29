namespace Coiny.Application.Common.Currency;

/// <summary>
/// UAH↔USD conversion at the Stripe boundary. Pure logic, no Stripe.net dependency,
/// so it lives in Application and can be unit-tested in isolation. The rounding mode
/// is locked per docs/06-open-questions.md A8 — never parameterize.
/// </summary>
public static class CurrencyConverter
{
    private const MidpointRounding RoundingMode = MidpointRounding.AwayFromZero;

    public static long UahKopiykasToUsdCents(long uahKopiykas, decimal uahPerUsd)
    {
        if (uahPerUsd <= 0m)
            throw new ArgumentOutOfRangeException(nameof(uahPerUsd), "FX rate must be positive.");

        decimal usd = Math.Round((decimal)uahKopiykas / 100m / uahPerUsd, 2, RoundingMode);
        return (long)(usd * 100m);
    }

    public static long UsdCentsToUahKopiykas(long usdCents, decimal uahPerUsd)
    {
        if (uahPerUsd <= 0m)
            throw new ArgumentOutOfRangeException(nameof(uahPerUsd), "FX rate must be positive.");

        decimal uah = Math.Round((decimal)usdCents / 100m * uahPerUsd, 2, RoundingMode);
        return (long)(uah * 100m);
    }
}
