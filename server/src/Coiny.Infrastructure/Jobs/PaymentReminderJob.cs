using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Features.Auctions;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Coiny.Application.Features.Auctions.Events;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Hourly Hangfire job that emits a 48h won-pay reminder for every <see cref="PaymentStatus.PendingAuthorization"/>
/// payment whose <see cref="Payment.DueAt"/> is 47-49 hours away. The hourly cadence + 2-hour window
/// guarantees one fire per payment. <see cref="Payment.ReminderSentAt"/> blocks duplicates if a payment
/// happens to slip through the window twice (unlikely but cheap to guard).
///
/// Inserts <c>EmailOutboxEvent</c> with type <see cref="AuctionWonPayReminderPayload.EventType"/>;
/// the actual send happens in task 14 via <c>EmailOutboxFlushJob</c> (which reuses the won-pay
/// template with a "REMINDER:" banner per <c>THESIS-SCOPE.md</c> §1 §15).
/// </summary>
[AutomaticRetry(Attempts = 3)]
public class PaymentReminderJob(
    IApplicationDbContext db,
    IDateTimeProvider clock,
    ILogger<PaymentReminderJob> logger)
{
    private static readonly TimeSpan _windowStart = TimeSpan.FromHours(47);
    private static readonly TimeSpan _windowEnd = TimeSpan.FromHours(49);
    private const int _batchSize = 200;

    public async Task RunAsync(CancellationToken ct)
    {
        DateTime now = clock.UtcNow;
        DateTime lowerBound = now.Add(_windowStart);
        DateTime upperBound = now.Add(_windowEnd);

        List<Payment> dueForReminder = await db.Payments
            .Where(p => p.Status == PaymentStatus.PendingAuthorization
                     && p.ReminderSentAt == null
                     && p.DueAt >= lowerBound
                     && p.DueAt <= upperBound)
            .OrderBy(p => p.DueAt)
            .Take(_batchSize)
            .ToListAsync(ct);

        if (dueForReminder.Count == 0)
            return;

        logger.LogInformation("PaymentReminderJob: emitting {Count} won-pay reminders", dueForReminder.Count);

        foreach (Payment payment in dueForReminder)
        {
            Lot? lot = await db.Lots.FirstOrDefaultAsync(l => l.Id == payment.LotId, ct);
            string lotTitle = lot?.Title ?? "(unknown lot)";

            db.EmailOutboxEvents.Add(new EmailOutboxEvent
            {
                AggregateType = nameof(Payment),
                AggregateId = payment.Id,
                EventType = AuctionWonPayReminderPayload.EventType,
                Payload = new AuctionWonPayReminderPayload(
                    PaymentId: payment.Id,
                    LotId: payment.LotId,
                    LotTitle: lotTitle,
                    AmountUahKopiykas: payment.AmountUahKopiykas,
                    DueAt: payment.DueAt).Serialize(),
                CreatedAt = now,
                AttemptCount = 0,
            });

            payment.ReminderSentAt = now;
        }

        await db.SaveChangesAsync(ct);
    }
}
