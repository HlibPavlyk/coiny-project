using CoinyProject.Application.Abstractions.Querying;
using CoinyProject.Application.Common.Querying;

namespace CoinyProject.Application.Common.Requests;

public record GetItemsBaseRequest : ITextSearch
{
    public string SearchText { get; init; }
    public Dictionary<string, string> ColumnsSearch { get; init; } = new();
    public SortByModel[] SortBy { get; init; } = [];
}