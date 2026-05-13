import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotImagePlaceholder } from '@/components/LotImagePlaceholder';
import { ShipmentTimeline } from '@/components/ShipmentTimeline';
import { payments, type MyPurchaseItemModel, type PaymentStatus } from '@/api/payments';

const PAGE_SIZE = 20;

const PAYMENT_STATUS_LABEL: Record<PaymentStatus, string> = {
  PendingAuthorization: 'Awaiting payment',
  Authorized: 'Paid · escrow',
  Captured: 'Delivered & paid',
  Cancelled: 'Cancelled',
  Failed: 'Failed',
};

const PAYMENT_STATUS_TONE: Record<PaymentStatus, { bg: string; fg: string }> = {
  PendingAuthorization: { bg: 'var(--color-warning-soft)', fg: '#92400E' },
  Authorized: { bg: 'var(--color-info-soft)', fg: '#1E3A8A' },
  Captured: { bg: 'var(--color-success-soft)', fg: '#166534' },
  Cancelled: { bg: 'var(--color-bg-soft)', fg: '#525252' },
  Failed: { bg: 'var(--color-danger-soft)', fg: '#7F1D1D' },
};

function StatusBadge({ status }: { status: PaymentStatus }) {
  const tone = PAYMENT_STATUS_TONE[status];
  return (
    <span
      className="inline-flex items-center font-semibold uppercase rounded-full"
      style={{
        fontSize: 11,
        padding: '3px 8px',
        background: tone.bg,
        color: tone.fg,
        letterSpacing: '0.04em',
      }}
    >
      {PAYMENT_STATUS_LABEL[status]}
    </span>
  );
}

function PurchaseRow({ item }: { item: MyPurchaseItemModel }) {
  const [expanded, setExpanded] = useState(false);
  const amountUah = (item.amountUahKopiykas / 100).toLocaleString('en-US', {
    minimumFractionDigits: 2,
  });

  // Choose the primary CTA depending on lifecycle.
  let action: React.ReactNode = null;
  if (item.paymentStatus === 'PendingAuthorization') {
    action = (
      <Link
        to={`/my-purchases/${item.lot.id}/pay`}
        className="rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3.5 py-1.5 text-xs no-underline"
      >
        Complete checkout
      </Link>
    );
  } else if (
    (item.paymentStatus === 'Authorized' || item.paymentStatus === 'Captured') &&
    item.shipment
  ) {
    action = (
      <button
        type="button"
        onClick={() => setExpanded((v) => !v)}
        className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-3.5 py-1.5 text-xs"
      >
        {expanded ? 'Hide tracking' : 'Track shipment'}
      </button>
    );
  }

  return (
    <article className="border border-border rounded-lg bg-surface p-4">
      <div className="flex items-center gap-4">
        <div className="w-16 h-16 rounded-md overflow-hidden flex-shrink-0 bg-bg-soft">
          {item.lot.coverUrl ? (
            <img
              src={item.lot.coverUrl}
              alt=""
              className="w-full h-full object-cover"
              loading="lazy"
            />
          ) : (
            <LotImagePlaceholder />
          )}
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2.5 flex-wrap">
            <Link
              to={`/lot/${item.lot.id}`}
              className="text-[14.5px] font-semibold no-underline hover:underline"
            >
              {item.lot.title}
            </Link>
            <StatusBadge status={item.paymentStatus} />
          </div>
          <div className="text-[12.5px] text-text-3 mt-1">
            <span className="mono">{amountUah} UAH</span>
            {item.paymentStatus === 'PendingAuthorization' && (
              <>
                {' · '}
                <span>
                  Due <span className="mono">{new Date(item.dueAt).toLocaleString('en-US')}</span>
                </span>
              </>
            )}
          </div>
        </div>
        {action && <div className="flex-shrink-0">{action}</div>}
      </div>

      {expanded && item.shipment && (
        <div className="mt-4 pt-4 border-t border-border-soft">
          <ShipmentTimeline paymentId={item.paymentId} />
        </div>
      )}
    </article>
  );
}

export default function MyPurchasesPage() {
  const [page, setPage] = useState(0);

  const { data, isLoading } = useQuery({
    queryKey: ['my-purchases', page],
    queryFn: () =>
      payments.myPurchasesSearch({
        offset: page * PAGE_SIZE,
        count: PAGE_SIZE,
        sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
      }),
    staleTime: 15_000,
  });

  const items = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const pageCount = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div>
      <TopNav />
      <div className="max-w-[1080px] mx-auto px-7 py-8">
        <div className="flex items-baseline justify-between flex-wrap gap-3 mb-5">
          <h1 className="text-[28px] font-bold m-0">My purchases</h1>
          <p className="text-[13px] text-text-3 m-0">
            <span className="mono font-semibold text-text">{total}</span> total
          </p>
        </div>

        {isLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="bg-bg-soft border border-border rounded-lg h-24" />
            ))}
          </div>
        ) : items.length === 0 ? (
          <div className="bg-surface border border-dashed border-border rounded-lg py-12 text-center">
            <p className="text-text-3">You haven&apos;t won any lots yet.</p>
            <Link
              to="/search"
              className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-2.5 text-sm no-underline mt-4"
            >
              Browse lots
            </Link>
          </div>
        ) : (
          <div className="space-y-3">
            {items.map((item) => (
              <PurchaseRow key={item.paymentId} item={item} />
            ))}
          </div>
        )}

        {pageCount > 1 && (
          <div className="flex justify-center items-center gap-3 mt-7">
            <button
              type="button"
              disabled={page === 0}
              onClick={() => setPage((p) => Math.max(0, p - 1))}
              className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Prev
            </button>
            <span className="text-sm text-text-3">
              Page <span className="mono font-semibold text-text">{page + 1}</span> of{' '}
              <span className="mono">{pageCount}</span>
            </span>
            <button
              type="button"
              disabled={page >= pageCount - 1}
              onClick={() => setPage((p) => Math.min(pageCount - 1, p + 1))}
              className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Next
            </button>
          </div>
        )}
      </div>
      <Footer />
    </div>
  );
}
