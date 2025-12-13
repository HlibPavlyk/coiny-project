using CoinyProject.Application.Common.Querying.Models;

namespace CoinyProject.Application.Common.Requests;

public record GetItemsBaseRequest : ITextSearch
{
    public string SearchText { get; init; }
    public Dictionary<string, string> ColumnsSearch { get; init; } = new();
    public SortByModel[] SortBy { get; init; } = [];
}