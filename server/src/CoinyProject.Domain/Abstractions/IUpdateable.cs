namespace CoinyProject.Domain.Abstractions;

public interface IUpdateable
{
    DateTime UpdatedAt { get; set; }
}