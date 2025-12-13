namespace CoinyProject.Application.Common.Querying.Models;

public interface IPaginate
{
    int Offset { get; }
    int Count { get; }
}