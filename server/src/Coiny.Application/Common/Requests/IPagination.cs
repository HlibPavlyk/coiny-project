namespace Coiny.Application.Common.Requests;

/// <summary>
/// Pagination contract: page offset and item count. Implemented by request DTOs that drive
/// paginated listings; consumed by the <c>Paginate</c> extension over <see cref="IQueryable{T}"/>.
/// </summary>
public interface IPagination
{
    int Offset { get; }
    int Count { get; }
}
