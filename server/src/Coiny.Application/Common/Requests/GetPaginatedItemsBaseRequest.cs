using System.ComponentModel;
using Coiny.Application.Common.Querying;

namespace Coiny.Application.Common.Requests;

/// <summary>Base for paginated list requests. Handlers validate SortBy column names against a per-endpoint allowlist.</summary>
public record GetPaginatedItemsBaseRequest : GetItemsBaseRequest
{
    public int Offset { get; init; } = 0;

    [DefaultValue(10)]
    public int Count { get; init; } = 10;

    public List<SortByModel>? SortBy { get; init; }
}
