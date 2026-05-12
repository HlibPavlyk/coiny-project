namespace Coiny.Application.Features.Payments.Models;

public record PublicConfigModel(
    string StripePublishableKey,
    string SignalRHubUrl,
    decimal UahPerUsdDisplay);
