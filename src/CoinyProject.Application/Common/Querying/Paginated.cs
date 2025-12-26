namespace CoinyProject.Application.Common.Querying;

public record Paginated<T>
{
    public int TotalCount { get; set; }
    public T[] Items { get; set; } = [];
}