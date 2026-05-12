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
    /// Reschedule the auction-close job after an anti-snipe extension. Deletes the
    /// <paramref name="previousJobId"/> (when present) and schedules a fresh job at
    /// <paramref name="endsAtUtc"/>. Returns the new Hangfire job id so the caller can persist it.
    /// </summary>
    string ReScheduleAuctionClose(string? previousJobId, Guid lotId, DateTime endsAtUtc);

    /// <summary>Fire-and-forget enqueue of <see cref="ICreateTtnJob"/> for the given paid payment.</summary>
    string EnqueueCreateTtn(Guid paymentId);
}
