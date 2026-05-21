namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Job that cancels an authorized Stripe PaymentIntent to release the hold back to the buyer.
/// Enqueued by <c>NovaPoshtaPollingJob</c> when a shipment reaches Refused or Returned — the lot
/// never reached the buyer, so the escrowed funds must be refunded automatically (THESIS-SCOPE §B).
/// </summary>
public interface ICancelPaymentJob
{
    Task RunAsync(Guid paymentId, CancellationToken ct);
}
