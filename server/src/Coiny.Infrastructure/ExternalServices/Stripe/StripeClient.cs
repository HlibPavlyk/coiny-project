using Coiny.Application.Abstractions.Payments;
using Microsoft.Extensions.Options;
using Stripe;
using IAppStripeClient = Coiny.Application.Abstractions.Payments.IStripeClient;

namespace Coiny.Infrastructure.ExternalServices.Stripe;

public class StripeClient : IAppStripeClient
{
    private readonly StripeOptions _options;

    public StripeClient(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
    }

    public decimal UahPerUsdRate => _options.UahPerUsd;

    public string PublishableKey => _options.PublishableKey;

    public async Task<StripeAccountInfo> CreateConnectAccountAsync(string email, CancellationToken ct)
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

        Account account = await service.CreateAsync(createOptions, cancellationToken: ct);
        return MapAccount(account);
    }

    public async Task<StripeAccountLink> CreateOnboardingLinkAsync(string accountId, CancellationToken ct)
    {
        var service = new AccountLinkService();
        var createOptions = new AccountLinkCreateOptions
        {
            Account = accountId,
            Type = "account_onboarding",
            RefreshUrl = _options.RefreshUrl,
            ReturnUrl = _options.ReturnUrl,
        };

        AccountLink link = await service.CreateAsync(createOptions, cancellationToken: ct);
        return new StripeAccountLink(link.Url, DateTime.SpecifyKind(link.ExpiresAt, DateTimeKind.Utc));
    }

    public async Task<StripeAccountInfo> GetAccountAsync(string accountId, CancellationToken ct)
    {
        var service = new AccountService();
        Account account = await service.GetAsync(accountId, cancellationToken: ct);
        return MapAccount(account);
    }

    public async Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        long usdCents,
        string sellerAccountId,
        IDictionary<string, string> metadata,
        string idempotencyKey,
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

        var requestOptions = new RequestOptions { IdempotencyKey = idempotencyKey };
        PaymentIntent intent = await service.CreateAsync(createOptions, requestOptions, ct);
        return new StripePaymentIntentResult(intent.Id, intent.Status, intent.ClientSecret);
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

    private static StripeAccountInfo MapAccount(Account account) => new(
        Id: account.Id,
        DetailsSubmitted: account.DetailsSubmitted,
        ChargesEnabled: account.ChargesEnabled,
        RequirementsCurrentlyDue: account.Requirements?.CurrentlyDue?.ToList() ?? []);
}
