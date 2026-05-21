using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Search;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.ExternalServices.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coiny.Infrastructure.Jobs;

/// <summary>
/// Drains <see cref="SearchOutboxEvent"/> rows into Meilisearch. Reconciles by <em>current</em> lot
/// state rather than interpreting the event type: a lot that is Active or Sold is upserted; anything
/// else (Draft / EndedNoSale / Cancelled / soft-deleted / missing) is removed from the index. Because
/// reconciliation reads the latest state, multiple events for the same lot collapse to one operation —
/// the job dedups by <see cref="SearchOutboxEvent.AggregateId"/> before touching the engine.
///
/// Idempotent and at-least-once: a row is marked processed only after its lot is successfully
/// reconciled; transient failures leave it unprocessed (with AttemptCount/LastError) for the next tick.
/// </summary>
public class SearchIndexFlushJob(
    IApplicationDbContext db,
    ISearchIndex searchIndex,
    IDateTimeProvider clock,
    ILogger<SearchIndexFlushJob> logger)
{
    private const int _batchSize = 100;

    public async Task RunAsync(CancellationToken ct)
    {
        List<SearchOutboxEvent> pending = await db.SearchOutboxEvents
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.Id)
            .Take(_batchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        // Collapse to one reconcile per lot. Each group's events are all marked processed together.
        List<IGrouping<Guid, SearchOutboxEvent>> byLot = pending.GroupBy(e => e.AggregateId).ToList();
        List<Guid> lotIds = byLot.Select(g => g.Key).ToList();

        // The global soft-delete query filter is intentionally honoured: a soft-deleted lot is simply
        // absent from this dictionary, and "absent" reconciles to a delete-from-index below.
        Dictionary<Guid, Lot> lots = await db.Lots
            .AsNoTracking()
            .Include(l => l.Images)
            .Where(l => lotIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        Dictionary<int, Category> categoriesById = await db.Categories
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, ct);

        DateTime now = clock.UtcNow;

        foreach (IGrouping<Guid, SearchOutboxEvent> group in byLot)
        {
            Guid lotId = group.Key;
            try
            {
                bool shouldIndex = lots.TryGetValue(lotId, out Lot? lot)
                    && lot.Status is LotStatus.Active or LotStatus.Sold;

                if (shouldIndex)
                    await searchIndex.UpsertLotAsync(LotSearchDocumentFactory.Create(lot!, categoriesById), ct);
                else
                    await searchIndex.DeleteLotAsync(lotId, ct);

                foreach (SearchOutboxEvent evt in group)
                    evt.ProcessedAt = now;
            }
            catch (Exception ex)
            {
                foreach (SearchOutboxEvent evt in group)
                {
                    evt.AttemptCount++;
                    evt.LastError = ex.Message;
                }
                logger.LogWarning(ex, "SearchIndexFlushJob: failed reconciling lot {LotId}", lotId);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
