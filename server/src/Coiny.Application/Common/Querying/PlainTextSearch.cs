using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Coiny.Application.Common.Querying;

/// <summary>
/// Reflection-based in-memory filter utility for admin pages and small in-memory collections.
/// NOT used for EF-backed queries — those filter via Meilisearch or LINQ directly.
/// </summary>
public static class PlainTextSearch<T>
{
    private static readonly Dictionary<string, Func<T, string, bool>> _properties;

    static PlainTextSearch()
    {
        _properties = new Dictionary<string, Func<T, string, bool>>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, prop);
            var getValue = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(property, typeof(object)), parameter)
                .Compile();

            _properties[prop.Name] = (obj, text) =>
            {
                var value = getValue(obj);
                return value switch
                {
                    IConvertible c => c.ToString(CultureInfo.InvariantCulture)
                        .Contains(text, StringComparison.OrdinalIgnoreCase),
                    IFormattable f => f.ToString(null, CultureInfo.InvariantCulture)
                        .Contains(text, StringComparison.OrdinalIgnoreCase),
                    _ => false,
                };
            };
        }
    }

    public static bool Search(T obj, string text, params string[] properties)
    {
        var keys = properties.Length > 0 ? properties : [.. _properties.Keys];
        foreach (var key in keys)
        {
            if (_properties.TryGetValue(key, out var match) && match(obj, text))
                return true;
        }
        return false;
    }

    public static bool Search(T obj, IReadOnlyDictionary<string, string> columnFilters)
    {
        foreach (var (col, text) in columnFilters)
        {
            if (!_properties.TryGetValue(col, out var match) || !match(obj, text))
                return false;
        }
        return true;
    }
}
