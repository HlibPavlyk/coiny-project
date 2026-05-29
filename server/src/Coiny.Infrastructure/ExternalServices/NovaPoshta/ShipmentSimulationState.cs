namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

/// <summary>
/// In-memory record tracking when a synthetic TTN was created. Used by
/// <see cref="HybridNovaPoshtaClient"/> to drive the time-based state machine
/// that turns code <c>1 → 2 → 5 → 7 → 9</c> over ~8 minutes.
/// </summary>
internal sealed record ShipmentSimulationState(DateTime CreatedAt);
