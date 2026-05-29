namespace Coiny.Application.Features.Payments.GetConnectStatus;

public record ConnectStatusResponse(bool StripeOnboarded, IReadOnlyList<string> RequirementsRemaining);
