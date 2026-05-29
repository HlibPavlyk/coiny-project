namespace Coiny.Application.Abstractions.ExternalServices.Payments;

/// <summary>
/// Stripe Connect onboarding link, mapped from <c>Stripe.AccountLink</c>.
/// Lifetime is short (~5 minutes) — the frontend redirects to it immediately.
/// </summary>
public record StripeAccountLink(string Url, DateTime ExpiresAt);
