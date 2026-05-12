using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Payments.Models;

public record PaymentDetailModel(
    Guid Id,
    Guid LotId,
    Guid BuyerId,
    Guid SellerId,
    long AmountUahKopiykas,
    long AmountUsdCents,
    decimal RateUsedUahPerUsd,
    string StripePaymentIntentId,
    PaymentStatus Status,
    DateTime DueAt,
    DateTime? AuthorizedAt,
    DateTime? CapturedAt,
    DateTime? CancelledAt,
    Guid? ShipmentId,
    ShipmentStatus? ShipmentStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt);
