namespace Coiny.Application.Abstractions.Jobs;

/// <summary>
/// Schedules background jobs from Application code without depending on Hangfire directly.
/// Concrete impl is HangfireJobScheduler in Infrastructure.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Schedule the auction-close job to fire at <paramref name="endsAtUtc"/>. Returns the Hangfire job id.</summary>
    string ScheduleAuctionClose(Guid lotId, DateTime endsAtUtc);

    /// <summary>
    /// Reschedule the auction-close job after an anti-snipe extension. Schedules a fresh job at the new
    /// <paramref name="endsAtUtc"/>; any previously-scheduled job for the same lot becomes a no-op because
    /// AuctionCloseJob (task 08) reads the lot's current EndsAt and bails out when it has moved.
    /// </summary>
    string ReScheduleAuctionClose(Guid lotId, DateTime endsAtUtc);
}
