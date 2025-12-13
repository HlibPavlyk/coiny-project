namespace CoinyProject.Application.Common.Querying.Models;

public interface ISearchable
{
    bool Search(string text);
}