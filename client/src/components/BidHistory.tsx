import { useState } from 'react';
import { useBidHistory } from '@/api/bids';
import { formatKopiykasAsUah } from '@/lib/money';
import { formatLocal } from '@/lib/datetime';

const INITIAL_PAGE_SIZE = 10;
const STEP = 10;

interface BidHistoryProps {
  lotId: string;
}

/**
 * Paginated bid history for a lot. Anonymization is decided server-side
 * (<c>b****&lt;n&gt;</c> while Active, real DisplayName after close); the client
 * just renders whatever <c>bidderDisplay</c> is supplied.
 *
 * Live updates flow through <c>useAuctionLot</c> in <c>BidPanel</c> — its
 * <c>onBidPlaced</c> / <c>onAuctionClosed</c> handlers invalidate the
 * <c>['bid-history', lotId, ...]</c> cache prefix, which triggers this
 * <c>useBidHistory</c> query to refetch.
 */
export function BidHistory({ lotId }: BidHistoryProps) {
  const [pageSize, setPageSize] = useState(INITIAL_PAGE_SIZE);

  const { data, isLoading, isError } = useBidHistory(lotId, {
    offset: 0,
    count: pageSize,
    sortBy: [{ columnName: 'amountUahKopiykas', direction: 'Desc' }],
  });

  if (isLoading) {
    return (
      <div className="text-[13px] text-text-3 py-6 text-center">Loading bid history…</div>
    );
  }

  if (isError || !data) {
    return (
      <div className="text-[13px] text-danger py-6 text-center">
        Could not load bid history.
      </div>
    );
  }

  if (data.items.length === 0) {
    return (
      <div className="text-[13px] text-text-3 py-8 text-center border border-dashed border-border-soft rounded-md">
        No bids yet — be the first.
      </div>
    );
  }

  const hasMore = data.items.length < data.totalCount;

  return (
    <div>
      <div className="overflow-x-auto">
        <table className="w-full text-[13px] border-collapse">
          <thead>
            <tr>
              <th
                scope="col"
                className="text-left font-semibold text-text-3 text-[11px] uppercase tracking-wider px-3 py-2 border-b border-border"
              >
                Bidder
              </th>
              <th
                scope="col"
                className="text-right font-semibold text-text-3 text-[11px] uppercase tracking-wider px-3 py-2 border-b border-border"
              >
                Amount
              </th>
              <th
                scope="col"
                className="text-right font-semibold text-text-3 text-[11px] uppercase tracking-wider px-3 py-2 border-b border-border"
              >
                When
              </th>
            </tr>
          </thead>
          <tbody>
            {data.items.map((bid, idx) => {
              const isLeader = idx === 0;
              return (
                <tr key={bid.id} className="border-b border-border-soft last:border-b-0">
                  <td className="px-3 py-2.5">
                    <div className="flex items-center gap-2">
                      <span className="mono text-text">{bid.bidderDisplay}</span>
                      {isLeader && (
                        <span
                          className="inline-flex items-center rounded-full font-semibold uppercase"
                          style={{
                            padding: '2px 7px',
                            fontSize: 9,
                            letterSpacing: '0.06em',
                            background: 'var(--color-accent-tint)',
                            color: 'var(--color-accent-deep)',
                          }}
                        >
                          Leader
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="px-3 py-2.5 text-right mono font-semibold">
                    {formatKopiykasAsUah(bid.amountUahKopiykas, { integer: true })}
                  </td>
                  <td
                    className="px-3 py-2.5 text-right text-text-3"
                    title={formatLocal(bid.createdAt)}
                  >
                    {formatRelative(bid.createdAt)}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between mt-3 text-[12px] text-text-3">
        <span>
          Showing {data.items.length} of {data.totalCount}
        </span>
        {hasMore && (
          <button
            type="button"
            onClick={() => setPageSize((s) => s + STEP)}
            className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px]"
          >
            Load more
          </button>
        )}
      </div>
    </div>
  );
}

/** Minimal relative-time formatter. Recomputes per render — fine for ≤20 rows. */
function formatRelative(iso: string): string {
  const ms = Date.now() - new Date(iso).getTime();
  if (ms < 0) return 'just now';
  const sec = Math.floor(ms / 1000);
  if (sec < 60) return `${sec}s ago`;
  const min = Math.floor(sec / 60);
  if (min < 60) return `${min} min ago`;
  const h = Math.floor(min / 60);
  if (h < 24) return `${h}h ago`;
  const d = Math.floor(h / 24);
  if (d < 30) return `${d}d ago`;
  return formatLocal(iso);
}

