import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { Icon } from '@/components/Icon';
import { LotImagePlaceholder } from '@/components/LotImagePlaceholder';
import { CountdownTimer } from '@/components/CountdownTimer';
import { lots, type MyLotItem, type LotStatus } from '@/api/lots';
import { ApiError } from '@/api/fetch';
import { formatKopiykasAsUah } from '@/lib/money';
import { useToastStore } from '@/state/useToastStore';

type Tab = 'Draft' | 'Active' | 'Sold' | 'Ended';

const TAB_TO_STATUS: Record<Tab, LotStatus | LotStatus[]> = {
  Draft: 'Draft',
  Active: 'Active',
  Sold: 'Sold',
  Ended: 'EndedNoSale',
};

function statusOf(tab: Tab): LotStatus {
  return TAB_TO_STATUS[tab] as LotStatus;
}

function LotRow({
  lot,
  onCancelled,
}: {
  lot: MyLotItem;
  onCancelled: () => void;
}) {
  const navigate = useNavigate();
  const pushToast = useToastStore((s) => s.push);
  const [busy, setBusy] = useState(false);

  const canEdit = lot.status === 'Draft';
  const canCancel = lot.status === 'Active' && lot.bidCount === 0;

  const cancel = async () => {
    if (!confirm('Cancel this lot? This cannot be undone.')) return;
    setBusy(true);
    try {
      await lots.deleteLot(lot.id);
      pushToast({ kind: 'success', title: 'Lot cancelled' });
      onCancelled();
    } catch (err) {
      pushToast({
        kind: 'danger',
        title: 'Could not cancel',
        description: err instanceof ApiError ? err.detail ?? err.message : undefined,
      });
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="grid items-center gap-4 py-3.5 border-b border-border-soft last:border-b-0"
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
          {lot.title || '(untitled draft)'}
        </Link>
        <div className="text-[12px] text-text-3 mt-0.5 flex items-center gap-3">
          <span>{lot.bidCount} {lot.bidCount === 1 ? 'bid' : 'bids'}</span>
          {lot.status === 'Active' && <CountdownTimer endsAt={lot.endsAt} size="sm" />}
        </div>
      </div>

      <div className="mono text-[14px] font-bold text-right">
        {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
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
          {lot.status}
        </span>
      </div>

      <div className="flex gap-1.5">
        {canEdit && (
          <Link
            to={`/lots/${lot.id}/edit`}
            className="inline-flex items-center gap-1 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px] no-underline"
          >
            <Icon name="edit" size={12} />
            Edit
          </Link>
        )}
        {canCancel && (
          <button
            type="button"
            disabled={busy}
            onClick={cancel}
            className="inline-flex items-center gap-1 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px] disabled:opacity-60"
            style={{ cursor: busy ? 'not-allowed' : 'pointer' }}
          >
            <Icon name="x" size={12} />
            Cancel
          </button>
        )}
        {!canEdit && !canCancel && (
          <button
            type="button"
            onClick={() => navigate(`/lot/${lot.id}`)}
            className="inline-flex items-center gap-1 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px]"
            style={{ cursor: 'pointer' }}
          >
            View
          </button>
        )}
      </div>
    </div>
  );
}

export default function MyLotsPage() {
  const [tab, setTab] = useState<Tab>('Draft');
  const queryClient = useQueryClient();
  const status = statusOf(tab);
  const { data, isLoading } = useQuery({
    queryKey: ['my-lots', status],
    queryFn: () =>
      lots.myLotsSearch({
        offset: 0,
        count: 50,
        sortBy: [{ columnName: 'endsAt', direction: 'Desc' }],
        filters: { status },
      }),
  });

  const refresh = () => queryClient.invalidateQueries({ queryKey: ['my-lots', status] });

  const tabs: { id: Tab; label: string }[] = [
    { id: 'Draft', label: 'Drafts' },
    { id: 'Active', label: 'Active' },
    { id: 'Sold', label: 'Sold' },
    { id: 'Ended', label: 'Ended' },
  ];

  return (
    <div>
      <TopNav />
      <div className="max-w-[1080px] mx-auto px-7 pt-8 pb-16">
        <div className="flex items-baseline justify-between flex-wrap gap-3 mb-5">
          <h1 className="text-[28px] font-bold m-0">My lots</h1>
          <Link
            to="/lots/new"
            className="inline-flex items-center gap-1.5 rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-2.5 text-sm no-underline"
          >
            <Icon name="plus" size={14} color="white" />
            Create lot
          </Link>
        </div>

        <div className="border-b border-border flex gap-1 mb-2">
          {tabs.map((t) => {
            const active = tab === t.id;
            return (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className="px-4 py-2.5 text-[13px] font-medium"
                style={{
                  color: active ? 'var(--color-accent-deep)' : 'var(--color-text-3)',
                  borderBottom: active ? '2px solid var(--color-accent)' : '2px solid transparent',
                  marginBottom: -1,
                  background: 'transparent',
                  cursor: 'pointer',
                }}
              >
                {t.label}
              </button>
            );
          })}
        </div>

        <div className="bg-surface border border-border rounded-lg px-4">
          {isLoading ? (
            <div className="py-10 text-center text-text-3 text-sm">Loading…</div>
          ) : !data || data.items.length === 0 ? (
            <div className="py-12 text-center">
              <p className="text-text-3 text-sm m-0">
                No {tab.toLowerCase()} lots.
              </p>
            </div>
          ) : (
            data.items.map((lot) => <LotRow key={lot.id} lot={lot} onCancelled={refresh} />)
          )}
        </div>
      </div>
      <Footer />
    </div>
  );
}
