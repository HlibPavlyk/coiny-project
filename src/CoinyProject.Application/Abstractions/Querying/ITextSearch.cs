namespace CoinyProject.Application.Abstractions.Querying;

public interface ITextSearch
{
    string SearchText { get; }
    Dictionary<string, string> ColumnsSearch { get; }
}