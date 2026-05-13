namespace Coiny.Application.Abstractions.Shipping;

/// <summary>
/// Inputs for <c>InternetDocument/save</c>. Refs reference cities/warehouses obtained
/// from <see cref="NpCity.Ref"/> / <see cref="NpWarehouse.Ref"/>.
/// </summary>
public record NpSaveDocumentRequest(
    string SenderName,
    string SenderPhone,
    string SenderCityRef,
    string SenderWarehouseRef,
    string RecipientName,
    string RecipientPhone,
    string RecipientCityRef,
    string RecipientWarehouseRef,
    decimal DeclaredValueUah,
    string Description);
