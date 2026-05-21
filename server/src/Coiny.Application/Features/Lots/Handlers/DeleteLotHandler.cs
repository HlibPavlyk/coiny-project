using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Files;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Application.Features.Lots.Handlers;

public class DeleteLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IFileService files,
    IDateTimeProvider clock,
    ILogger<DeleteLotHandler> logger)
    : IRequestHandler<DeleteLotRequest, Result>
{
    public async Task<Result> Handle(DeleteLotRequest request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
            return Result.Failure(Error.Unauthorized("Auth.NotAuthenticated", "Authentication required."));

        Lot? lot = await db.Lots
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (lot is null)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        if (lot.SellerId != userId)
            return Result.Failure(Error.Forbidden("Lot.NotOwner", "Only the seller can delete this lot."));

        return lot.Status switch
        {
            LotStatus.Draft => await HardDeleteAsync(lot, ct),
            LotStatus.Active when lot.BidCount == 0 => await CancelAsync(lot, ct),
            LotStatus.Active => Result.Failure(Error.Conflict(
                "Lot.HasBids",
                "Cannot cancel an auction with bids; contact admin.")),
            _ => Result.Failure(Error.Conflict(
                "Lot.NotDeletable",
                $"Lots in status '{lot.Status}' cannot be deleted.")),
        };
    }

    private async Task<Result> HardDeleteAsync(Lot lot, CancellationToken ct)
    {
        foreach (LotImage image in lot.Images)
        {
            try
            {
                await files.DeleteAsync(image.StorageKey, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "DeleteLotHandler: failed to delete R2 object {StorageKey} for lot {LotId}; row will still be removed.",
                    image.StorageKey, lot.Id);
            }
        }

        db.Lots.Remove(lot);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> CancelAsync(Lot lot, CancellationToken ct)
    {
        DateTime now = clock.UtcNow;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        lot.Status = LotStatus.Cancelled;
        lot.UpdatedAt = now;

        LotEndedPayload payload = new(lot.Id, lot.Status);
        db.SearchOutboxEvents.Add(new SearchOutboxEvent
        {
            AggregateType = "Lot",
            AggregateId = lot.Id,
            EventType = LotEndedPayload.EventType,
            Payload = payload.Serialize(),
            CreatedAt = now,
        });

        await db.Categories
            .Where(c => c.Id == lot.CategoryId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LotCountActive, c => Math.Max(c.LotCountActive - 1, 0)), ct);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result.Success();
    }
}
