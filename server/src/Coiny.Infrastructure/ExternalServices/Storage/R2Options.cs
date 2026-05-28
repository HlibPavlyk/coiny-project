using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Storage;

public class R2Options
{
    public const string Section = "R2";

    public string AccountId { get; init; } = string.Empty;
    public string AccessKeyId { get; init; } = string.Empty;
    public string SecretAccessKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;

    /// <summary>Public-facing URL base for serving uploaded images. e.g. https://pub-xxx.r2.dev or https://images.coiny.tld.</summary>
    public string PublicUrlBase { get; init; } = string.Empty;
}

public class R2OptionsValidator : IValidateOptions<R2Options>
{
    public ValidateOptionsResult Validate(string? name, R2Options options)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(options.AccountId))
            errors.Add("R2:AccountId is required.");
        if (string.IsNullOrWhiteSpace(options.AccessKeyId))
            errors.Add("R2:AccessKeyId is required.");
        if (string.IsNullOrWhiteSpace(options.SecretAccessKey))
            errors.Add("R2:SecretAccessKey is required.");
        if (string.IsNullOrWhiteSpace(options.BucketName))
            errors.Add("R2:BucketName is required.");
        if (string.IsNullOrWhiteSpace(options.PublicUrlBase))
            errors.Add("R2:PublicUrlBase is required.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
