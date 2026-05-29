namespace Coiny.Application.Features.Lots.SuggestLots;

/// <summary>
/// Minimal projection for the typeahead dropdown — id (for navigation), title, cover image, category
/// breadcrumb, current price. No bid count, no facets, no end date. Keep this small: it ships on
/// every keystroke after debounce.
/// </summary>
public record LotSuggestItem(
    Guid Id,
    string Title,
    string CoverImageUrl,
    string CategoryPath,
    long CurrentPriceUahKopiykas);
