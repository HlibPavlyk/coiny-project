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

    /// <summary>
    /// Capture a previously-authorized manual-capture PaymentIntent. Triggers Stripe's
    /// <c>payment_intent.succeeded</c> webhook, which is the source of truth for
    /// <c>Payment.Status = Captured</c> — this method itself does not mutate local state.
    /// </summary>
    Task<StripePaymentIntentResult> CapturePaymentIntentAsync(string paymentIntentId, CancellationToken ct);

    /// <summary>
    /// Cancel an authorized or pending PaymentIntent. <paramref name="reason"/> follows Stripe's
    /// vocabulary (<c>"abandoned"</c>, <c>"requested_by_customer"</c>, <c>"fraudulent"</c>, …).
    /// The Stripe <c>payment_intent.canceled</c> webhook is the source of truth for
    /// <c>Payment.Status = Cancelled</c>.
    /// </summary>
    Task<StripePaymentIntentResult> CancelPaymentIntentAsync(string paymentIntentId, string? reason, CancellationToken ct);
}
