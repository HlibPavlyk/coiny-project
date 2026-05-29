using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Resend;

public class ResendOptions
{
    public const string Section = "Resend";

    public string ApiKey { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
}

public class ResendOptionsValidator : IValidateOptions<ResendOptions>
{
    public ValidateOptionsResult Validate(string? name, ResendOptions options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("Resend:ApiKey is required.");

        if (string.IsNullOrWhiteSpace(options.FromAddress))
            errors.Add("Resend:FromAddress is required.");
        else if (!options.FromAddress.Contains('@'))
            errors.Add("Resend:FromAddress must be a valid email address.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
