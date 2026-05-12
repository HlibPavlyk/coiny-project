namespace Coiny.Application.Abstractions.Payments;

/// <summary>
/// Stripe Connect account snapshot, mapped from <c>Stripe.Account</c> at the Infrastructure boundary.
/// Keeps Stripe.net types out of Coiny.Application.
/// </summary>
public record StripeAccountInfo(
    string Id,
    bool DetailsSubmitted,
    bool ChargesEnabled,
    IReadOnlyList<string> RequirementsCurrentlyDue);
