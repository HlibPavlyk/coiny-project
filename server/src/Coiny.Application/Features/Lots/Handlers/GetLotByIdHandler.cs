using System.Text.Json;
using Coiny.Application.Abstractions.Data;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class GetLotByIdHandler(IApplicationDbContext db)
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
            WinningBid: null));
    }

    private async Task<IReadOnlyList<string>> BuildCategoryPathAsync(int leafId, CancellationToken ct)
    {
        List<Category> all = await db.Categories.AsNoTracking().ToListAsync(ct);
        Dictionary<int, Category> byId = all.ToDictionary(c => c.Id);

        List<string> path = [];
        int? cursor = leafId;
        while (cursor is int id && byId.TryGetValue(id, out Category? node))
        {
            path.Add(node.Name);
            cursor = node.ParentId;
        }
        path.Reverse();
        return path;
    }
}
