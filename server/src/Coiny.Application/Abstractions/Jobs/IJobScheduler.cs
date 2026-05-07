namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Schedules background jobs from Application code without depending on Hangfire directly.
/// Concrete impl is HangfireJobScheduler in Infrastructure.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Schedule the auction-close job to fire at <paramref name="endsAtUtc"/>. Returns the Hangfire job id.</summary>
    string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc);

    /// <summary>Replace an existing scheduled close (e.g. anti-snipe extension). Cancels the previous job id and schedules a new one.</summary>
    string ReScheduleAuctionClose(string previousJobId, Guid lotId, DateTime endsAtUtc);
}
