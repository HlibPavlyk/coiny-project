using System.Reflection;
using System.Text.Json;
using Coiny.Application.Abstractions.Search;
using Meilisearch;
using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.Search;

/// <summary>
/// Meilisearch-backed <see cref="ISearchIndex"/>. The only place that touches the Meilisearch SDK.
/// The index attribute lists are derived by reflecting the role-attributes declared on
/// <see cref="LotSearchDocument"/> — done here (not in Application) because the wire-name conversion
/// is the SDK's serialization concern: the SDK emits camelCase JSON, so we convert property names
/// with the same <see cref="JsonNamingPolicy.CamelCase"/> policy. Computed once per process.
/// </summary>
public class MeilisearchSearchIndex(MeilisearchClient client, IOptions<MeilisearchOptions> options)
    : ISearchIndex
{
    private static readonly string[] _searchable = WireNamesWith<SearchableAttribute>();
    private static readonly string[] _filterable = WireNamesWith<FilterableAttribute>();
    private static readonly string[] _sortable = WireNamesWith<SortableAttribute>();

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

    // Reflect the role-attributes off LotSearchDocument and convert each property name to its
    // camelCase wire form (matching the SDK's serializer). For searchable, declaration order is
    // preserved — Meilisearch treats it as ranking priority.
    private static string[] WireNamesWith<TAttr>() where TAttr : Attribute =>
        typeof(LotSearchDocument)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<TAttr>() is not null)
            .Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name))
            .ToArray();

    public Task UpsertLotAsync(LotSearchDocument document, CancellationToken ct) =>
        client.Index(_indexName).AddDocumentsAsync([document], primaryKey: "id", ct);

    public Task UpsertLotsAsync(IReadOnlyList<LotSearchDocument> documents, CancellationToken ct) =>
        documents.Count == 0
            ? Task.CompletedTask
            : client.Index(_indexName).AddDocumentsAsync(documents, primaryKey: "id", ct);

    public Task DeleteLotAsync(Guid lotId, CancellationToken ct) =>
        client.Index(_indexName).DeleteOneDocumentAsync(lotId.ToString(), ct);
}
