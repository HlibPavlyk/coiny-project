using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Models;

namespace Coiny.Application.Abstractions.ExternalServices.Search;

/// <summary>
/// Application-layer seam over the search engine. The concrete implementation (Meilisearch) lives in
/// Coiny.Infrastructure and is the only place that touches the Meilisearch SDK. Keeps Application free
/// of any search-engine dependency — mirrors <c>IStripeClient</c> / <c>INovaPoshtaClient</c>.
/// </summary>
public interface ISearchIndex
{
    /// <summary>
    /// Idempotently create the index and apply its searchable / filterable / sortable attribute
    /// configuration. Safe to call on every startup.
    /// </summary>
    Task EnsureIndexAsync(CancellationToken ct);

    /// <summary>Add or replace a lot document. Used for Active and Sold lots.</summary>
    Task UpsertLotAsync(LotSearchDocument document, CancellationToken ct);

    /// <summary>Add or replace many lot documents in one request (used by the bootstrap backfill and flush job).</summary>
    Task UpsertLotsAsync(IReadOnlyList<LotSearchDocument> documents, CancellationToken ct);

    /// <summary>Remove a lot from the index (EndedNoSale, Cancelled, admin-deleted, or hard-deleted).</summary>
    Task DeleteLotAsync(Guid lotId, CancellationToken ct);

    /// <summary>
    /// Run a full-text + filtered search over the indexed (Active/Sold) lots. Returns a page of matched
    /// documents plus the facet distribution for the adapter's configured facets.
    /// </summary>
    Task<FacetedPage<LotSearchDocument>> SearchAsync(LotSearchQuery query, CancellationToken ct);
}
