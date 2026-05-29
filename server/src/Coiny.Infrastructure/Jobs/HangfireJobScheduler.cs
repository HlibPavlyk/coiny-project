using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Hangfire;

namespace Coiny.Infrastructure.Jobs;

public class HangfireJobScheduler(IBackgroundJobClient client, IRecurringJobManager recurring) : IJobScheduler
{
    public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) =>
        client.Schedule<IAuctionCloseJob>(
            j => j.RunAsync(lotId, CancellationToken.None),
            new DateTimeOffset(DateTime.SpecifyKind(endsAtUtc, DateTimeKind.Utc)));

    public string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc)
    {
        // Hangfire has no "reschedule"; the canonical pattern is delete + schedule fresh.
        // The brief window between delete and schedule is safe: AuctionCloseJob is idempotent —
        // if it ever runs against a non-Active or mismatched-EndsAt lot, it no-ops.
        if (!string.IsNullOrEmpty(previousJobId))
            client.Delete(previousJobId);

        return ScheduleAuctionClose(lotId, endsAtUtc);
    }

    public string EnqueueCreateTtn(Guid paymentId) =>
        client.Enqueue<ICreateTtnJob>(j => j.RunAsync(paymentId, CancellationToken.None));

    public string EnqueueCapture(Guid paymentId) =>
        client.Enqueue<ICapturePaymentJob>(j => j.RunAsync(paymentId, CancellationToken.None));

    public string EnqueueCancelPayment(Guid paymentId) =>
        client.Enqueue<ICancelPaymentJob>(j => j.RunAsync(paymentId, CancellationToken.None));

    public string EnqueueAuctionCloseNow(Guid lotId) =>
        client.Enqueue<IAuctionCloseJob>(j => j.RunAsync(lotId, CancellationToken.None));

    public void TriggerPaymentReminderSweep() => recurring.Trigger("payment-reminder");

    public void TriggerNonPaymentCancelSweep() => recurring.Trigger("non-payment-cancel");
}
