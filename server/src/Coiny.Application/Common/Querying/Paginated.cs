namespace Coiny.Application.Common.Querying;

/// <summary>Page of items returned by any paginated query handler.</summary>
public record Paginated<T>(int TotalCount, IReadOnlyList<T> Items);
