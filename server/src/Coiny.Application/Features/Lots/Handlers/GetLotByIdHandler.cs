using System.Text.Json;
using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class GetLotByIdHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetLotByIdRequest, Result<LotDetailModel>>
{
    public async Task<Result<LotDetailModel>> Handle(GetLotByIdRequest request, CancellationToken ct)
    {
        Lot? lot = await db.Lots
            .AsNoTracking()
            .Include(l => l.Category)
            .Include(l => l.Images)
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (lot is null)
            return Result.Failure<LotDetailModel>(Error.NotFound("Lot.NotFound", "Lot not found."));

        IReadOnlyList<string> namePath = await BuildCategoryPathAsync(lot.CategoryId, ct);

        using JsonDocument attrs = JsonDocument.Parse(lot.Attributes);

        var images = lot.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new LotImageModel(i.Id, i.PublicUrl, i.DisplayOrder, i.Width, i.Height))
            .ToList();

        // "Leading" = the top bid (highest amount, earliest CreatedAt tie-break) is the caller's.
        // Only computed for authenticated callers; anonymous viewers always see false.
        bool isCallerLeading = false;
        if (currentUser.IsAuthenticated && currentUser.UserId is { } userId)
        {
            Guid? topBidderId = await db.Bids
                .AsNoTracking()
                .Where(b => b.LotId == lot.Id)
                .OrderByDescending(b => b.AmountUahKopiykas)
                .ThenBy(b => b.CreatedAt)
                .Select(b => (Guid?)b.BidderId)
                .FirstOrDefaultAsync(ct);

            isCallerLeading = topBidderId == userId;
        }

        // For closed lots (Sold), resolve the winning bid so the UI can render winner +
        // the "Complete checkout" CTA. Returns null for any non-Sold status.
        LotWinningBidModel? winningBid = null;
        if (lot.Status == LotStatus.Sold && lot.WinningBidId is { } winningBidId)
        {
            winningBid = await db.Bids
                .AsNoTracking()
                .Where(b => b.Id == winningBidId)
                .Select(b => new LotWinningBidModel(
                    b.Id,
                    b.Bidder!.DisplayName,
                    b.AmountUahKopiykas))
                .FirstOrDefaultAsync(ct);
        }

        // Look up the caller's payment for this lot (if any). Used by BidPanel to choose
        // between "Complete checkout", "Payment in progress", and "View order" CTAs so the
        // buyer cannot re-enter checkout for an already-paid lot. Only computed for the
        // winning bidder — non-winners have no business knowing payment state.
        Guid? callerPaymentId = null;
        PaymentStatus? callerPaymentStatus = null;
        if (isCallerLeading && lot.Status == LotStatus.Sold && currentUser.UserId is { } callerId)
        {
            var paymentRow = await db.Payments
                .AsNoTracking()
                .Where(p => p.LotId == lot.Id && p.BuyerId == callerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.Id, p.Status })
                .FirstOrDefaultAsync(ct);
            if (paymentRow is not null)
            {
                callerPaymentId = paymentRow.Id;
                callerPaymentStatus = paymentRow.Status;
            }
        }

        return Result.Success(new LotDetailModel(
            lot.Id,
            lot.Title,
            lot.Description,
            new LotCategoryBreadcrumbModel(
                lot.CategoryId,
                lot.Category?.Slug ?? string.Empty,
                namePath),
            lot.Condition,
            lot.StartingPriceUahKopiykas,
            lot.CurrentPriceUahKopiykas,
            lot.BidCount,
            lot.ViewCount,
            lot.Status,
            lot.StartsAt == default ? null : lot.StartsAt,
            lot.EndsAt,
            attrs.RootElement.Clone(),
            images,
            new LotSellerModel(
                lot.SellerId,
                lot.Seller?.DisplayName ?? string.Empty,
                lot.Seller?.TrustScore ?? 0),
            WinningBid: winningBid,
            IsCallerLeading: isCallerLeading,
            CallerPaymentId: callerPaymentId,
            CallerPaymentStatus: callerPaymentStatus));
    }

    private async Task<IReadOnlyList<string>> BuildCategoryPathAsync(int leafId, CancellationToken ct)
    {
        Dictionary<int, Category> byId = await db.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, ct);
        return CategoryPathResolver.NamesFromRoot(leafId, byId);
    }
}
