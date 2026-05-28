import { useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { Breadcrumb, type BreadcrumbPart } from '@/components/Breadcrumb';
import { ImageGallery } from '@/components/ImageGallery';
import { AttributesTable } from '@/components/AttributesTable';
import { MarkdownView } from '@/components/MarkdownView';
import { BidPanel } from '@/components/BidPanel';
import { BidHistory } from '@/components/BidHistory';
import { ConditionBadge } from '@/components/ConditionBadge';
import { Icon } from '@/components/Icon';
import { ReportLotModal } from '@/components/ReportLotModal';
import { useLot } from '@/api/lots';
import { useCategoryTree, findCategoryPath } from '@/api/categories';
import { formatLocal } from '@/lib/datetime';
import { ApiError } from '@/api/fetch';

const STATUS_STYLES: Record<string, { bg: string; fg: string; dot: string; label: string }> = {
  Active: { bg: '#0E2A17', fg: '#9CE0B3', dot: '#34D399', label: 'Active' },
  Sold: { bg: '#1A2C52', fg: '#9CB7F0', dot: '#60A5FA', label: 'Sold' },
  EndedNoSale: { bg: '#3A2A2A', fg: '#C09898', dot: '#A78080', label: 'Ended · no sale' },
  Cancelled: { bg: '#3A2A2A', fg: '#C09898', dot: '#A78080', label: 'Cancelled' },
  Draft: { bg: '#F0EDE5', fg: '#5C5040', dot: '#A89878', label: 'Draft' },
};

function StatusPill({ status }: { status: string }) {
  const s = STATUS_STYLES[status] ?? STATUS_STYLES.Draft;
  return (
    <span
      className="inline-flex items-center gap-1.5 rounded-full font-semibold"
      style={{
        padding: '4px 10px',
        background: s.bg,
        color: s.fg,
        fontSize: 11,
        letterSpacing: '0.04em',
      }}
    >
      <span
        className="inline-block rounded-full"
        style={{ width: 7, height: 7, background: s.dot }}
      />
      {s.label}
    </span>
  );
}

function NotFound() {
  return (
    <div>
      <TopNav />
      <div className="max-w-[720px] mx-auto px-7 py-24 text-center">
        <h1 className="text-4xl font-bold m-0 text-text">Lot not found</h1>
        <p className="text-text-2 mt-3">
          This lot may have been removed, cancelled, or never existed.
        </p>
        <Link
          to="/"
          className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-6"
        >
          Back to home
        </Link>
      </div>
      <Footer />
    </div>
  );
}

function LotSkeleton() {
  return (
    <div>
      <TopNav />
      <div className="max-w-[1280px] mx-auto px-7 py-8">
        <div className="bg-bg-soft rounded h-4 w-72 mb-6" />
        <div className="grid gap-9" style={{ gridTemplateColumns: '1fr 360px' }}>
          <div>
            <div className="bg-bg-soft rounded h-8 w-2/3 mb-6" />
            <div className="bg-bg-soft border border-border rounded-lg" style={{ aspectRatio: '1 / 1' }} />
          </div>
          <div className="bg-bg-soft border border-border rounded-lg h-[420px]" />
        </div>
      </div>
      <Footer />
    </div>
  );
}

export default function LotPage() {
  const { id } = useParams<{ id: string }>();
  const { data: lot, isLoading, error } = useLot(id);
  const { data: tree } = useCategoryTree();
  const [reportOpen, setReportOpen] = useState(false);

  const breadcrumbParts: BreadcrumbPart[] = useMemo(() => {
    const head: BreadcrumbPart = { label: 'Home', href: '/' };
    if (!lot) return [head];
    const path = findCategoryPath(tree, lot.category.id);
    if (path) {
      const trail: BreadcrumbPart[] = path.map((node) => ({
        label: node.name,
        href: `/category/${node.slug}`,
      }));
      return [head, ...trail, { label: lot.title }];
    }
    // Fallback: tree not loaded — use namePath as plain labels, link the leaf
    const trail: BreadcrumbPart[] = lot.category.namePath.map((name, i, arr) => ({
      label: name,
      href: i === arr.length - 1 ? `/category/${lot.category.slug}` : undefined,
    }));
    return [head, ...trail, { label: lot.title }];
  }, [lot, tree]);

  const subcategoryKind = useMemo(() => {
    if (!lot || !tree) return null;
    const path = findCategoryPath(tree, lot.category.id);
    const leaf = path?.[path.length - 1];
    return leaf?.subcategoryKind ?? null;
  }, [lot, tree]);

  if (isLoading) return <LotSkeleton />;
  if (error instanceof ApiError && error.status === 404) return <NotFound />;
  if (error || !lot) return <NotFound />;

  return (
    <div>
      <TopNav />

      <div className="max-w-[1280px] mx-auto px-7 pt-5 pb-2">
        <Breadcrumb parts={breadcrumbParts} />
      </div>

      {/* Title row (full width above the 2-column grid) */}
      <div className="max-w-[1280px] mx-auto px-7 pt-3">
        <div className="flex items-center gap-3 flex-wrap">
          <h1
            className="font-bold m-0 text-text"
            style={{ fontSize: 26, lineHeight: 1.25, letterSpacing: '-0.015em' }}
          >
            {lot.title}
          </h1>
          <div className="flex items-center gap-2">
            <ConditionBadge value={lot.condition} size="md" />
            <StatusPill status={lot.status} />
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-x-5 gap-y-1.5 mt-3 text-[12.5px] text-text-3">
          <span>
            <span className="font-semibold text-text">{lot.bidCount}</span>{' '}
            {lot.bidCount === 1 ? 'bid' : 'bids'}
          </span>
          <span>
            <span className="font-semibold text-text">{lot.viewCount}</span> views
          </span>
          <span>
            Ends <span className="text-text-2">{formatLocal(lot.endsAt)}</span>
          </span>
          <span>
            Seller{' '}
            <Link
              to={`/profile/${lot.seller.id}`}
              className="text-accent-deep hover:underline no-underline font-medium"
            >
              {lot.seller.displayName}
            </Link>{' '}
            <span className="mono text-text-3">({lot.seller.trustScore})</span>
          </span>
        </div>
      </div>

      {/* Main 2-column grid: gallery left, bid panel right (starts at same vertical level) */}
      <div
        className="max-w-[1280px] mx-auto px-7 pt-5 grid gap-9"
        style={{ gridTemplateColumns: '3fr 2fr' }}
      >
        {/* LEFT — gallery only */}
        <div>
          <ImageGallery images={lot.images} />
        </div>

        {/* RIGHT — bid panel stack (static, aligned to top of gallery) */}
        <div>
          <div className="flex flex-col gap-3">
            <BidPanel
              lotId={lot.id}
              sellerId={lot.seller.id}
              status={lot.status}
              startingPriceUahKopiykas={lot.startingPriceUahKopiykas}
              currentPriceUahKopiykas={lot.currentPriceUahKopiykas}
              bidCount={lot.bidCount}
              endsAt={lot.endsAt}
              isCallerLeading={lot.isCallerLeading}
              winningPriceUahKopiykas={lot.winningBid?.amountUahKopiykas}
              callerPaymentId={lot.callerPaymentId}
              callerPaymentStatus={lot.callerPaymentStatus}
            />


            {/* Seller card — separate block under bid panel */}
            <div className="bg-surface border border-border rounded-lg p-3.5">
              <div className="text-[11px] font-semibold uppercase tracking-wider text-text-3 mb-2.5">
                Seller
              </div>
              <div className="flex items-center gap-3">
                <div
                  className="rounded-full flex items-center justify-center text-white font-bold flex-shrink-0"
                  style={{
                    width: 40,
                    height: 40,
                    background: 'linear-gradient(135deg, #C8B380, #8A6A2A)',
                    fontSize: 14,
                  }}
                >
                  {lot.seller.displayName.slice(0, 2).toUpperCase()}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="mono text-[13.5px] font-semibold truncate">
                    {lot.seller.displayName}
                  </div>
                  <div className="text-[11.5px] text-text-3 flex items-center gap-1.5 mt-0.5">
                    <Icon name="shield" size={11} color="var(--color-accent-deep)" />
                    TrustScore{' '}
                    <span className="mono font-semibold text-text-2">{lot.seller.trustScore}</span>
                  </div>
                </div>
              </div>
              <Link
                to={`/profile/${lot.seller.id}`}
                className="inline-flex w-full items-center justify-center mt-3 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-2 text-[13px] no-underline"
              >
                Seller profile
              </Link>
            </div>

            {/* Trust mini-block */}
            <div className="bg-surface border border-border rounded-lg p-3.5">
              <div className="flex items-center gap-2 mb-2.5">
                <Icon name="shield" size={15} color="var(--color-accent-deep)" />
                <span className="text-[13px] font-semibold text-text">How this deal is protected</span>
              </div>
              <div className="flex flex-col gap-2.5 text-[12px]">
                {[
                  ['Stripe escrow', 'Funds held until delivery is confirmed'],
                  ['Nova Poshta', 'Tracked shipping, optional insurance'],
                  ['Verified seller', 'KYC via Stripe Connect Express'],
                ].map(([t, s]) => (
                  <div key={t}>
                    <div className="font-semibold text-text">{t}</div>
                    <div className="text-text-3">{s}</div>
                  </div>
                ))}
              </div>

              {/* Report footer — logically grouped with the trust/safety section. Outlined button so
                  it reads as a real action without competing with the primary bid CTA on this page. */}
              <div className="mt-3.5 pt-3.5 border-t border-border-soft">
                <div className="text-[12px] text-text-3 mb-2">Spot something off?</div>
                <button
                  type="button"
                  onClick={() => setReportOpen(true)}
                  className="inline-flex w-full items-center justify-center gap-1.5 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-2 text-[13px]"
                >
                  <Icon name="info" size={13} stroke={1.6} />
                  Report this lot
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Two-column info section. Left column stacks Attributes → Description so the
          description sits directly under the attributes block at the same width.
          Right column hosts the Bid history table. */}
      <div className="max-w-[1280px] mx-auto px-7 pt-10 pb-14">
        <div className="grid gap-9 items-start" style={{ gridTemplateColumns: '3fr 2fr' }}>
          <div className="flex flex-col gap-8 min-w-0">
            <section>
              <h2 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 m-0 mb-2">
                Attributes
              </h2>
              <AttributesTable
                attributes={lot.attributes ?? {}}
                subcategoryKind={subcategoryKind}
                headerRows={[
                  { label: 'Condition', value: lot.condition },
                  { label: 'Category', value: lot.category.namePath.join(' › ') },
                ]}
              />
            </section>

            <section>
              <h2 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 m-0 mb-2.5">
                Description
              </h2>
              <MarkdownView source={lot.description} />
            </section>
          </div>

          <section className="min-w-0">
            <div className="flex items-baseline justify-between mb-3">
              <h2 className="text-[11px] font-semibold uppercase tracking-wider text-text-3 m-0">
                Bid history · {lot.bidCount}
              </h2>
              <span className="text-[12px] text-text-3">
                Anonymized until close
              </span>
            </div>
            <div className="border-t border-border-soft pt-4">
              <BidHistory lotId={lot.id} />
            </div>
          </section>
        </div>
      </div>

      <Footer />

      <ReportLotModal open={reportOpen} lotId={lot.id} onClose={() => setReportOpen(false)} />
    </div>
  );
}
