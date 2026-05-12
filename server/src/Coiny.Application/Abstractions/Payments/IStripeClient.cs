namespace Coiny.Application.Abstractions.Payments;

/// <summary>
/// Application-layer seam over the Stripe SDK. The concrete implementation lives
/// in Coiny.Infrastructure and is the only place that touches Stripe.net types.
/// </summary>
public interface IStripeClient
{
    Task<StripeAccountInfo> CreateConnectAccountAsync(string email, CancellationToken ct);

    Task<StripeAccountLink> CreateOnboardingLinkAsync(string accountId, CancellationToken ct);

    Task<StripeAccountInfo> GetAccountAsync(string accountId, CancellationToken ct);
}
