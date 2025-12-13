namespace CoinyProject.Application.Common.Querying.Models;

public interface ITextSearch
{
    string SearchText { get; }
    Dictionary<string, string> ColumnsSearch { get; }
}