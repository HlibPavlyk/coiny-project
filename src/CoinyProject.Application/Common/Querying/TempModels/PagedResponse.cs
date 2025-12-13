namespace CoinyProject.Application.Dto.Other;

public class PagedResponse<T>
{
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; }

}