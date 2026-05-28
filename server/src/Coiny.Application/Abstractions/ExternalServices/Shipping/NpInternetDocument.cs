namespace Coiny.Application.Abstractions.ExternalServices.Shipping;

/// <summary>
/// Result of <c>InternetDocument/save</c>. <see cref="Ttn"/> is the public waybill number
/// ("204000XXXXXX"); <see cref="IntDocNumber"/> is NP's internal id, used in subsequent calls.
/// </summary>
public record NpInternetDocument(string Ttn, string IntDocNumber);
