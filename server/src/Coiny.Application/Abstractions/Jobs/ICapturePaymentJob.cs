namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Job that captures a previously-authorized Stripe PaymentIntent 24 hours after the shipment
/// transitions to Delivered (sprint 3 task 12 implements the body). Scheduled by
/// <c>NovaPoshtaPollingJob</c> the moment NP reports the parcel was handed off to the buyer.
/// </summary>
public interface ICapturePaymentJob
{
    Task RunAsync(Guid paymentId, CancellationToken ct);
}
