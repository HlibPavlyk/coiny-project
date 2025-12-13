namespace CoinyProject.Application.Models;

public interface ITextSearch
{
    string SearchText { get; }
    Dictionary<string, string> ColumnsSearch { get; }
}