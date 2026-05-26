using System.Globalization;
using Coiny.Application.Features.Lots.Models;

namespace Coiny.Infrastructure.ExternalServices.Search;

/// <summary>
/// Builds a Meilisearch filter expression from a <see cref="LotSearchQuery"/>. Lives in Infrastructure
/// because the filter syntax is a Meilisearch concern; kept pure and public so it can be unit-tested
/// without a live engine. Clauses are AND-combined; <c>IN [...]</c> is OR-within-a-field.
/// </summary>
public static class MeilisearchLotFilter
{
    public static string? Build(LotSearchQuery query)
    {
        var clauses = new List<string>();

        if (query.CategoryIds.Count > 0)
            clauses.Add($"categoryId IN [{string.Join(", ", query.CategoryIds.Select(id => id.ToString(CultureInfo.InvariantCulture)))}]");

        if (query.Statuses.Count > 0)
            clauses.Add($"status IN [{Quote(query.Statuses)}]");

        if (query.Conditions.Count > 0)
            clauses.Add($"condition IN [{Quote(query.Conditions)}]");

        if (query.Metals.Count > 0)
            clauses.Add($"metal IN [{Quote(query.Metals)}]");

        if (query.Countries.Count > 0)
            clauses.Add($"country IN [{Quote(query.Countries)}]");

        if (query.PriceFrom is { } from)
            clauses.Add($"currentPriceUahKopiykas >= {from.ToString(CultureInfo.InvariantCulture)}");

        if (query.PriceTo is { } to)
            clauses.Add($"currentPriceUahKopiykas <= {to.ToString(CultureInfo.InvariantCulture)}");

        if (query.EndingBeforeUnix is { } endUnix)
            clauses.Add($"endsAt <= {endUnix.ToString(CultureInfo.InvariantCulture)}");

        return clauses.Count == 0 ? null : string.Join(" AND ", clauses);
    }

    // Meilisearch string literals use double quotes; escape any embedded quote or backslash.
    private static string Quote(IEnumerable<string> values) =>
        string.Join(", ", values.Select(v => $"\"{v.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""));
}
