using System.Linq.Expressions;
using Coiny.Application.Common.Requests;

namespace Coiny.Application.Common.Extensions;

/// <summary>IQueryable helpers used by paginated list handlers.</summary>
public static class CollectionExtensions
{
    /// <summary>Skips and takes from an IQueryable using an <see cref="IPagination"/>-shaped request.</summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, IPagination paginate) =>
        source.Skip(paginate.Offset).Take(paginate.Count);

    /// <summary>Conditionally appends a <c>Where</c> clause — keeps fluent chains intact when a filter is optional.</summary>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate) =>
        condition ? source.Where(predicate) : source;
}
