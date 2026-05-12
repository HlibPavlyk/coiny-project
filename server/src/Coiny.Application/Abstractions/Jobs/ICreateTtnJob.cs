namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Job that creates a Nova Poshta TTN for the shipment attached to a paid Payment.
/// Enqueued by <c>StripeWebhookProcessor</c> on <c>payment_intent.amount_capturable_updated</c>.
/// Concrete implementation lives in sprint 3 task 10.
/// </summary>
public interface ICreateTtnJob
{
    Task RunAsync(Guid paymentId, CancellationToken ct);
}
