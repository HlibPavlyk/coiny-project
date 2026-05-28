using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Abstractions.ExternalServices.Files;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Infrastructure.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Lots.Handlers;

public class UploadLotImageHandler(
    IApplicationDbContext db,
    IFileService files,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<UploadLotImageRequest, Result<LotImageUploadModel>>
{
    private const int MaxImagesPerLot = 5;

    public async Task<Result<LotImageUploadModel>> Handle(UploadLotImageRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure<LotImageUploadModel>(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.LotId, ct);

        if (lot is null)
            return Result.Failure<LotImageUploadModel>(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure<LotImageUploadModel>(Error.Forbidden("Lot.NotOwner", "Only the seller can upload images."));

        if (lot.Status != LotStatus.Draft)
            return Result.Failure<LotImageUploadModel>(Error.Conflict("Lot.NotDraft", "Images can only be added to Draft lots."));

        if (lot.Images.Count >= MaxImagesPerLot)
            return Result.Failure<LotImageUploadModel>(Error.Conflict(
                "Lot.ImageCapReached",
                $"A lot can have at most {MaxImagesPerLot} images."));

        UploadedImage uploaded = await files.UploadImageAsync(
            request.Content, request.ContentType, $"lots/{lot.Id}", ct);

        var image = new LotImage
        {
            Id = Guid.NewGuid(),
            LotId = lot.Id,
            StorageKey = uploaded.StorageKey,
            PublicUrl = uploaded.PublicUrl,
            DisplayOrder = lot.Images.Count,
            Width = uploaded.Width,
            Height = uploaded.Height,
            SizeBytes = uploaded.SizeBytes,
            CreatedAt = clock.UtcNow,
        };

        db.LotImages.Add(image);
        await db.SaveChangesAsync(ct);

        return Result.Success(new LotImageUploadModel(image.Id, image.PublicUrl, image.DisplayOrder));
    }
}
