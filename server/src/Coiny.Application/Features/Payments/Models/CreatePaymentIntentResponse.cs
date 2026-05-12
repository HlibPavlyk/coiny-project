namespace Coiny.Application.Features.Payments.Models;

public record CreatePaymentIntentResponse(
    Guid PaymentId,
    string ClientSecret,
    string PublishableKey,
    long AmountUahKopiykasDisplay,
    long AmountUsdCentsCharged,
    decimal RateUsedUahPerUsd);
