using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Payments.GetMyPurchases;

/// <summary>
/// One row in the buyer's purchases dashboard. Eagerly joins the lot's title + cover image
/// and the linked shipment's id+status so the list page renders without N follow-up calls.
/// The detail panel still calls <c>GET /shipments/{paymentId}</c> for the full event timeline.
/// </summary>
public record MyPurchaseItemModel(
    Guid PaymentId,
    PaymentStatus PaymentStatus,
    long AmountUahKopiykas,
    DateTime DueAt,
    DateTime CreatedAt,
    MyPurchaseLotInfo Lot,
    MyPurchaseShipmentInfo? Shipment);

public record MyPurchaseLotInfo(
    Guid Id,
    string Title,
    string CoverUrl);

public record MyPurchaseShipmentInfo(
    Guid Id,
    ShipmentStatus Status,
    string? NovaPoshtaTtn);
