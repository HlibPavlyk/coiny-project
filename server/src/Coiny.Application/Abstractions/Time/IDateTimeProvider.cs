namespace Coiny.Application.Abstractions.Time;

/// <summary>
/// The only permitted source of the current time in Application code.
/// Always returns UTC. Inject this instead of calling DateTime.UtcNow directly
/// so handlers are unit-testable with a controlled clock (THESIS-SCOPE.md risk #4).
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
