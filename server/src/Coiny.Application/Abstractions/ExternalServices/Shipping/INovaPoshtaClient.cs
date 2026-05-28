namespace Coiny.Application.Abstractions.ExternalServices.Shipping;

/// <summary>
/// Application-layer seam over Nova Poshta's JSON-RPC API. Three implementations live
/// in Infrastructure: the real <c>NovaPoshtaClient</c>, the pure-synthetic
/// <c>MockNovaPoshtaClient</c> (tests), and the <c>HybridNovaPoshtaClient</c> that uses
/// the real API for read methods but mocks TTN creation + status (default in this thesis
/// build so demos never produce physical parcels).
/// </summary>
public interface INovaPoshtaClient
{
    /// <summary>Autocomplete by partial name. Returns up to ~10 matches.</summary>
    Task<IReadOnlyList<NpCity>> SearchSettlementsAsync(string query, CancellationToken ct);

    /// <summary>List warehouses for a given city <see cref="NpCity.Ref"/>.</summary>
    Task<IReadOnlyList<NpWarehouse>> GetWarehousesAsync(string cityRef, CancellationToken ct);

    /// <summary>Create a waybill. Real implementation creates a physical TTN at NP.</summary>
    Task<NpInternetDocument> SaveInternetDocumentAsync(NpSaveDocumentRequest request, CancellationToken ct);

    /// <summary>Batch lookup of current statuses for a list of TTN numbers.</summary>
    Task<IReadOnlyList<NpTrackingStatus>> GetStatusDocumentsAsync(IReadOnlyList<string> ttns, CancellationToken ct);
}
