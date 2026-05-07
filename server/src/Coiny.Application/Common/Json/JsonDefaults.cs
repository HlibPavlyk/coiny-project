using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coiny.Application.Common.Json;

/// <summary>
/// Project-wide JSON serialization defaults: camelCase property names, omit-null on write.
/// Used by outbox payload contracts, ASP.NET Core JSON (task 14), and Npgsql JSONB serialization.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
