using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Infrastructure.Jobs;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Demo.CancelUnpaidNow;

/// <summary>
/// Demo handler that fast-forwards the payment past its 96h <c>DueAt</c> so the production
/// <c>NonPaymentCancelJob</c>'s <c>DueAt &lt;= now</c> guard passes, then triggers the recurring
/// sweep. The job calls Stripe to void the authorized intent (or marks Cancelled locally for
/// intentless rows — Option A path), penalizes the buyer's TrustScore (−10), and flips the lot to
/// <see cref="LotStatus.EndedNoSale"/> — all real production paths.
/// </summary>
public class CancelUnpaidNowHandler(
    IApplicationDbContext db,
    IJobScheduler jobScheduler,
    IDateTimeProvider clock)
    : IRequestHandler<CancelUnpaidNowRequest, Result>
{
    public async Task<Result> Handle(CancelUnpaidNowRequest request, CancellationToken ct)
    {
        Payment? payment = await db.Payments.FirstOrDefaultAsync(p => p.LotId == request.LotId, ct);
        if (payment is null)
            return Result.Failure(Error.NotFound("Payment.NotFound", "No payment exists for this lot."));

        if (payment.Status != PaymentStatus.PendingAuthorization)
            return Result.Failure(Error.Conflict(
                "Demo.PaymentNotPending",
                $"Payment is in {payment.Status} — only PendingAuthorization can be cancelled as unpaid."));

        // Push DueAt into the past so the daily sweep treats it as expired.
        payment.DueAt = clock.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync(ct);

        jobScheduler.TriggerNonPaymentCancelSweep();
        return Result.Success();
    }
}
