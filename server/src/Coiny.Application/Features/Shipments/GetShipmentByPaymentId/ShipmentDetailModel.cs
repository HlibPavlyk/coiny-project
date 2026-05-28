using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Shipments.GetShipmentByPaymentId;

public record ShipmentDetailModel(
    Guid Id,
    Guid? PaymentId,
    Guid LotId,
    Guid BuyerId,
    Guid SellerId,
    string? NovaPoshtaTtn,
    string? IntDocNumber,
    string RecipientCityRef,
    string RecipientCityLabel,
    string RecipientWarehouseRef,
    string RecipientWarehouseLabel,
    string RecipientName,
    string RecipientPhone,
    long DeclaredValueUahKopiykas,
    ShipmentStatus Status,
    int LastNpStatusCode,
    DateTime? DeliveredAt,
    DateTime? LastPolledAt,
    IReadOnlyList<ShipmentEventModel> Events,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ShipmentEventModel(
    long Id,
    ShipmentStatus Status,
    int NpStatusCode,
    string? Description,
    DateTime ObservedAt);
