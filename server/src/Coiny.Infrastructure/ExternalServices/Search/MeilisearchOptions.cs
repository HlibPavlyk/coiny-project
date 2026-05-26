using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Search;

public class MeilisearchOptions
{
    public const string Section = "Meilisearch";

    /// <summary>Base URL of the Meilisearch instance, e.g. <c>http://localhost:7700</c>.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Master (or admin) API key. Required for index configuration and document writes.</summary>
    public string MasterKey { get; init; } = string.Empty;

    /// <summary>Name of the single index holding searchable lots (Active + Sold).</summary>
    public string IndexName { get; init; } = "lots";

    /// <summary>
    /// Facet fields computed on each search and returned in the distribution. The frontend uses these
    /// value lists to populate the filter pickers. Configurable so the set can be shrunk without code
    /// changes; values are the document's filterable field names.
    /// </summary>
    public string[] Facets { get; init; } = ["metal", "country", "condition", "categoryId"];
}

public class MeilisearchOptionsValidator : IValidateOptions<MeilisearchOptions>
{
    public ValidateOptionsResult Validate(string? name, MeilisearchOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Url))
            return ValidateOptionsResult.Fail("Meilisearch:Url is required.");
        if (string.IsNullOrWhiteSpace(options.MasterKey))
            return ValidateOptionsResult.Fail("Meilisearch:MasterKey is required.");
        if (string.IsNullOrWhiteSpace(options.IndexName))
            return ValidateOptionsResult.Fail("Meilisearch:IndexName is required.");
        return ValidateOptionsResult.Success;
    }
}
