namespace Coiny.Application.Common.Requests;

/// <summary>
/// Abstract base for POST-body search requests that carry both a pagination envelope
/// and feature-specific filter fields. Subclasses add their own filter properties.
/// </summary>
public abstract record GetPaginatedItemsByModelBaseRequest
{
    public GetPaginatedItemsBaseRequest Paginate { get; init; } = new();
}
