namespace Coiny.Application.Abstractions.Payments;

/// <summary>
/// Stripe PaymentIntent snapshot, mapped from <c>Stripe.PaymentIntent</c>.
/// <c>ClientSecret</c> is the value the frontend uses with Stripe.js to confirm the card.
/// </summary>
public record StripePaymentIntentResult(string Id, string Status, string? ClientSecret);
