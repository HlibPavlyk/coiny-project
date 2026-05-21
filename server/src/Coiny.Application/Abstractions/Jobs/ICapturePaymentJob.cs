namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Job that captures a previously-authorized Stripe PaymentIntent the moment NP reports the
/// parcel was handed off to the buyer (Delivered). Enqueued immediately by
/// <c>NovaPoshtaPollingJob</c> — no time buffer (THESIS-SCOPE §B/§F).
/// </summary>
public interface ICapturePaymentJob
{
    Task RunAsync(Guid paymentId, CancellationToken ct);
}
