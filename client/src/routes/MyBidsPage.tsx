import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Icon } from '@/components/Icon';
import { LotImagePlaceholder } from '@/components/LotImagePlaceholder';
import { CountdownTimer } from '@/components/CountdownTimer';
import { useMyBids, type MyBidItemModel } from '@/api/bids';
import { formatKopiykasAsUah } from '@/lib/money';

type Tab = 'Active' | 'Won' | 'Lost';

const PAGE_SIZE = 50;

const TABS: { id: Tab; label: string }[] = [
  { id: 'Active', label: 'Active' },
  { id: 'Won', label: 'Won' },
  { id: 'Lost', label: 'Lost' },
];

function filterFor(tab: Tab, items: MyBidItemModel[]): MyBidItemModel[] {
  if (tab === 'Active') {
    return items.filter((b) => b.lot.status === 'Active');
  }
  if (tab === 'Won') {
    return items.filter((b) => b.lot.status === 'Sold' && b.lot.isCallerLeading);
  }
  // Lost — auction terminated and the caller was not the leader
  return items.filter(
    (b) =>
      (b.lot.status === 'Sold' && !b.lot.isCallerLeading) ||
      b.lot.status === 'EndedNoSale' ||
      b.lot.status === 'Cancelled',
  );
}

function dedupePerLot(items: MyBidItemModel[]): MyBidItemModel[] {
  // The user may have many bids per lot. The list-by-lot view shows only the most recent one
  // (the API already sorts createdAt Desc by default, so first-seen wins).
  const seen = new Set<string>();
  const out: MyBidItemModel[] = [];
  for (const bid of items) {
    if (seen.has(bid.lot.id)) continue;
    seen.add(bid.lot.id);
    out.push(bid);
  }
  return out;
}

function BidRow({ bid }: { bid: MyBidItemModel }) {
  const { lot, amountUahKopiykas } = bid;
  const status = lot.status;

  return (
    <div
      className="grid items-center gap-4 py-3.5 border-b border-border-soft last:border-b-0"
      style={{ gridTemplateColumns: '64px 1fr auto auto auto' }}
    >
      <div className="relative w-16 h-16 rounded-md overflow-hidden bg-bg-soft">
        {lot.coverImageUrl ? (
          <img src={lot.coverImageUrl} alt="" className="w-full h-full object-cover" />
        ) : (
          <LotImagePlaceholder kind="coin" variant={lot.id.charCodeAt(0) % 6} />
        )}
      </div>

      <div className="min-w-0">
        <Link
          to={`/lot/${lot.id}`}
          className="text-[14px] font-medium text-text hover:text-accent-deep no-underline truncate block"
        >
          {lot.title}
        </Link>
        <div className="text-[12px] text-text-3 mt-0.5 flex items-center gap-3 flex-wrap">
          <span>
            Your bid:{' '}
            <span className="mono font-medium text-text-2">
              {formatKopiykasAsUah(amountUahKopiykas, { integer: true })}
            </span>
          </span>
          <span>
            Current:{' '}
            <span className="mono font-medium text-text-2">
              {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
            </span>
          </span>
          {status === 'Active' && <CountdownTimer endsAt={lot.endsAt} size="sm" />}
        </div>
      </div>

      <div>
        {status === 'Active' ? (
          lot.isCallerLeading ? (
            <Pill kind="success">You're leading</Pill>
          ) : (
            <Pill kind="warning">Outbid</Pill>
          )
        ) : status === 'Sold' ? (
          lot.isCallerLeading ? (
            <Pill kind="success">Won</Pill>
          ) : (
            <Pill kind="neutral">Lost</Pill>
          )
        ) : (
          <Pill kind="neutral">{status}</Pill>
        )}
      </div>

      <div>
        <span
          className="inline-block rounded-full font-semibold text-[10px]"
          style={{
            padding: '3px 9px',
            background: 'var(--color-bg-soft)',
            color: 'var(--color-text-2)',
            letterSpacing: '0.04em',
          }}
        >
          {status}
        </span>
      </div>

      <Link
        to={`/lot/${lot.id}`}
        className="inline-flex items-center gap-1 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px] no-underline"
      >
        View
        <Icon name="arrowR" size={12} />
      </Link>
    </div>
  );
}

function Pill({
  kind,
  children,
}: {
  kind: 'success' | 'warning' | 'neutral';
  children: React.ReactNode;
}) {
  const styles: Record<typeof kind, { bg: string; fg: string }> = {
    success: { bg: 'var(--color-success-soft)', fg: '#166534' },
    warning: { bg: 'var(--color-warning-soft)', fg: '#92400E' },
    neutral: { bg: 'var(--color-bg-soft)', fg: 'var(--color-text-2)' },
  };
  const s = styles[kind];
  return (
    <span
      className="inline-flex items-center font-semibold text-[10px] rounded-full uppercase"
      style={{ padding: '3px 9px', background: s.bg, color: s.fg, letterSpacing: '0.04em' }}
    >
      {children}
    </span>
  );
}

export default function MyBidsPage() {
  const [tab, setTab] = useState<Tab>('Active');
  const { data, isLoading } = useMyBids({
    offset: 0,
    count: PAGE_SIZE,
    sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
  });

  const rowsByTab = useMemo(() => {
    if (!data) return null;
    const dedup = dedupePerLot(data.items);
    return {
      Active: filterFor('Active', dedup),
      Won: filterFor('Won', dedup),
      Lost: filterFor('Lost', dedup),
    };
  }, [data]);

  const rows = rowsByTab ? rowsByTab[tab] : [];

  return (
    <>
      <div className="border-b border-border flex gap-1 mb-2">
          {TABS.map((t) => {
            const active = tab === t.id;
            const count = rowsByTab ? rowsByTab[t.id].length : null;
            return (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className="px-4 py-2.5 text-[13px] font-medium"
                style={{
                  color: active ? 'var(--color-accent-deep)' : 'var(--color-text-3)',
                  borderBottom: active
                    ? '2px solid var(--color-accent)'
                    : '2px solid transparent',
                  marginBottom: -1,
                  background: 'transparent',
                  cursor: 'pointer',
                }}
              >
                {t.label}
                {count !== null && (
                  <span className="ml-1.5 mono text-text-3">
                    {count}
                  </span>
                )}
              </button>
            );
          })}
        </div>

        <div className="bg-surface border border-border rounded-lg px-4">
          {isLoading ? (
            <div className="py-10 text-center text-text-3 text-sm">Loading…</div>
          ) : rows.length === 0 ? (
            <div className="py-12 text-center">
              <p className="text-text-3 text-sm m-0">
                {tab === 'Active' && 'No active bids — browse lots to get started.'}
                {tab === 'Won' && 'No wins yet — keep bidding!'}
                {tab === 'Lost' && 'Nothing in this tab.'}
              </p>
            </div>
          ) : (
            rows.map((bid) => <BidRow key={bid.bidId} bid={bid} />)
          )}
        </div>
    </>
  );
}
