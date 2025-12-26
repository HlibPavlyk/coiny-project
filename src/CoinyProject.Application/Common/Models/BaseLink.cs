using CoinyProject.Application.Abstractions.Querying;

namespace CoinyProject.Application.Common.Models;

public class BaseLink : IComparable, ISearchable
{
    public Guid Id { get; set; }
    public string Title { get; set; }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        if (obj is BaseLink other)
        {
            return string.Compare(Title, other.Title, StringComparison.OrdinalIgnoreCase);
        }

        throw new ArgumentException("Object is not a BaseLink");
    }

    public bool Search(string text)
    {
        return Title?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;
    }
}