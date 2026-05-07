using System.ComponentModel;
using Coiny.Application.Common.Querying;

namespace Coiny.Application.Common.Requests;

/// <summary>
/// Wire-ready paginated request shape: page offset, count, sort criteria.
/// Use as <c>[FromBody]</c> on the controller, or as a base for flat request records that add
/// custom fields directly (e.g. <c>GetMyLotsRequest : PageRequest</c>).
/// For server-composed requests (route param + body) use <see cref="PageQuery"/> instead.
/// </summary>
public record PageRequest : IPagination
{
    public int Offset { get; init; } = 0;

    [DefaultValue(10)]
    public int Count { get; init; } = 10;

    public SortByModel[]? SortBy { get; init; }
}
