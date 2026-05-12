using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Stripe;

public class StripeOptions
{
    public const string Section = "Stripe";

    public string SecretKey { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public string ConnectClientId { get; init; } = string.Empty;
    public decimal UahPerUsd { get; init; }
    public string ReturnUrl { get; init; } = string.Empty;
    public string RefreshUrl { get; init; } = string.Empty;
}

public class StripeOptionsValidator : IValidateOptions<StripeOptions>
{
    public ValidateOptionsResult Validate(string? name, StripeOptions options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.SecretKey))
            errors.Add("Stripe:SecretKey is required.");

        if (string.IsNullOrWhiteSpace(options.PublishableKey))
            errors.Add("Stripe:PublishableKey is required.");

        if (string.IsNullOrWhiteSpace(options.WebhookSecret))
            errors.Add("Stripe:WebhookSecret is required.");

        if (string.IsNullOrWhiteSpace(options.ConnectClientId))
            errors.Add("Stripe:ConnectClientId is required.");

        if (options.UahPerUsd <= 0m)
            errors.Add("Stripe:UahPerUsd must be greater than zero.");

        if (!IsAbsoluteUrl(options.ReturnUrl))
            errors.Add("Stripe:ReturnUrl must be an absolute URL.");

        if (!IsAbsoluteUrl(options.RefreshUrl))
            errors.Add("Stripe:RefreshUrl must be an absolute URL.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsAbsoluteUrl(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && Uri.TryCreate(value, UriKind.Absolute, out _);
}
