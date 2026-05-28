using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Payments.Models;
using Coiny.Application.Features.Payments.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Payments.Handlers;

public class GetMyPurchasesHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyPurchasesRequest, Result<Paginated<MyPurchaseItemModel>>>
{
    public async Task<Result<Paginated<MyPurchaseItemModel>>> Handle(
        GetMyPurchasesRequest request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<Paginated<MyPurchaseItemModel>>(
                Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        // 1) Real payments — caller is the buyer on a Payment row.
        List<MyPurchaseItemModel> paymentRows = await db.Payments
            .AsNoTracking()
            .Where(p => p.BuyerId == userId)
            .Select(p => new MyPurchaseItemModel(
                p.Id,
                p.Status,
                p.AmountUahKopiykas,
                p.DueAt,
                p.CreatedAt,
                new MyPurchaseLotInfo(
                    p.LotId,
                    db.Lots.Where(l => l.Id == p.LotId).Select(l => l.Title).FirstOrDefault() ?? string.Empty,
                    db.LotImages
                        .Where(i => i.LotId == p.LotId)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.PublicUrl)
                        .FirstOrDefault() ?? string.Empty),
                db.Shipments
                    .Where(s => s.PaymentId == p.Id)
                    .Select(s => new MyPurchaseShipmentInfo(s.Id, s.Status, s.NovaPoshtaTtn))
                    .FirstOrDefault()))
            .ToListAsync(ct);

        // 2) Pre-payment wins — lots the caller has won (Sold, WinningBid.BidderId == caller)
        // but no Payment row exists yet (buyer hasn't started checkout). These are synthetic
        // entries with PaymentId = Lot.Id so the existing /my-purchases/{lotId}/pay route works.
        // DueAt is reconstructed as EndsAt + 96h (the would-be Payment.DueAt window).
        List<MyPurchaseItemModel> pendingWins = await db.Lots
            .AsNoTracking()
            .Where(l => l.Status == LotStatus.Sold
                     && l.WinningBidId != null
                     && db.Bids.Any(b => b.Id == l.WinningBidId && b.BidderId == userId)
                     && !db.Payments.Any(p => p.LotId == l.Id))
            .Select(l => new MyPurchaseItemModel(
                l.Id, // synthetic — frontend keys by it and routes via lot.id anyway
                PaymentStatus.PendingAuthorization,
                db.Bids.Where(b => b.Id == l.WinningBidId).Select(b => b.AmountUahKopiykas).FirstOrDefault(),
                l.EndsAt.AddHours(96),
                l.UpdatedAt,
                new MyPurchaseLotInfo(
                    l.Id,
                    l.Title,
                    db.LotImages
                        .Where(i => i.LotId == l.Id)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => i.PublicUrl)
                        .FirstOrDefault() ?? string.Empty),
                null))
            .ToListAsync(ct);

        IEnumerable<MyPurchaseItemModel> merged = paymentRows.Concat(pendingWins);

        // Sort allowlist is the same as the underlying Payment columns; we apply it in memory
        // since the merged set spans two source tables.
        Result<IEnumerable<MyPurchaseItemModel>> sorted = ApplySort(merged, request.SortBy);
        if (sorted.IsFailure)
            return Result.Failure<Paginated<MyPurchaseItemModel>>(sorted.Error);

        List<MyPurchaseItemModel> ordered = sorted.Value.ToList();
        int totalCount = ordered.Count;

        List<MyPurchaseItemModel> page = ordered
            .Skip(request.Offset)
            .Take(request.Count)
            .ToList();

        return Result.Success(new Paginated<MyPurchaseItemModel>(totalCount, page));
    }

    private static Result<IEnumerable<MyPurchaseItemModel>> ApplySort(
        IEnumerable<MyPurchaseItemModel> source,
        IReadOnlyList<SortByModel>? sortBy)
    {
        // Default: newest first.
        if (sortBy is null || sortBy.Count == 0)
            return Result.Success<IEnumerable<MyPurchaseItemModel>>(source.OrderByDescending(x => x.CreatedAt));

        IOrderedEnumerable<MyPurchaseItemModel>? ordered = null;
        foreach (SortByModel s in sortBy)
        {
            Func<MyPurchaseItemModel, IComparable> selector = s.ColumnName switch
            {
                "createdAt" => x => x.CreatedAt,
                "dueAt" => x => x.DueAt,
                _ => null!,
            };
            if (selector is null)
                return Result.Failure<IEnumerable<MyPurchaseItemModel>>(
                    Error.Validation("Sort.InvalidColumn", $"Cannot sort by '{s.ColumnName}'."));

            bool desc = s.Direction == SortDirection.Desc;
            ordered = ordered is null
                ? (desc ? source.OrderByDescending(selector) : source.OrderBy(selector))
                : (desc ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector));
        }

        return Result.Success<IEnumerable<MyPurchaseItemModel>>(ordered!);
    }
}
