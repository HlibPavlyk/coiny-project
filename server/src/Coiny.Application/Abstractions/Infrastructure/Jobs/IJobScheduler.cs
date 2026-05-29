namespace Coiny.Application.Abstractions.Infrastructure.Jobs;

/// <summary>
/// Schedules background jobs from Application code without depending on Hangfire directly.
/// Concrete impl is HangfireJobScheduler in Infrastructure.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Schedule the auction-close job to fire at <paramref name="endsAtUtc"/>. Returns the Hangfire job id.</summary>
    string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc);

    /// <summary>
    /// Reschedule the auction-close job after an anti-snipe extension. Deletes the
    /// <paramref name="previousJobId"/> (when present) and schedules a fresh job at
    /// <paramref name="endsAtUtc"/>. Returns the new Hangfire job id so the caller can persist it.
    /// </summary>
    string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc);

    /// <summary>Fire-and-forget enqueue of <see cref="ICreateTtnJob"/> for the given paid payment.</summary>
    string EnqueueCreateTtn(Guid paymentId);

    /// <summary>
    /// Fire-and-forget enqueue of <see cref="ICapturePaymentJob"/>. Enqueued by the NP polling job
    /// on the same fire that observes the shipment reaching Delivered — captured immediately, with
    /// no time buffer (THESIS-SCOPE §B/§F: NP-counter handover is the operational truth point).
    /// </summary>
    string EnqueueCapture(Guid paymentId);

    /// <summary>
    /// Fire-and-forget enqueue of <see cref="ICancelPaymentJob"/>. Enqueued by the NP polling job
    /// when a shipment reaches Refused or Returned — cancels the authorized PaymentIntent to release
    /// the hold back to the buyer (THESIS-SCOPE §B: automated refund on non-delivery).
    /// </summary>
    string EnqueueCancelPayment(Guid paymentId);

    /// <summary>
    /// Demo-only: fire-and-forget enqueue of <see cref="IAuctionCloseJob"/> without a delay. Used by
    /// <c>Features/Demo/CloseLotNow</c> to short-circuit the time-based <c>EndsAt</c> trigger while
    /// keeping the production job (with its anti-snipe / status guards) on the critical path.
    /// </summary>
    string EnqueueAuctionCloseNow(Guid lotId);

    /// <summary>
    /// Demo-only: trigger the <c>payment-reminder</c> recurring job ad-hoc, instead of waiting for
    /// the next hourly tick. The job itself still applies its own <c>DueAt ∈ [now+47h, now+49h]</c>
    /// window — callers are expected to mutate <c>Payment.DueAt</c> into that window first.
    /// </summary>
    void TriggerPaymentReminderSweep();

    /// <summary>
    /// Demo-only: trigger the <c>non-payment-cancel</c> recurring job ad-hoc, instead of waiting for
    /// the next daily tick. The job itself still applies <c>DueAt &lt;= now</c> — callers are expected
    /// to mutate <c>Payment.DueAt</c> into the past first.
    /// </summary>
    void TriggerNonPaymentCancelSweep();
}
