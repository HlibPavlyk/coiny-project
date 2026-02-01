using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using CoinyProject.Application.Abstractions.Querying;

namespace CoinyProject.Application.Common.Querying;

public class PlainTextSearch<T>
{
    private static readonly Dictionary<string, Func<T, string, bool>> Properties = new();

    static PlainTextSearch()
    {
        foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyInfo);

            var getValue = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(property, typeof(object)),
                    parameter)
                .Compile();

            Properties.Add(propertyInfo.Name, (obj, text) =>
            {
                var value = getValue(obj);

                if (value is IConvertible convertible)
                {
                    var textValue = convertible.ToString(CultureInfo.InvariantCulture);

                    return textValue.Contains(text, StringComparison.OrdinalIgnoreCase);
                }

                if (value is IFormattable formattable)
                {
                    var textValue = formattable.ToString(null, CultureInfo.InvariantCulture);

                    return textValue.Contains(text, StringComparison.OrdinalIgnoreCase);
                }

                if (value is ISearchable searchable)
                {
                    return searchable.Search(text);
                }

                return false;
            });
        }
    }

    public static bool Search(T obj, string text, params string[] properties)
    {
        var selected = properties.Length > 0 ? properties : Properties.Keys.ToArray();

        foreach (var propertyName in selected)
        {
            if (Properties.TryGetValue(propertyName, out var search) && search(obj, text))
                return true;
        }

        return false;
    }

    public static bool Search(T obj, IReadOnlyDictionary<string, string> properties)
    {
        foreach (var (propertyName, text) in properties)
        {
            if (!Properties.TryGetValue(propertyName, out var search)) return false;
            if (!search(obj, text)) return false;
        }

        return true;
    }
}