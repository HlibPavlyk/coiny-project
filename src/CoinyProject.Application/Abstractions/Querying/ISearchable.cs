namespace CoinyProject.Application.Abstractions.Querying;

public interface ISearchable
{
    bool Search(string text);
}