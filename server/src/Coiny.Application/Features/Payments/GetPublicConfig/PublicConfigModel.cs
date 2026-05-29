namespace Coiny.Application.Features.Payments.GetPublicConfig;

public record PublicConfigModel(
    string StripePublishableKey,
    string SignalRHubUrl,
    decimal UahPerUsdDisplay,
    bool DemoModeEnabled);
