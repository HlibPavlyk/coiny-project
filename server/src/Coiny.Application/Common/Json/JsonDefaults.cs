using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coiny.Application.Common.Json;

/// <summary>
/// Project-wide JSON serialization defaults: camelCase property names, omit-null on write,
/// enums as strings. Used by outbox payload contracts and ASP.NET Core JSON (mirrored in Program.cs).
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Defensively deserialize untrusted JSON (e.g. a JSONB column) into <typeparamref name="T"/>
    /// using the project-wide <see cref="Options"/>. Returns <c>default</c> for null/blank input or
    /// malformed JSON — never throws — so one bad row can't break a batch.
    /// </summary>
    public static T? TryDeserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
