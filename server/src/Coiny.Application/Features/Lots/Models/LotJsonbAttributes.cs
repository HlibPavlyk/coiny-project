namespace Coiny.Application.Features.Lots.Models;

/// <summary>
/// The subset of a lot's free-text JSONB attributes that get flattened into the search document
/// (so faceted search never touches JSONB in EF — THESIS-SCOPE risk #6). All optional. Bound from
/// the lowercase JSONB keys by the project-wide camelCase policy (<c>JsonDefaults.Options</c>);
/// unknown keys (series, denomination, …) are ignored.
/// </summary>
public sealed class LotJsonbAttributes
{
    public string? Country { get; init; }
    public string? Year { get; init; }
    public string? Metal { get; init; }
}
