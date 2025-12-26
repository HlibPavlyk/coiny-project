namespace CoinyProject.Application.Abstractions.Querying;

public interface IPaginate
{
    int Offset { get; }
    int Count { get; }
}