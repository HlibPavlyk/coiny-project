using Coiny.Application.Abstractions.Data;
using Coiny.Domain.Entities;
using Coiny.Infrastructure.ExternalServices.Stripe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Hourly recurring job that re-dispatches Stripe webhook rows whose previous dispatch failed.
/// Bounded at 100 rows per tick and 10 attempts per row — beyond that we leave them for manual review.
/// </summary>
public class RetryFailedWebhookJob(
    IApplicationDbContext db,
    StripeWebhookProcessor processor,
    ILogger<RetryFailedWebhookJob> logger)
{
    private const int _batchSize = 100;
    private const int _maxAttempts = 10;

    public async Task RunAsync(CancellationToken ct)
    {
        List<StripeWebhookEvent> pending = await db.StripeWebhookEvents
            .Where(e => e.ProcessedAt == null && e.AttemptCount < _maxAttempts)
            .OrderBy(e => e.ReceivedAt)
            .Take(_batchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        logger.LogInformation("RetryFailedWebhookJob: replaying {Count} unprocessed webhook events", pending.Count);

        foreach (StripeWebhookEvent row in pending)
        {
            try
            {
                await processor.ReDispatchAsync(row, ct);
            }
            catch (Exception ex)
            {
                // Defensive — ReDispatchAsync catches and persists internally, but never let one bad row
                // kill the whole batch.
                logger.LogError(ex, "RetryFailedWebhookJob: hard failure on event {EventId}", row.Id);
            }
        }
    }
}
