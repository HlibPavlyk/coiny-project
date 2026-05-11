using Coiny.Application.Abstractions.Jobs;
using Hangfire;

namespace Coiny.Infrastructure.Jobs;

public class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) =>
        client.Schedule<IAuctionCloseJob>(
            j => j.RunAsync(lotId, CancellationToken.None),
            new DateTimeOffset(DateTime.SpecifyKind(endsAtUtc, DateTimeKind.Utc)));

    public string ReScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) =>
        // No previous-job tracking: AuctionCloseJob (task 08) is idempotent — it re-reads the lot's
        // current EndsAt and bails out when the snapshot doesn't match, so stale schedules are harmless.
        ScheduleAuctionClose(lotId, endsAtUtc);
}
