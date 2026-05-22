using Coiny.Application.Abstractions.Search;
using Coiny.Application.Common.Json;
using Coiny.Application.Features.Categories;
using Coiny.Domain.Entities;

namespace Coiny.Infrastructure.ExternalServices.Search;

/// <summary>
/// Projects a <see cref="Lot"/> (with its images) into the flat <see cref="LotSearchDocument"/>.
/// Category path is resolved from a pre-loaded category lookup (the category table is tiny and
/// loaded once per flush batch). Country/year/metal come from the lot's JSONB attributes; all are
/// optional and free-text. Dates are converted to Unix seconds for Meilisearch numeric sort/filter.
/// </summary>
public static class LotSearchDocumentFactory
{
    public static LotSearchDocument Create(Lot lot, IReadOnlyDictionary<int, Category> categoriesById)
    {
        // JSONB attributes are untrusted free-text; deserialize defensively via the shared helper.
        var attributes = JsonDefaults.TryDeserialize<LotJsonbAttributes>(lot.Attributes) ?? new LotJsonbAttributes();

        string coverUrl = lot.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => i.PublicUrl)
            .FirstOrDefault() ?? string.Empty;

        return new LotSearchDocument
        {
            Id = lot.Id.ToString(),
            Title = lot.Title,
            Description = lot.Description,
            CategoryPath = string.Join(" > ", CategoryHierarchy.NamesFromRoot(lot.CategoryId, categoriesById)),
            Country = attributes.Country,
            Year = attributes.Year,
            Metal = attributes.Metal,
            Status = lot.Status.ToString(),
            CategoryId = lot.CategoryId,
            Condition = lot.Condition.ToString(),
            CurrentPriceUahKopiykas = lot.CurrentPriceUahKopiykas,
            EndsAtUnix = new DateTimeOffset(DateTime.SpecifyKind(lot.EndsAt, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            CreatedAtUnix = new DateTimeOffset(DateTime.SpecifyKind(lot.CreatedAt, DateTimeKind.Utc)).ToUnixTimeSeconds(),
            CoverImageUrl = coverUrl,
            BidCount = lot.BidCount,
        };
    }
}
