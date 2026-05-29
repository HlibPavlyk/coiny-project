namespace Coiny.Domain.Enums;

/// <summary>
/// Lifecycle of a buyer's payment intent. Mirrors Stripe's PaymentIntent state machine to the
/// granularity Coiny cares about.
/// </summary>
public enum PaymentStatus
{
    PendingAuthorization,
    Authorized,
    Captured,
    Cancelled,
    Failed,
}
