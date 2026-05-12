namespace Coiny.Application.Features.Payments.Models;

public record ConnectStatusResponse(bool StripeOnboarded, IReadOnlyList<string> RequirementsRemaining);
