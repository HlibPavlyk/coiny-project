using Coiny.Application.Abstractions.ExternalServices.Search;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.SearchLots;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.SuggestLots;

/// <summary>
/// Builds a minimal <see cref="LotSearchQuery"/> (text + public-status filter, no facets, top 8 by
/// relevance) and projects each Meili hit into a slim <see cref="LotSuggestItem"/>. A blank/whitespace
/// query short-circuits to an empty list so the dropdown closes cleanly on backspace.
/// </summary>
public class SuggestLotsHandler(ISearchIndex search)
    : IRequestHandler<SuggestLotsRequest, Result<IReadOnlyList<LotSuggestItem>>>
{
    private const int MaxSuggestions = 8;

    private static readonly IReadOnlyList<string> _publicStatuses =
        [nameof(LotStatus.Active), nameof(LotStatus.Sold)];

    public async Task<Result<IReadOnlyList<LotSuggestItem>>> Handle(
        SuggestLotsRequest request, CancellationToken ct)
    {
        string text = (request.Q ?? string.Empty).Trim();
        if (text.Length == 0)
            return Result.Success<IReadOnlyList<LotSuggestItem>>([]);

        var query = new LotSearchQuery
        {
            Text = text,
            Statuses = _publicStatuses,
            Limit = MaxSuggestions,
            // Empty sort defers to Meilisearch's relevance ranking — the right default for typeahead.
            Sort = [],
        };

        FacetedPage<LotSearchDocument> hits = await search.SearchAsync(query, ct);

        IReadOnlyList<LotSuggestItem> items = hits.Items
            .Select(d => new LotSuggestItem(
                Guid.Parse(d.Id),
                d.Title,
                d.CoverImageUrl,
                d.CategoryPath,
                d.CurrentPriceUahKopiykas))
            .ToList();

        return Result.Success(items);
    }
}
