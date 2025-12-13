using System.Linq.Expressions;
using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Querying.Models;

namespace CoinyProject.Application.Common.Extensions;

public static class CollectionExtensions
{
    public static IAsyncEnumerable<T> DistinctBy<T, TValue>(this IAsyncEnumerable<T> enumerable, Func<T, TValue> selector)
    {
        return enumerable.Distinct(new DistinctByEqualityComparer<T, TValue>(selector));
    }
    
    public static IAsyncEnumerable<T> DistinctBy<T, TValue1, TValue2>(
        this IAsyncEnumerable<T> enumerable,
        Func<T, TValue1> selector1,
        Func<T, TValue2> selector2)
    {
        return enumerable.Distinct(new DistinctByEqualityComparer<T, (TValue1, TValue2)>(
            item => (selector1(item), selector2(item))
        ));
    }

    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, IPaginate paginate)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (paginate is null) return source;

        return source.Skip(paginate.Offset).Take(paginate.Count);
    }

    public static IEnumerable<T> Search<T>(this IEnumerable<T> collection, ITextSearch search, params Expression<Func<T, object>>[] columns)
    {
        var properties = columns.Select(GetPropertyName).ToArray();

        return Search(collection, search, properties);
    }

    public static IEnumerable<T> Search<T>(this IEnumerable<T> collection, ITextSearch search, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var result = collection;

        if (!string.IsNullOrWhiteSpace(search.SearchText))
        {
            result = result.Where(item => PlainTextSearch<T>.Search(item, search.SearchText, properties));
        }

        if (search.ColumnsSearch is { Count: > 0 })
        {
            result = result.Where(item => PlainTextSearch<T>.Search(item, search.ColumnsSearch));
        }

        return result;
    }

    public static IEnumerable<T> Search<T>(this IEnumerable<T> collection, string searchText, params Expression<Func<T, object>>[] columns)
    {
        var properties = columns.Select(GetPropertyName).ToArray();

        return Search(collection, searchText, properties);
    }

    public static IEnumerable<T> Search<T>(this IEnumerable<T> collection, string searchText, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (string.IsNullOrWhiteSpace(searchText))
            return collection;

        return collection.Where(item => PlainTextSearch<T>.Search(item, searchText, properties));
    }

    public static IAsyncEnumerable<T> Search<T>(this IAsyncEnumerable<T> collection, string searchText, params Expression<Func<T, object>>[] columns)
    {
        var properties = columns.Select(GetPropertyName).ToArray();

        return Search(collection, searchText, properties);
    }
    
    public static IAsyncEnumerable<T> Search<T>(this IAsyncEnumerable<T> collection, ITextSearch search, params Expression<Func<T, object>>[] columns)
    {
        var properties = columns.Select(GetPropertyName).ToArray();

        return Search(collection, search, properties);
    }

    public static IAsyncEnumerable<T> Search<T>(this IAsyncEnumerable<T> collection, string searchText, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (string.IsNullOrWhiteSpace(searchText))
            return collection;

        return collection.Where(item => PlainTextSearch<T>.Search(item, searchText, properties));
    }
    
    public static IAsyncEnumerable<T> Search<T>(this IAsyncEnumerable<T> collection, ITextSearch search, params string[] properties)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var result = collection;

        if (!string.IsNullOrWhiteSpace(search.SearchText))
        {
            result = result.Where(item => PlainTextSearch<T>.Search(item, search.SearchText, properties));
        }

        if (search.ColumnsSearch is { Count: > 0 })
        {
            result = result.Where(item => PlainTextSearch<T>.Search(item, search.ColumnsSearch));
        }

        return result;
    }

    public static IEnumerable<T> SortBy<T>(this IEnumerable<T> collection, SortByModel[] sortBy, string[] displayProperties = null)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (sortBy is not { Length: > 0 })
            return collection;

        IOrderedEnumerable<T> orderedCollection = null;

        foreach (var model in sortBy)
        {
            var columnName = model.ColumnName;
            var direction = model.Direction;

            var propertyInfo = typeof(T).GetProperty(columnName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{columnName}' does not exist on type '{typeof(T).Name}'");
            }

            Func<T, object> keySelector = x =>
            {
                var value = propertyInfo.GetValue(x, null);
                
                if (value is Array array)
                {
                    return string.Join(", ", array.Cast<object>().Select(item => GetDisplayValue(item, displayProperties)));
                }

                if (value is IEnumerable<object> enumerable)
                {
                    return string.Join(", ", enumerable.Select(item => GetDisplayValue(item, displayProperties)));
                }

                return value;
            };

            if (orderedCollection is null)
            {
                orderedCollection = direction == SortDirection.Asc
                    ? collection.OrderBy(keySelector)
                    : collection.OrderByDescending(keySelector);
            }
            else
            {
                orderedCollection = direction == SortDirection.Asc
                    ? orderedCollection.ThenBy(keySelector)
                    : orderedCollection.ThenByDescending(keySelector);
            }
        }

        return orderedCollection;
    }

    private static string GetDisplayValue(object item, string[] displayProperties)
    {
        if (item == null) return "";        

        var itemType = item.GetType();

        foreach (var propName in displayProperties)
        {
            var prop = itemType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(item);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    return value.ToString();
                }
            }
        }

        return item.ToString() ?? "";
    }

    private static string GetPropertyName<T>(this Expression<Func<T, object>> expression)
    {
        var body = expression.Body;

        if (body is UnaryExpression unaryExpression)
        {
            body = unaryExpression.Operand;
        }

        if (body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member expression");
    }

    private class DistinctByEqualityComparer<T, TValue> : IEqualityComparer<T>
    {
        private readonly Func<T, TValue> _selector;
        private readonly IEqualityComparer<TValue> _comparer;

        public DistinctByEqualityComparer(Func<T, TValue> selector) :
            this(selector, EqualityComparer<TValue>.Default) { }

        public DistinctByEqualityComparer(Func<T, TValue> selector, IEqualityComparer<TValue> comparer)
        {
            _selector = selector;
            _comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            return _comparer.Equals(_selector(x), _selector(y));
        }

        public int GetHashCode(T obj)
        {
            return _comparer.GetHashCode(_selector(obj));
        }
    }
}