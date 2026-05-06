using System.Linq.Expressions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;

namespace Coiny.Application.Common.Extensions;

/// <summary>In-memory and IQueryable helpers for pagination and sorting. Not for EF text-search — use Meilisearch.</summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Applies sort criteria to an in-memory collection using a caller-supplied allowlist.
    /// Returns <see cref="Error.Validation"/> if any column name is absent from the allowlist.
    /// </summary>
    public static Result<IEnumerable<T>> SortBy<T>(
        this IEnumerable<T> source,
        IReadOnlyList<SortByModel>? sortBy,
        IReadOnlyDictionary<string, Func<T, object?>> allowlist)
    {
        if (sortBy is not { Count: > 0 })
            return Result.Success(source);

        IOrderedEnumerable<T>? ordered = null;

        foreach (SortByModel model in sortBy)
        {
            if (!allowlist.TryGetValue(model.ColumnName, out Func<T, object?>? keySelector))
                return Error.Validation("Sort.InvalidColumn", $"Column '{model.ColumnName}' is not sortable.");

            ordered = ordered is null
                ? model.Direction == SortDirection.Asc
                    ? source.OrderBy(keySelector)
                    : source.OrderByDescending(keySelector)
                : model.Direction == SortDirection.Asc
                    ? ordered.ThenBy(keySelector)
                    : ordered.ThenByDescending(keySelector);
        }

        return Result.Success<IEnumerable<T>>(ordered!);
    }

    /// <summary>Skips and takes from an in-memory sequence.</summary>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int offset, int count) =>
        source.Skip(offset).Take(count);

    /// <summary>Skips and takes from an IQueryable (EF-compatible).</summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int offset, int count) =>
        source.Skip(offset).Take(count);

    /// <summary>
    /// Filters an in-memory collection by free text across specified properties.
    /// For admin pages only — EF-backed endpoints must not call this.
    /// </summary>
    public static IEnumerable<T> SearchBy<T>(
        this IEnumerable<T> source,
        string? searchText,
        params Expression<Func<T, object>>[] columns)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return source;

        string[] props = columns.Select(GetPropertyName).ToArray();
        return source.Where(item => PlainTextSearch<T>.Search(item, searchText, props));
    }

    /// <summary>
    /// Filters an in-memory collection by per-column values.
    /// For admin pages only — EF-backed endpoints must not call this.
    /// </summary>
    public static IEnumerable<T> SearchBy<T>(
        this IEnumerable<T> source,
        IReadOnlyDictionary<string, string>? columnFilters)
    {
        if (columnFilters is not { Count: > 0 })
            return source;

        return source.Where(item => PlainTextSearch<T>.Search(item, columnFilters));
    }

    private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        Expression body = expression.Body is UnaryExpression unary ? unary.Operand : expression.Body;

        return body is MemberExpression member
            ? member.Member.Name
            : throw new ArgumentException("Expression must be a member access.");
    }
}
