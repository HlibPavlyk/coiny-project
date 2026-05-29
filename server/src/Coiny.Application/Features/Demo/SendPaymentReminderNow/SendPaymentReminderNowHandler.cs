using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Demo.SendPaymentReminderNow;

/// <summary>
/// Demo handler that places the payment into <c>PaymentReminderJob</c>'s firing window
/// (<c>DueAt ∈ [now+47h, now+49h]</c>) by setting <c>DueAt = now + 48h</c> and clearing
/// <c>ReminderSentAt</c>, then triggers the recurring sweep. The production job then queries,
/// finds this row, and emits the same EmailOutboxEvent the natural cadence would.
///
/// <para>Payment is resolved by lot id — after Option A, every Sold lot has exactly one Payment
/// row, so the lookup is 1:1.</para>
/// </summary>
public class SendPaymentReminderNowHandler(
    IApplicationDbContext db,
    IJobScheduler jobScheduler,
    IDateTimeProvider clock)
    : IRequestHandler<SendPaymentReminderNowRequest, Result>
{
    public async Task<Result> Handle(SendPaymentReminderNowRequest request, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.LotId == request.LotId, ct);
        if (payment is null)
            return Result.Failure(Error.NotFound("Payment.NotFound", "No payment exists for this lot."));

        if (payment.Status != PaymentStatus.PendingAuthorization)
            return Result.Failure(Error.Conflict(
                "Demo.PaymentNotPending",
                $"Payment is in {payment.Status} — reminders only fire for PendingAuthorization."));

        // Place into the [now+47h, now+49h] window the job scans for.
        payment.DueAt = clock.UtcNow.AddHours(48);
        payment.ReminderSentAt = null; // clear idempotency mark so the sweep picks it up
        await db.SaveChangesAsync(ct);

        jobScheduler.TriggerPaymentReminderSweep();
        return Result.Success();
    }
}
