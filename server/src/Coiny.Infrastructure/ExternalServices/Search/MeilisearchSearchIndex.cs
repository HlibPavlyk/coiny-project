using Coiny.Application.Abstractions.ExternalServices.Search;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Search;
using Coiny.Application.Features.Lots.Models;
using Meilisearch;
using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Search;

/// <summary>
/// Meilisearch-backed <see cref="ISearchIndex"/>. The only place that touches the Meilisearch SDK.
/// The index attribute lists are derived from the role-attributes declared on
/// <see cref="LotSearchDocument"/> via <see cref="SearchDocumentSchema"/>, so they can never drift
/// from the document shape. Computed once per process.
/// </summary>
public class MeilisearchSearchIndex(MeilisearchClient client, IOptions<MeilisearchOptions> options)
    : ISearchIndex
{
    private static readonly string[] _searchable = SearchDocumentSchema.FieldsWith<LotSearchDocument, SearchableAttribute>();
    private static readonly string[] _filterable = SearchDocumentSchema.FieldsWith<LotSearchDocument, FilterableAttribute>();
    private static readonly string[] _sortable = SearchDocumentSchema.FieldsWith<LotSearchDocument, SortableAttribute>();

    private readonly string _indexName = options.Value.IndexName;

    public async Task EnsureIndexAsync(CancellationToken ct)
    {
        // Create the index (primary key "id") if it doesn't exist yet. GetIndexAsync throws
        // MeilisearchApiError ("index_not_found") when absent — that's our signal to create.
        try
        {
            await client.GetIndexAsync(_indexName, ct);
        }
        catch (MeilisearchApiError)
        {
            TaskInfo created = await client.CreateIndexAsync(_indexName, "id", ct);
            await client.WaitForTaskAsync(created.TaskUid, cancellationToken: ct);
        }

        // Attribute config is idempotent — re-applying the same settings on every startup is a no-op
        // once Meilisearch has them.
        Meilisearch.Index index = client.Index(_indexName);
        await index.UpdateSearchableAttributesAsync(_searchable, ct);
        await index.UpdateFilterableAttributesAsync(_filterable, ct);
        await index.UpdateSortableAttributesAsync(_sortable, ct);
    }

    public Task UpsertLotAsync(LotSearchDocument document, CancellationToken ct) =>
        client.Index(_indexName).AddDocumentsAsync([document], primaryKey: "id", ct);

    public Task UpsertLotsAsync(IReadOnlyList<LotSearchDocument> documents, CancellationToken ct) =>
        documents.Count == 0
            ? Task.CompletedTask
            : client.Index(_indexName).AddDocumentsAsync(documents, primaryKey: "id", ct);

    public Task DeleteLotAsync(Guid lotId, CancellationToken ct) =>
        client.Index(_indexName).DeleteOneDocumentAsync(lotId.ToString(), ct);

    public async Task<FacetedPage<LotSearchDocument>> SearchAsync(LotSearchQuery query, CancellationToken ct)
    {
        var searchQuery = new SearchQuery
        {
            Filter = MeilisearchLotFilter.Build(query),
            Sort = query.Sort.Count > 0
                ? query.Sort.Select(s => $"{s.ColumnName}:{(s.Direction == SortDirection.Desc ? "desc" : "asc")}").ToList()
                : null,
            Offset = query.Offset,
            Limit = query.Limit,
            Facets = options.Value.Facets,
        };

        ISearchable<LotSearchDocument> raw = await client.Index(_indexName)
            .SearchAsync<LotSearchDocument>(query.Text, searchQuery, ct);

        var result = (SearchResult<LotSearchDocument>)raw;

        IReadOnlyDictionary<string, IReadOnlyList<FacetValue>> facets =
            result.FacetDistribution?.ToDictionary(
                field => field.Key, IReadOnlyList<FacetValue> (field) => field.Value
                    .Select(v => new FacetValue(v.Key, v.Value))
                    .ToList())
            ?? new Dictionary<string, IReadOnlyList<FacetValue>>();

        return new FacetedPage<LotSearchDocument>(result.EstimatedTotalHits, result.Hits.ToList(), facets);
    }
}
