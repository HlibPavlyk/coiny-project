using Coiny.Application.Abstractions.Jobs;
using Hangfire;

namespace Coiny.Infrastructure.Jobs;

public class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc) =>
        client.Schedule<IAuctionCloseJob>(
            j => j.RunAsync(lotId, CancellationToken.None),
            new DateTimeOffset(DateTime.SpecifyKind(endsAtUtc, DateTimeKind.Utc)));

    public string ReScheduleAuctionClose(string previousJobId, Guid lotId, DateTime endsAtUtc)
    {
        if (!string.IsNullOrEmpty(previousJobId))
            client.Delete(previousJobId);

        return ScheduleAuctionClose(lotId, endsAtUtc);
    }
}
