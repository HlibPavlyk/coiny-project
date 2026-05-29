namespace Coiny.Application.Common.Requests;

/// <summary>
/// Server-composed paginated query envelope. Wraps a <see cref="PageRequest"/>
/// and is meant to be inherited by feature-specific MediatR queries that combine
/// route parameters with the body — never serialized over the wire directly.
/// </summary>
public abstract record PageQuery
{
    public PageRequest Paginate { get; init; } = new();
}
