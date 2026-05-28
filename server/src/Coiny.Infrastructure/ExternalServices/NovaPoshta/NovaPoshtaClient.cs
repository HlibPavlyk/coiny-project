using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Coiny.Application.Abstractions.ExternalServices.Shipping;
using Microsoft.Extensions.Options;

namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

/// <summary>
/// Real Nova Poshta JSON-RPC client. Every call POSTs to a single endpoint with
/// <c>{ apiKey, modelName, calledMethod, methodProperties }</c>; NP responds with
/// <c>{ success, data, errors, warnings, info }</c>. NP's wire format is mixed —
/// the envelope uses camelCase (<c>success</c>, <c>data</c>, <c>errors</c>) while
/// the inner data objects use PascalCase (<c>Ref</c>, <c>MainDescription</c>, …).
/// We deserialize case-insensitively to handle both layers with one set of options.
/// </summary>
public class NovaPoshtaClient : INovaPoshtaClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        // NP's envelope is lowercase, our DTO is PascalCase — without this every envelope
        // field defaults (success → false) and the client thinks every response failed.
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly NovaPoshtaOptions _options;

    public NovaPoshtaClient(HttpClient http, IOptions<NovaPoshtaOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<NpCity>> SearchSettlementsAsync(string query, CancellationToken ct)
    {
        var props = new
        {
            CityName = query,
            Limit = 20,
        };

        NpEnvelopeResponse<SettlementDto> response = await CallAsync<SettlementDto>(
            modelName: "Address",
            calledMethod: "searchSettlements",
            methodProperties: props,
            ct);

        // NP wraps results in `Addresses` array nested under each row; flatten the first hit.
        return response.Data
            .SelectMany(d => d.Addresses ?? [])
            .Select(a => new NpCity(a.Ref, a.MainDescription, a.Area))
            .ToList();
    }

    public async Task<IReadOnlyList<NpWarehouse>> GetWarehousesAsync(string cityRef, CancellationToken ct)
    {
        var props = new
        {
            SettlementRef = cityRef,
            Limit = "500",
        };

        NpEnvelopeResponse<WarehouseDto> response = await CallAsync<WarehouseDto>(
            modelName: "AddressGeneral",
            calledMethod: "getWarehouses",
            methodProperties: props,
            ct);

        return response.Data
            .Select(w => new NpWarehouse(w.Ref, w.Number, w.Description))
            .ToList();
    }

    public async Task<NpInternetDocument> SaveInternetDocumentAsync(NpSaveDocumentRequest request, CancellationToken ct)
    {
        // Minimal payload that creates a valid TTN at NP. A real production version would
        // also set SeatsAmount, Cost, PayerType, PaymentMethod, CargoType, ServiceType, etc.
        // For the thesis we keep this minimal; HybridNovaPoshtaClient is the default anyway.
        var props = new
        {
            request.SenderName,
            request.SenderPhone,
            CitySender = request.SenderCityRef,
            SenderAddress = request.SenderWarehouseRef,
            request.RecipientName,
            request.RecipientPhone,
            CityRecipient = request.RecipientCityRef,
            RecipientAddress = request.RecipientWarehouseRef,
            Cost = request.DeclaredValueUah,
            request.Description,
        };

        NpEnvelopeResponse<InternetDocumentDto> response = await CallAsync<InternetDocumentDto>(
            modelName: "InternetDocument",
            calledMethod: "save",
            methodProperties: props,
            ct);

        InternetDocumentDto doc = response.Data.First();
        return new NpInternetDocument(doc.IntDocNumber, doc.Ref);
    }

    public async Task<IReadOnlyList<NpTrackingStatus>> GetStatusDocumentsAsync(
        IReadOnlyList<string> ttns,
        CancellationToken ct)
    {
        var props = new
        {
            Documents = ttns.Select(t => new { DocumentNumber = t }).ToArray(),
        };

        NpEnvelopeResponse<TrackingDto> response = await CallAsync<TrackingDto>(
            modelName: "TrackingDocument",
            calledMethod: "getStatusDocuments",
            methodProperties: props,
            ct);

        return response.Data
            .Select(t => new NpTrackingStatus(
                Ttn: t.Number,
                StatusCode: int.TryParse(t.StatusCode, out int code) ? code : 0,
                Description: t.Status ?? string.Empty))
            .ToList();
    }

    private async Task<NpEnvelopeResponse<T>> CallAsync<T>(
        string modelName,
        string calledMethod,
        object methodProperties,
        CancellationToken ct)
    {
        var envelope = new
        {
            apiKey = _options.ApiKey,
            modelName,
            calledMethod,
            methodProperties,
        };

        HttpResponseMessage httpResponse = await _http.PostAsJsonAsync(
            _options.BaseUrl, envelope, JsonOptions, ct);
        httpResponse.EnsureSuccessStatusCode();

        NpEnvelopeResponse<T>? parsed = await httpResponse.Content
            .ReadFromJsonAsync<NpEnvelopeResponse<T>>(JsonOptions, ct);
        if (parsed is null)
            throw new NovaPoshtaException(["Empty response body."]);

        if (!parsed.Success)
            throw new NovaPoshtaException(parsed.Errors ?? ["Unknown NP error."]);

        return parsed;
    }

    // ── DTOs that mirror NP's PascalCase JSON ──────────────────────────────

    private sealed record NpEnvelopeResponse<T>(
        bool Success,
        IReadOnlyList<T> Data,
        IReadOnlyList<string>? Errors,
        IReadOnlyList<string>? Warnings);

    private sealed record SettlementDto(
        IReadOnlyList<SettlementAddressDto>? Addresses);

    private sealed record SettlementAddressDto(
        string Ref,
        string MainDescription,
        string Area);

    private sealed record WarehouseDto(
        string Ref,
        string Number,
        string Description);

    private sealed record InternetDocumentDto(
        string Ref,
        string IntDocNumber);

    private sealed record TrackingDto(
        string Number,
        string StatusCode,
        string? Status);
}
