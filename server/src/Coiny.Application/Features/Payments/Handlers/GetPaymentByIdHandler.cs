using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.Handlers;

public class GetPaymentByIdHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetPaymentByIdRequest, Result<PaymentDetailModel>>
{
    public async Task<Result<PaymentDetailModel>> Handle(GetPaymentByIdRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<PaymentDetailModel>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        PaymentDetailModel? model = await db.Payments
            .AsNoTracking()
            .Where(p => p.Id == request.PaymentId)
            .Select(p => new PaymentDetailModel(
                p.Id,
                p.LotId,
                p.BuyerId,
                p.SellerId,
                p.AmountUahKopiykas,
                p.AmountUsdCents,
                p.RateUsedUahPerUsd,
                p.StripePaymentIntentId,
                p.Status,
                p.DueAt,
                p.AuthorizedAt,
                p.CapturedAt,
                p.CancelledAt,
                db.Shipments
                    .Where(s => s.PaymentId == p.Id)
                    .Select(s => (Guid?)s.Id)
                    .FirstOrDefault(),
                db.Shipments
                    .Where(s => s.PaymentId == p.Id)
                    .Select(s => (Domain.Enums.ShipmentStatus?)s.Status)
                    .FirstOrDefault(),
                p.CreatedAt,
                p.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        if (model is null)
            return Result.Failure<PaymentDetailModel>(
                Error.NotFound("Payment.NotFound", $"Payment {request.PaymentId} does not exist."));

        if (model.BuyerId != userId && model.SellerId != userId)
            return Result.Failure<PaymentDetailModel>(
                Error.Forbidden("Payment.NotParty", "Only the buyer or seller can view this payment."));

        return Result.Success(model);
    }
}
