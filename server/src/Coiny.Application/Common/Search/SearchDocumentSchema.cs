using System.Reflection;
using System.Text.Json;

namespace Coiny.Application.Common.Search;

/// <summary>
/// Derives field lists from the role-attributes declared on a search-document type, so the index
/// configuration and the sortable allowlist are reflected from the document shape and can never drift.
/// Property names are converted with <see cref="JsonNamingPolicy.CamelCase"/> to match the search SDK's
/// serializer (camelCase JSON). Reflection is cheap and callers cache the results in statics.
/// </summary>
public static class SearchDocumentSchema
{
    /// <summary>
    /// CamelCase wire names of <typeparamref name="TDocument"/> properties carrying
    /// <typeparamref name="TAttr"/>, in declaration order (Meilisearch treats searchable order as ranking).
    /// </summary>
    public static string[] FieldsWith<TDocument, TAttr>() where TAttr : Attribute =>
        typeof(TDocument)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<TAttr>() is not null)
            .Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name))
            .ToArray();
}
