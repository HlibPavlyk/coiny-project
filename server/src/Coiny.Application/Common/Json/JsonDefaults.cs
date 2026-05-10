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
}
