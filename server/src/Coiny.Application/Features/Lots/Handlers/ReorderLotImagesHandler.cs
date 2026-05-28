using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class ReorderLotImagesHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<ReorderLotImagesRequest, Result>
{
    public async Task<Result> Handle(ReorderLotImagesRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.LotId, ct);

        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure(Error.Forbidden("Lot.NotOwner", "Only the seller can reorder images."));

        if (lot.Status != LotStatus.Draft)
            return Result.Failure(Error.Conflict("Lot.NotDraft", "Image order can only be changed for Draft lots."));

        var existingIds = lot.Images.Select(i => i.Id).ToHashSet();
        var incomingIds = request.ImageIds.ToHashSet();

        if (existingIds.Count != incomingIds.Count || !existingIds.SetEquals(incomingIds))
            return Result.Failure(Error.Validation(
                "Lot.ImageReorderMismatch",
                "Reorder ID set must exactly match the lot's current image set."));

        var orderById = request.ImageIds
            .Select((id, idx) => (id, idx))
            .ToDictionary(t => t.id, t => t.idx);

        foreach (LotImage image in lot.Images)
            image.DisplayOrder = orderById[image.Id];

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
