using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Bids.Models;
using Coiny.Application.Features.Bids.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Bids.Handlers;

public class GetBidHistoryHandler(IApplicationDbContext db)
    : IRequestHandler<GetBidHistoryRequest, Result<Paginated<BidItemModel>>>
{
    public async Task<Result<Paginated<BidItemModel>>> Handle(GetBidHistoryRequest request, CancellationToken ct)
    {
        LotStatusProjection? lotInfo = await db.Lots
            .AsNoTracking()
            .Where(l => l.Id == request.LotId)
            .Select(l => new LotStatusProjection(l.Status))
            .FirstOrDefaultAsync(ct);

        if (lotInfo is null)
            return Result.Failure<Paginated<BidItemModel>>(
                Error.NotFound("Lot.NotFound", "Lot not found."));

        Result<IQueryable<Bid>> sortResult = db.Bids
            .AsNoTracking()
            .Where(b => b.LotId == request.LotId)
            .SortBy(request.Paginate.SortBy, b => b.AmountUahKopiykas, b => b.CreatedAt);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<BidItemModel>>(sortResult.Error);

        int totalCount = await sortResult.Value.CountAsync(ct);

        // Distinct-bidder map for anonymization. Built once per call from a small projection;
        // typical lots have ≤ 30 distinct bidders so the materialized list stays cheap.
        IReadOnlyDictionary<Guid, string> bidderDisplay = lotInfo.Status == LotStatus.Active
            ? await BuildAnonymizedMapAsync(request.LotId, ct)
            : await BuildFullDisplayNameMapAsync(request.LotId, ct);

        List<BidItemRow> pageRows = await sortResult.Value
            .Paginate(request.Paginate)
            .Select(b => new BidItemRow(b.Id, b.BidderId, b.AmountUahKopiykas, b.CreatedAt))
            .ToListAsync(ct);

        var items = pageRows
            .Select(b => new BidItemModel(
                b.Id,
                b.AmountUahKopiykas,
                bidderDisplay.GetValueOrDefault(b.BidderId, "b****?"),
                b.CreatedAt))
            .ToList();

        return Result.Success(new Paginated<BidItemModel>(totalCount, items));
    }

    private async Task<IReadOnlyDictionary<Guid, string>> BuildAnonymizedMapAsync(Guid lotId, CancellationToken ct)
    {
        // Pull minimal (bidderId, createdAt) pairs and group in memory — the per-lot set is small
        // (typically ≤ 30 distinct bidders, hundreds of bids at the high end), and the in-memory
        // approach is portable across providers (Postgres translation of GroupBy → aggregate is
        // brittle in EF Core 10, and the EF in-memory provider can't translate it at all).
        var firstBidOrder = (await db.Bids
                .AsNoTracking()
                .Where(b => b.LotId == lotId)
                .Select(b => new { b.BidderId, b.CreatedAt })
                .ToListAsync(ct))
            .GroupBy(r => r.BidderId)
            .Select(g => new { BidderId = g.Key, FirstBidAt = g.Min(r => r.CreatedAt) })
            .OrderBy(x => x.FirstBidAt)
            .Select(x => x.BidderId)
            .ToList();

        var map = new Dictionary<Guid, string>(firstBidOrder.Count);
        for (int i = 0; i < firstBidOrder.Count; i++)
            map[firstBidOrder[i]] = $"b****{i + 1}";
        return map;
    }

    private async Task<IReadOnlyDictionary<Guid, string>> BuildFullDisplayNameMapAsync(Guid lotId, CancellationToken ct)
    {
        List<Guid> bidderIds = await db.Bids
            .AsNoTracking()
            .Where(b => b.LotId == lotId)
            .Select(b => b.BidderId)
            .Distinct()
            .ToListAsync(ct);

        if (bidderIds.Count == 0)
            return new Dictionary<Guid, string>();

        return await db.Users
            .AsNoTracking()
            .Where(u => bidderIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName })
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, ct);
    }

    private record LotStatusProjection(LotStatus Status);
    private record BidItemRow(Guid Id, Guid BidderId, long AmountUahKopiykas, DateTime CreatedAt);
}
