using Coiny.Application.Abstractions.Shipping;

namespace Coiny.Application.Tests.Fakes;

/// <summary>
/// Pure-synthetic Nova Poshta test double. Deterministic responses, no time-based logic,
/// no network. For lifecycle progression in tests, instantiate the real
/// <c>HybridNovaPoshtaClient</c> against a controllable <c>IDateTimeProvider</c> instead.
/// </summary>
public sealed class FakeNovaPoshtaClient : INovaPoshtaClient
{
    public Task<IReadOnlyList<NpCity>> SearchSettlementsAsync(string query, CancellationToken ct)
    {
        IReadOnlyList<NpCity> cities =
        [
            new("city-fake-1", "Fake City 1", "Fake Area"),
            new("city-fake-2", "Fake City 2", "Fake Area"),
            new("city-fake-3", "Fake City 3", "Fake Area"),
        ];
        return Task.FromResult(cities);
    }

    public Task<IReadOnlyList<NpWarehouse>> GetWarehousesAsync(string cityRef, CancellationToken ct)
    {
        IReadOnlyList<NpWarehouse> warehouses =
        [
            new("wh-fake-1", "1", "Fake Warehouse 1"),
            new("wh-fake-2", "2", "Fake Warehouse 2"),
            new("wh-fake-3", "3", "Fake Warehouse 3"),
            new("wh-fake-4", "4", "Fake Warehouse 4"),
            new("wh-fake-5", "5", "Fake Warehouse 5"),
        ];
        return Task.FromResult(warehouses);
    }

    public Task<NpInternetDocument> SaveInternetDocumentAsync(NpSaveDocumentRequest request, CancellationToken ct)
    {
        string ttn = "204000" + Random.Shared.NextInt64(100_000, 999_999).ToString();
        string intDocNumber = Guid.NewGuid().ToString("N")[..16];
        return Task.FromResult(new NpInternetDocument(ttn, intDocNumber));
    }

    public Task<IReadOnlyList<NpTrackingStatus>> GetStatusDocumentsAsync(
        IReadOnlyList<string> ttns,
        CancellationToken ct)
    {
        // Static "InTransit" (NP code 5) for every TTN.
        IReadOnlyList<NpTrackingStatus> statuses = ttns
            .Select(t => new NpTrackingStatus(t, StatusCode: 5, Description: "In transit (fake)"))
            .ToList();
        return Task.FromResult(statuses);
    }
}
