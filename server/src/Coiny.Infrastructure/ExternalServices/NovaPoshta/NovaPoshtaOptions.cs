using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

public class NovaPoshtaOptions
{
    public const string Section = "NovaPoshta";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.novaposhta.ua/v2.0/json/";

    /// <summary>
    /// Sender defaults applied to every TTN. Per sprint 3 task 10 path A:
    /// the platform itself is the logistical sender — sellers don't configure per-account refs.
    /// </summary>
    public PlatformSenderOptions PlatformSender { get; init; } = new();
}

public class PlatformSenderOptions
{
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string CityRef { get; init; } = string.Empty;
    public string WarehouseRef { get; init; } = string.Empty;
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

        // Platform sender is consumed by CreateTtnJob (task 10). If it's not set, TTN creation
        // will fail at runtime — surface that misconfiguration at startup instead.
        PlatformSenderOptions s = options.PlatformSender;
        if (string.IsNullOrWhiteSpace(s.Name))
            errors.Add("NovaPoshta:PlatformSender:Name is required.");
        if (string.IsNullOrWhiteSpace(s.Phone))
            errors.Add("NovaPoshta:PlatformSender:Phone is required.");
        if (string.IsNullOrWhiteSpace(s.CityRef))
            errors.Add("NovaPoshta:PlatformSender:CityRef is required.");
        if (string.IsNullOrWhiteSpace(s.WarehouseRef))
            errors.Add("NovaPoshta:PlatformSender:WarehouseRef is required.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
