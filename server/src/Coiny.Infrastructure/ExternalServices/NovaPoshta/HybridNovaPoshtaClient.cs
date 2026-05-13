using System.Collections.Concurrent;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Shipping;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

/// <summary>
/// Default Nova Poshta client for the thesis build. Read methods (cities, warehouses) are
/// delegated to the real <see cref="NovaPoshtaClient"/> so UX shows real Ukrainian data.
/// Write methods (<see cref="SaveInternetDocumentAsync"/>) return synthetic TTNs and seed an
/// in-memory state machine that <see cref="GetStatusDocumentsAsync"/> reads to simulate
/// shipment progression — no physical parcels are ever created at NP.
///
/// <para>
/// Simulation timeline (2 minutes per state, ~8 minutes total to Delivered):
/// <code>
///   0–2 min  → code 1 (registered)
///   2–4 min  → code 2 (removed from sender city)
///   4–6 min  → code 5 (in transit)
///   6–8 min  → code 7 (arrived at destination warehouse)
///   8+ min   → code 9 (received) → maps to ShipmentStatus.Delivered
/// </code>
/// </para>
///
/// <para>
/// State is process-local: an API restart between TTN creation and the next polling cycle
/// reseeds the state with <c>CreatedAt = utcNow</c> (lifecycle effectively resets). Acceptable
/// for thesis demos; documented so it's not surprising.
/// </para>
/// </summary>
public class HybridNovaPoshtaClient : INovaPoshtaClient
{
    private readonly NovaPoshtaClient _real;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<HybridNovaPoshtaClient> _logger;

    private static readonly ConcurrentDictionary<string, ShipmentSimulationState> _states = new();

    public HybridNovaPoshtaClient(
        NovaPoshtaClient real,
        IDateTimeProvider clock,
        ILogger<HybridNovaPoshtaClient> logger)
    {
        _real = real;
        _clock = clock;
        _logger = logger;
    }

    // Read methods → real API.

    public Task<IReadOnlyList<NpCity>> SearchSettlementsAsync(string query, CancellationToken ct) =>
        _real.SearchSettlementsAsync(query, ct);

    public Task<IReadOnlyList<NpWarehouse>> GetWarehousesAsync(string cityRef, CancellationToken ct) =>
        _real.GetWarehousesAsync(cityRef, ct);

    // Write method → synthetic TTN, seed simulation state.

    public Task<NpInternetDocument> SaveInternetDocumentAsync(NpSaveDocumentRequest request, CancellationToken ct)
    {
        string ttn = "204000" + Random.Shared.NextInt64(100_000, 999_999).ToString();
        string intDocNumber = Guid.NewGuid().ToString("N")[..16];

        _states[ttn] = new ShipmentSimulationState(_clock.UtcNow);
        _logger.LogInformation(
            "HybridNovaPoshta: synthetic TTN {Ttn} created (no real parcel registered at NP)",
            ttn);

        return Task.FromResult(new NpInternetDocument(ttn, intDocNumber));
    }

    // Status method → time-based state machine.

    public Task<IReadOnlyList<NpTrackingStatus>> GetStatusDocumentsAsync(
        IReadOnlyList<string> ttns,
        CancellationToken ct)
    {
        DateTime now = _clock.UtcNow;
        var statuses = new List<NpTrackingStatus>(ttns.Count);

        foreach (string ttn in ttns)
        {
            // Lazy-seed if the TTN isn't in the dictionary (process restart case).
            ShipmentSimulationState state = _states.GetOrAdd(ttn, _ =>
            {
                _logger.LogWarning(
                    "HybridNovaPoshta: no simulation state for {Ttn} — reseeding (process restart?)",
                    ttn);
                return new ShipmentSimulationState(now);
            });

            (int code, string description) = SimulateStatus(now - state.CreatedAt);
            statuses.Add(new NpTrackingStatus(ttn, code, description));
        }

        return Task.FromResult<IReadOnlyList<NpTrackingStatus>>(statuses);
    }

    private static (int code, string description) SimulateStatus(TimeSpan elapsed) =>
        elapsed.TotalMinutes switch
        {
            < 2 => (1, "Shipment registered (simulated)"),
            < 4 => (2, "Removed from sender city (simulated)"),
            < 6 => (5, "In transit (simulated)"),
            < 8 => (7, "Arrived at destination warehouse (simulated)"),
            _   => (9, "Received (simulated)"),
        };
}
