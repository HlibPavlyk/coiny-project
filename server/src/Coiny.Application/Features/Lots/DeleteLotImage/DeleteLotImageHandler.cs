using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.ExternalServices.Files;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Application.Features.Lots.DeleteLotImage;

public class DeleteLotImageHandler(
    IApplicationDbContext db,
    IFileService files,
    ICurrentUserService currentUser,
    ILogger<DeleteLotImageHandler> logger)
    : IRequestHandler<DeleteLotImageRequest, Result>
{
    public async Task<Result> Handle(DeleteLotImageRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.LotId, ct);

        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure(Error.Forbidden("Lot.NotOwner", "Only the seller can delete images."));

        if (lot.Status != LotStatus.Draft)
            return Result.Failure(Error.Conflict("Lot.NotDraft", "Images can only be removed from Draft lots."));

        LotImage? image = lot.Images.FirstOrDefault(i => i.Id == request.ImageId);
        if (image is null)
            return Result.Failure(Error.NotFound("Lot.ImageNotFound", "Image not found on this lot."));

        try
        {
            await files.DeleteAsync(image.StorageKey, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "DeleteLotImageHandler: failed to remove R2 object {StorageKey}; row will still be deleted.",
                image.StorageKey);
        }

        db.LotImages.Remove(image);

        // Re-pack DisplayOrder to keep it sequential (frontend assumes 0..N-1)
        int order = 0;
        foreach (LotImage remaining in lot.Images.Where(i => i.Id != image.Id).OrderBy(i => i.DisplayOrder))
        {
            remaining.DisplayOrder = order++;
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
