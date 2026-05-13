using Coiny.Application.Abstractions.Payments;

namespace Coiny.Application.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IStripeClient"/> double for handler tests. Counts calls and
/// returns deterministic identifiers; never hits the real Stripe API.
/// </summary>
public sealed class FakeStripeClient : IStripeClient
{
    public decimal UahPerUsdRate { get; init; } = 41.5m;
    public string PublishableKey { get; init; } = "pk_test_fake";

    public int CreateAccountCalls { get; private set; }
    public int CreateLinkCalls { get; private set; }
    public int GetAccountCalls { get; private set; }
    public int CreatePaymentIntentCalls { get; private set; }

    public string NextAccountId { get; set; } = "acct_test_001";
    public bool GetAccountReturnsOnboarded { get; set; } = true;
    public string NextPaymentIntentId { get; set; } = "pi_test_001";
    public string NextPaymentIntentClientSecret { get; set; } = "pi_test_001_secret_xyz";
    public string NextPaymentIntentStatus { get; set; } = "requires_payment_method";

    public string? LastIdempotencyKey { get; private set; }
    public long LastUsdCents { get; private set; }
    public string? LastSellerAccountId { get; private set; }
    public IDictionary<string, string>? LastMetadata { get; private set; }

    public Task<StripeAccountInfo> CreateConnectAccountAsync(string email, CancellationToken ct)
    {
        CreateAccountCalls++;
        return Task.FromResult(new StripeAccountInfo(
            Id: NextAccountId,
            DetailsSubmitted: false,
            ChargesEnabled: false,
            RequirementsCurrentlyDue: new[] { "business_profile.url" }));
    }

    public Task<StripeAccountLink> CreateOnboardingLinkAsync(string accountId, CancellationToken ct)
    {
        CreateLinkCalls++;
        return Task.FromResult(new StripeAccountLink(
            Url: $"https://connect.stripe.com/setup/{accountId}/{Guid.NewGuid():N}",
            ExpiresAt: DateTime.UtcNow.AddMinutes(5)));
    }

    public Task<StripeAccountInfo> GetAccountAsync(string accountId, CancellationToken ct)
    {
        GetAccountCalls++;
        return Task.FromResult(new StripeAccountInfo(
            Id: accountId,
            DetailsSubmitted: GetAccountReturnsOnboarded,
            ChargesEnabled: GetAccountReturnsOnboarded,
            RequirementsCurrentlyDue: Array.Empty<string>()));
    }

    public Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        long usdCents,
        string sellerAccountId,
        IDictionary<string, string> metadata,
        string idempotencyKey,
        CancellationToken ct)
    {
        CreatePaymentIntentCalls++;
        LastIdempotencyKey = idempotencyKey;
        LastUsdCents = usdCents;
        LastSellerAccountId = sellerAccountId;
        LastMetadata = new Dictionary<string, string>(metadata);

        return Task.FromResult(new StripePaymentIntentResult(
            Id: NextPaymentIntentId,
            Status: NextPaymentIntentStatus,
            ClientSecret: NextPaymentIntentClientSecret));
    }

    public int CapturePaymentIntentCalls { get; private set; }
    public string? LastCapturedPaymentIntentId { get; private set; }

    public Task<StripePaymentIntentResult> CapturePaymentIntentAsync(string paymentIntentId, CancellationToken ct)
    {
        CapturePaymentIntentCalls++;
        LastCapturedPaymentIntentId = paymentIntentId;
        return Task.FromResult(new StripePaymentIntentResult(
            Id: paymentIntentId,
            Status: "succeeded",
            ClientSecret: null));
    }
}
