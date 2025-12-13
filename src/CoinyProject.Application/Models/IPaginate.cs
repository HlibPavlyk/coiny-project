namespace CoinyProject.Application.Models;

public interface IPaginate
{
    int Offset { get; }
    int Count { get; }
}