using System.Linq.Expressions;
using System.Reflection;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;

namespace Coiny.Application.Common.Extensions;

/// <summary>
/// EF-translatable multi-column sort with a typed allowlist of property selectors.
/// Caller passes <c>l =&gt; l.PropertyName</c> expressions; <see cref="SortByModel.ColumnName"/>
/// is matched against the property name case-insensitively (JSON camelCase ↔ C# PascalCase).
/// Empty <c>sortBy</c> returns the source unchanged — the caller is responsible for substituting
/// a default when needed.
/// </summary>
public static class QueryableSortExtensions
{
    public static Result<IQueryable<T>> SortBy<T>(
        this IQueryable<T> source,
        IReadOnlyList<SortByModel>? sortBy,
        params Expression<Func<T, object>>[] allowedColumns)
    {
        if (sortBy is not { Count: > 0 })
            return Result.Success(source);

        IOrderedQueryable<T>? ordered = null;
        foreach (SortByModel s in sortBy)
        {
            Expression<Func<T, object>>? match = FindByName(allowedColumns, s.ColumnName);
            if (match is null)
                return Error.Validation("Sort.InvalidColumn", $"Column '{s.ColumnName}' is not sortable.");

            ordered = ApplyOrder(source, ordered, match, s.Direction);
        }

        return Result.Success<IQueryable<T>>(ordered!);
    }

    private static Expression<Func<T, object>>? FindByName<T>(
        Expression<Func<T, object>>[] selectors,
        string name)
    {
        foreach (Expression<Func<T, object>> selector in selectors)
        {
            string propertyName = ExtractMemberName(selector);
            if (propertyName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return selector;
        }
        return null;
    }

    private static string ExtractMemberName<T>(Expression<Func<T, object>> selector)
    {
        Expression body = selector.Body;

        // Value-type properties are auto-wrapped with Convert(x.Prop, object). Unwrap.
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException(
            $"Expected a simple property selector (e.g. x => x.Prop), got: {selector}.",
            nameof(selector));
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(
        IQueryable<T> source,
        IOrderedQueryable<T>? prev,
        Expression<Func<T, object>> selector,
        SortDirection direction)
    {
        // Strip the Convert(x.Prop, object) wrapping so EF orders by the actual column type
        // instead of boxing each row to object — which Postgres can't translate cleanly for indexes.
        Expression body = selector.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            body = unary.Operand;

        LambdaExpression typedLambda = Expression.Lambda(body, selector.Parameters);

        string methodName = (prev is null, direction) switch
        {
            (true,  SortDirection.Asc)  => nameof(Queryable.OrderBy),
            (true,  SortDirection.Desc) => nameof(Queryable.OrderByDescending),
            (false, SortDirection.Asc)  => nameof(Queryable.ThenBy),
            (false, SortDirection.Desc) => nameof(Queryable.ThenByDescending),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown SortDirection."),
        };

        MethodInfo method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), body.Type);

        return (IOrderedQueryable<T>)method.Invoke(null, [prev ?? (object)source, typedLambda])!;
    }
}
