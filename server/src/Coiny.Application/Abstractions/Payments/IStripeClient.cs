namespace Coiny.Application.Abstractions.Payments;

/// <summary>
/// Application-layer seam over the Stripe SDK. The concrete implementation lives
/// in Coiny.Infrastructure and is the only place that touches Stripe.net types.
/// </summary>
public interface IStripeClient
{
    /// <summary>UAH→USD rate locked at PaymentIntent creation. Sourced from <c>Stripe:UahPerUsd</c>.</summary>
    decimal UahPerUsdRate { get; }

    /// <summary>Stripe publishable key, returned to the frontend alongside the client secret.</summary>
    string PublishableKey { get; }

    Task<StripeAccountInfo> CreateConnectAccountAsync(string email, CancellationToken ct);

    Task<StripeAccountLink> CreateOnboardingLinkAsync(string accountId, CancellationToken ct);

    Task<StripeAccountInfo> GetAccountAsync(string accountId, CancellationToken ct);

    /// <summary>
    /// Creates a Connect destination charge PaymentIntent in <c>manual</c> capture mode.
    /// <paramref name="idempotencyKey"/> guards against duplicate intents on retries — typically the lot id.
    /// </summary>
    Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        long usdCents,
        string sellerAccountId,
        IDictionary<string, string> metadata,
        string idempotencyKey,
        CancellationToken ct);
}
