namespace Coiny.Application.Features.Payments.CreatePaymentIntent;

public record CreatePaymentIntentResponse(
    Guid PaymentId,
    string ClientSecret,
    string PublishableKey,
    long AmountUahKopiykasDisplay,
    long AmountUsdCentsCharged,
    decimal RateUsedUahPerUsd);
