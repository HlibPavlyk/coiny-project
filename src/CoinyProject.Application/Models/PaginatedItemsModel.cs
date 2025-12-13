namespace CoinyProject.Application.Models;

public record PaginatedItemsModel<T>
{
    public int TotalCount { get; set; }
    public T[] Items { get; set; } = [];
}