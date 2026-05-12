using Microsoft.Extensions.Options;
using Stripe;

namespace Coiny.Infrastructure.ExternalServices.Stripe;

public class StripeClient
{
    private readonly StripeOptions _options;

    public StripeClient(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public Task<Account> CreateConnectAccountAsync(string email, CancellationToken ct)
    {
        var service = new AccountService();
        var createOptions = new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
            },
        };

        return service.CreateAsync(createOptions, cancellationToken: ct);
    }

    public Task<AccountLink> CreateOnboardingLinkAsync(string accountId, CancellationToken ct)
    {
        var service = new AccountLinkService();
        var createOptions = new AccountLinkCreateOptions
        {
            Account = accountId,
            Type = "account_onboarding",
            RefreshUrl = _options.RefreshUrl,
            ReturnUrl = _options.ReturnUrl,
        };

        return service.CreateAsync(createOptions, cancellationToken: ct);
    }

    public Task<Account> GetAccountAsync(string accountId, CancellationToken ct)
    {
        var service = new AccountService();
        return service.GetAsync(accountId, cancellationToken: ct);
    }

    public Task<PaymentIntent> CreatePaymentIntentAsync(
        long usdCents,
        string sellerAccountId,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        var service = new PaymentIntentService();
        var createOptions = new PaymentIntentCreateOptions
        {
            Amount = usdCents,
            Currency = "usd",
            CaptureMethod = "manual",
            PaymentMethodTypes = ["card"],
            TransferData = new PaymentIntentTransferDataOptions
            {
                Destination = sellerAccountId,
            },
            Metadata = new Dictionary<string, string>(metadata),
        };

        return service.CreateAsync(createOptions, cancellationToken: ct);
    }

    public Task<PaymentIntent> CapturePaymentIntentAsync(string paymentIntentId, CancellationToken ct)
    {
        var service = new PaymentIntentService();
        return service.CaptureAsync(paymentIntentId, cancellationToken: ct);
    }

    public Task<PaymentIntent> CancelPaymentIntentAsync(
        string paymentIntentId,
        string? reason,
        CancellationToken ct)
    {
        var service = new PaymentIntentService();
        var cancelOptions = new PaymentIntentCancelOptions
        {
            CancellationReason = reason,
        };

        return service.CancelAsync(paymentIntentId, cancelOptions, cancellationToken: ct);
    }

    public Event ConstructWebhookEvent(string rawPayload, string signatureHeader) =>
        EventUtility.ConstructEvent(rawPayload, signatureHeader, _options.WebhookSecret);
}
