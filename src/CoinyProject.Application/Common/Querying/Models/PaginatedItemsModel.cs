namespace CoinyProject.Application.Common.Querying.Models;

public record PaginatedItemsModel<T>
{
    public int TotalCount { get; set; }
    public T[] Items { get; set; } = [];
}