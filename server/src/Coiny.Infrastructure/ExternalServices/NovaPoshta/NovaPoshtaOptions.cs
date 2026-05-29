using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

public class NovaPoshtaOptions
{
    public const string Section = "NovaPoshta";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.novaposhta.ua/v2.0/json/";

    // Sender refs intentionally omitted in this thesis build. The demo never creates real
    // TTNs (HybridNovaPoshtaClient generates synthetic numbers), so a platform-level sender
    // would be dead config. Production-time should introduce per-seller sender refs on the
    // User entity (city/warehouse picked during seller onboarding).
}

public class NovaPoshtaOptionsValidator : IValidateOptions<NovaPoshtaOptions>
{
    public ValidateOptionsResult Validate(string? name, NovaPoshtaOptions options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("NovaPoshta:ApiKey is required.");

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            errors.Add("NovaPoshta:BaseUrl must be an absolute URL.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
