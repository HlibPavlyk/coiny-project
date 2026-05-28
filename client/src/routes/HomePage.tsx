import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotCard } from '@/components/LotCard';
import { LotImagePlaceholder } from '@/components/LotImagePlaceholder';
import { CountdownTimer } from '@/components/CountdownTimer';
import { Icon, type IconName } from '@/components/Icon';
import { useCategoryTree, subtreeLotCount, type CategoryNode } from '@/api/categories';
import { usePublicLots, type PublicLotsRequest } from '@/api/lots';
import { formatKopiykasAsUah } from '@/lib/money';

const categoryCardStyles = [
  { bg: '#F2E9D5', accent: '#A8763E', icon: 'coin' as const, sub: 'Ukraine · USSR · Imperial · World' },
  { bg: '#E5DDC9', accent: '#8C5F2E', icon: 'bill' as const, sub: 'NBU · USSR · Imperial · World' },
  { bg: '#E9D8B9', accent: '#7C5A1F', icon: 'medal' as const, sub: 'Soviet · Military · Civilian' },
];

function CategoryCard({ root, idx }: { root: CategoryNode; idx: number }) {
  const style = categoryCardStyles[idx] ?? categoryCardStyles[0];
  return (
    <Link
      to={`/category/${root.slug}`}
      className="relative rounded-lg overflow-hidden no-underline cursor-pointer transition hover:-translate-y-px"
      style={{ background: style.bg, padding: '24px 24px 22px' }}
    >
      <div className="absolute -top-5 -right-5 opacity-20">
        <Icon name={style.icon} size={140} color={style.accent} stroke={1.2} />
      </div>
      <div className="relative">
        <div className="text-[22px] font-semibold mb-1 text-text">{root.name}</div>
        <div className="text-[13px] text-text-2 mb-3.5">{style.sub}</div>
        <div className="flex items-center justify-between">
          <span className="mono text-[13px] font-semibold" style={{ color: style.accent }}>
            {subtreeLotCount(root)} lots
          </span>
          <Icon name="arrowR" size={16} color={style.accent} />
        </div>
      </div>
    </Link>
  );
}

/** Compact swipeable category chip — used on mobile in place of the full CategoryCard. */
function CategoryChip({ root, idx }: { root: CategoryNode; idx: number }) {
  const style = categoryCardStyles[idx] ?? categoryCardStyles[0];
  return (
    <Link
      to={`/category/${root.slug}`}
      className="flex-shrink-0 inline-flex items-center gap-2 rounded-full no-underline cursor-pointer transition hover:-translate-y-px snap-start"
      style={{ background: style.bg, padding: '8px 14px' }}
    >
      <Icon name={style.icon} size={16} color={style.accent} stroke={1.6} />
      <span className="text-[14px] font-semibold text-text">{root.name}</span>
      <span className="mono text-[12px] font-semibold" style={{ color: style.accent }}>
        · {subtreeLotCount(root)}
      </span>
    </Link>
  );
}

function EmptyHint({ message = 'No lots here yet — check back soon.' }: { message?: string }) {
  return (
    <div className="bg-surface border border-dashed border-border rounded-lg py-10 px-6 text-center">
      <p className="text-text-3 text-sm m-0">{message}</p>
    </div>
  );
}

/** Section-specific empty-state copy. Generic fallback is fine for "Hot right now" and similar. */
const EMPTY_MESSAGES: Record<string, string> = {
  'Ending soon': 'No auctions ending in the next 24 hours.',
  'New listings': 'No new listings yet — be the first to list one!',
  'Recently sold': 'No lots have sold yet — closed wins will appear here.',
  'Record prices': 'No record-setting prices to celebrate yet.',
  'Hot right now': 'Quiet day — no hot bidding wars right now.',
};

/** Full-width panel whose body is a responsive grid of lot cards. */
function LotGridSection({
  title,
  icon,
  to,
  request,
  sold,
}: {
  title: string;
  icon?: IconName;
  to: string;
  request: PublicLotsRequest;
  sold?: boolean;
}) {
  const { data, isLoading } = usePublicLots(request);
  return (
    <section className="max-w-[1280px] mx-auto px-4 sm:px-7 pt-6 sm:pt-10">
      <Panel title={title} icon={icon} to={to} count={data?.totalCount}>
        <div className="py-2 sm:p-4">
          {isLoading ? (
            <div
              className="flex sm:grid sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-3 sm:gap-4 overflow-x-auto snap-x snap-mandatory sm:overflow-visible px-3 sm:px-0"
              style={{ scrollPaddingInline: '12px' }}
            >
              {Array.from({ length: 6 }).map((_, i) => (
                <div
                  key={i}
                  className="bg-bg-soft border border-border rounded-lg flex-shrink-0 w-[calc(50%-6px)] sm:w-auto snap-start"
                  style={{ aspectRatio: '0.78' }}
                />
              ))}
            </div>
          ) : data && data.items.length > 0 ? (
            <div
              className="flex sm:grid sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-3 sm:gap-4 overflow-x-auto snap-x snap-mandatory sm:overflow-visible px-3 sm:px-0"
              style={{ scrollPaddingInline: '12px' }}
            >
              {data.items.map((lot) => (
                <div key={lot.id} className="flex-shrink-0 w-[calc(50%-6px)] sm:w-auto snap-start">
                  <LotCard lot={lot} compact sold={sold} />
                </div>
              ))}
            </div>
          ) : (
            <EmptyHint message={EMPTY_MESSAGES[title]} />
          )}
        </div>
      </Panel>
    </section>
  );
}

/** Bordered dashboard panel: an icon+title header (with optional count and "View all"), then a body. */
function Panel({
  title,
  icon,
  to,
  count,
  className = '',
  children,
}: {
  title: string;
  icon?: IconName;
  to?: string;
  count?: number;
  /** Extra classes on the root — e.g. `flex-1` to fill a stretched column. */
  className?: string;
  children: ReactNode;
}) {
  const headerInner = (
    <>
      <div className="flex items-center gap-2 min-w-0">
        {icon && <Icon name={icon} size={15} color="var(--color-accent-deep)" />}
        <h2 className="text-[15px] font-semibold m-0 truncate">{title}</h2>
        {count !== undefined && <span className="mono text-[12px] text-text-3">{count}</span>}
      </div>
      {to && (
        <span className="text-[12px] font-medium text-accent-deep group-hover:underline inline-flex items-center gap-1 shrink-0">
          View all
          <Icon name="arrowR" size={12} />
        </span>
      )}
    </>
  );

  return (
    <div className={`bg-surface border border-border rounded-lg overflow-hidden flex flex-col ${className}`}>
      {to ? (
        <Link
          to={to}
          className="group flex items-center justify-between px-4 py-3 border-b border-border shrink-0 no-underline text-text hover:bg-bg-soft transition-colors"
        >
          {headerInner}
        </Link>
      ) : (
        <div className="flex items-center justify-between px-4 py-3 border-b border-border shrink-0">{headerInner}</div>
      )}
      {children}
    </div>
  );
}

/** Panel whose body is a fixed 3-column grid of compact cards (up to 9). */
function GridPanel({
  title,
  icon,
  to,
  request,
  sold,
}: {
  title: string;
  icon?: IconName;
  to: string;
  request: PublicLotsRequest;
  sold?: boolean;
}) {
  const { data, isLoading } = usePublicLots(request);
  const items = data?.items ?? [];
  return (
    <Panel title={title} icon={icon} to={to} count={data?.totalCount}>
      <div className="py-2 sm:p-4">
        {isLoading ? (
          <div
            className="flex sm:grid sm:grid-cols-3 gap-3 overflow-x-auto snap-x snap-mandatory sm:overflow-visible px-3 sm:px-0"
            style={{ scrollPaddingInline: '12px' }}
          >
            {Array.from({ length: 9 }).map((_, i) => (
              <div
                key={i}
                className="bg-bg-soft border border-border rounded-lg flex-shrink-0 w-[calc(50%-6px)] sm:w-auto snap-start"
                style={{ aspectRatio: '0.78' }}
              />
            ))}
          </div>
        ) : items.length > 0 ? (
          <div
            className="flex sm:grid sm:grid-cols-3 gap-3 overflow-x-auto snap-x snap-mandatory sm:overflow-visible px-3 sm:px-0"
            style={{ scrollPaddingInline: '12px' }}
          >
            {items.slice(0, 9).map((lot) => (
              <div key={lot.id} className="flex-shrink-0 w-[calc(50%-6px)] sm:w-auto snap-start">
                <LotCard lot={lot} compact sold={sold} />
              </div>
            ))}
          </div>
        ) : (
          <EmptyHint message={EMPTY_MESSAGES[title]} />
        )}
      </div>
    </Panel>
  );
}

/** Panel showing a single hero lot: a wide cover image with title, price and live countdown beneath. */
function BannerPanel({
  title,
  icon,
  to,
  request,
}: {
  title: string;
  icon?: IconName;
  to: string;
  request: PublicLotsRequest;
}) {
  const { data } = usePublicLots(request);
  const lot = data?.items?.[0];
  return (
    <Panel title={title} icon={icon} to={to}>
      {lot ? (
        <Link to={`/lot/${lot.id}`} className="group block no-underline text-text">
          <div className="relative bg-bg-soft overflow-hidden" style={{ aspectRatio: '16 / 10' }}>
            {lot.coverImageUrl ? (
              <img
                src={lot.coverImageUrl}
                alt=""
                loading="lazy"
                decoding="async"
                className="absolute inset-0 w-full h-full object-cover transition duration-300 group-hover:scale-[1.03]"
              />
            ) : (
              <LotImagePlaceholder kind="coin" variant={lot.id.charCodeAt(0) % 6} />
            )}
          </div>
          <div className="px-4 py-3 flex items-center justify-between gap-3">
            <div className="min-w-0">
              <div className="text-sm font-medium truncate" title={lot.title}>
                {lot.title}
              </div>
              <div className="text-[12px] text-text-3 mt-0.5">
                {lot.bidCount} {lot.bidCount === 1 ? 'bid' : 'bids'}
              </div>
            </div>
            <div className="text-right shrink-0">
              <div className="mono text-[17px] font-bold text-accent-deep" style={{ letterSpacing: '-0.01em' }}>
                {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
              </div>
              <div className="mt-1 flex justify-end">
                <CountdownTimer endsAt={lot.endsAt} size="sm" />
              </div>
            </div>
          </div>
        </Link>
      ) : (
        <div className="p-4">
          <EmptyHint message={EMPTY_MESSAGES[title]} />
        </div>
      )}
    </Panel>
  );
}

/** Panel listing lots as compact numbered rows (thumb · title · price) — a leaderboard treatment. */
function RankedListPanel({
  title,
  icon,
  to,
  request,
  sold,
}: {
  title: string;
  icon?: IconName;
  to: string;
  request: PublicLotsRequest;
  sold?: boolean;
}) {
  const { data } = usePublicLots(request);
  const items = data?.items ?? [];
  return (
    <Panel title={title} icon={icon} to={to} className="flex-1">
      {items.length > 0 ? (
        <ul className="flex-1 flex flex-col divide-y divide-border">
          {items.slice(0, 8).map((lot, i) => (
            <li key={lot.id} className="flex-1">
              <Link
                to={`/lot/${lot.id}`}
                className="h-full flex items-center gap-3 px-4 py-2.5 no-underline text-text hover:bg-bg-soft transition-colors"
              >
                <span className="mono text-[13px] font-semibold text-text-3 w-4 text-right shrink-0">{i + 1}</span>
                <div className="relative w-11 h-11 rounded bg-bg-soft overflow-hidden shrink-0">
                  {lot.coverImageUrl ? (
                    <img
                      src={lot.coverImageUrl}
                      alt=""
                      loading="lazy"
                      decoding="async"
                      className="absolute inset-0 w-full h-full object-cover"
                    />
                  ) : (
                    <LotImagePlaceholder kind="coin" variant={i % 6} />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <div className="text-[13px] leading-snug truncate" title={lot.title}>
                    {lot.title}
                  </div>
                  <div className="text-[11px] text-text-3 mt-0.5">
                    {sold ? 'Sold' : `${lot.bidCount} ${lot.bidCount === 1 ? 'bid' : 'bids'}`}
                  </div>
                </div>
                <div className="mono text-[14px] font-bold text-accent-deep shrink-0" style={{ letterSpacing: '-0.01em' }}>
                  {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
                </div>
              </Link>
            </li>
          ))}
        </ul>
      ) : (
        <div className="p-4">
          <EmptyHint message={EMPTY_MESSAGES[title]} />
        </div>
      )}
    </Panel>
  );
}

export default function HomePage() {
  const { data: tree } = useCategoryTree();

  return (
    <div>
      <TopNav />

      {/* Hero */}
      <section
        className="border-b border-border overflow-hidden"
        style={{ background: 'linear-gradient(180deg, #FAFAF7 0%, #F2EEE2 100%)' }}
      >
        <div
          className="max-w-[1280px] mx-auto px-5 md:px-7 grid gap-8 md:gap-14 items-center grid-cols-1 lg:grid-cols-[1.15fr_1fr] py-10 md:py-14"
        >
          <div>
            <div
              className="inline-flex items-center gap-1.5 rounded-full font-semibold mb-5"
              style={{
                padding: '5px 11px',
                background: 'var(--color-accent-tint)',
                color: 'var(--color-accent-deep)',
                fontSize: 12,
              }}
            >
              <Icon name="shield" size={12} color="var(--color-accent-deep)" />
              Stripe escrow · Funds held until delivery
            </div>
            <h1
              className="font-bold m-0 text-text text-[34px] md:text-[44px] lg:text-[52px]"
              style={{ lineHeight: 1.05, letterSpacing: '-0.025em' }}
            >
              Trusted Ukrainian
              <br />
              <span className="text-accent-deep">numismatic auctions</span>
            </h1>
            <p className="text-[15px] md:text-[17px] text-text-2 mt-4 max-w-[480px]" style={{ lineHeight: 1.55 }}>
              Coins, banknotes, medals and orders with guaranteed payment. Funds are released to
              sellers only after Nova Poshta confirms delivery.
            </p>
            <div className="flex flex-col sm:flex-row gap-2.5 mt-7">
              <Link
                to="/search"
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline transition"
              >
                Browse lots
              </Link>
              <a className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm cursor-pointer transition">
                How it works
              </a>
            </div>
            <div className="flex flex-wrap gap-6 sm:gap-8 mt-9 pt-6 border-t border-border">
              {[
                ['4,217', 'active lots'],
                ['12,480', 'collectors'],
                ['98%', 'successful trades'],
              ].map(([n, l]) => (
                <div key={l}>
                  <div className="mono text-2xl font-bold" style={{ letterSpacing: '-0.01em' }}>
                    {n}
                  </div>
                  <div className="text-xs text-text-3 mt-0.5">{l}</div>
                </div>
              ))}
            </div>
          </div>
          {/* Decorative coin/medal/banknote — hidden on mobile, scaled on tablet, full on desktop.
              `overflow-hidden` on the parent section is what tames the negative offsets. */}
          <div className="hidden lg:block relative" style={{ height: 420 }}>
            <img
              src="/hero/banknote.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{ position: 'absolute', top: 40, left: -20, width: 360, height: 'auto', transform: 'rotate(-6deg)', filter: 'drop-shadow(0 22px 40px rgba(60, 40, 20, 0.28))', zIndex: 1 }}
            />
            <img
              src="/hero/medal.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{ position: 'absolute', top: -20, right: -60, width: 360, height: 360, objectFit: 'contain', transform: 'rotate(10deg)', filter: 'drop-shadow(0 24px 36px rgba(60, 30, 20, 0.32))', zIndex: 2 }}
            />
            <img
              src="/hero/coin.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{ position: 'absolute', bottom: 0, left: 10, width: 220, height: 220, objectFit: 'contain', transform: 'rotate(-8deg)', filter: 'drop-shadow(0 24px 36px rgba(86, 56, 18, 0.35))', zIndex: 3 }}
            />
          </div>
        </div>
      </section>

      {/* Category cards */}
      {/* Mobile: horizontal-swipe category chips (~50px); Desktop: 3-column big cards.
          Only `pt` here — the next section provides matching gap below via its own `pt`. Doubling
          (categories pb + next section pt) is what created the visible asymmetry user reported. */}
      <section className="max-w-[1280px] mx-auto pt-6 sm:pt-10">
        {/* Extra left/right padding on the scroll container so the first chip doesn't kiss the
            viewport edge. `scroll-padding-inline` keeps snap-start aligned with that inset. */}
        <div
          className="flex md:hidden gap-2 overflow-x-auto snap-x snap-mandatory px-5"
          style={{ scrollbarWidth: 'none', scrollPaddingInline: '20px' }}
        >
          {tree?.roots.map((root, idx) => (
            <CategoryChip key={root.id} root={root} idx={idx} />
          ))}
        </div>
        <div className="hidden md:grid grid-cols-3 gap-4 px-7">
          {tree?.roots.map((root, idx) => (
            <CategoryCard key={root.id} root={root} idx={idx} />
          ))}
        </div>
      </section>

      {/* Mosaic: wide live grid on the left; featured banner + record-price leaderboard on the right */}
      <section className="max-w-[1280px] mx-auto px-4 sm:px-7 pt-6 sm:pt-10">
        <div className="grid gap-6 items-stretch grid-cols-1 lg:grid-cols-[1.5fr_1fr]">
          <GridPanel
            title="Ending soon"
            icon="clock"
            to="/search?sort=endsAt"
            request={{ offset: 0, count: 9, sortBy: [{ columnName: 'endsAt', direction: 'Asc' }], filters: { status: 'Active' } }}
          />
          <div className="flex flex-col gap-6">
            <BannerPanel
              title="Hot right now"
              icon="flame"
              to="/search"
              request={{ offset: 0, count: 1, sortBy: [{ columnName: 'bidCount', direction: 'Desc' }], filters: { status: 'Active' } }}
            />
            <RankedListPanel
              title="Record prices"
              icon="star"
              to="/search?status=Sold&sort=priceDesc"
              request={{ offset: 0, count: 8, sortBy: [{ columnName: 'currentPriceUahKopiykas', direction: 'Desc' }], filters: { status: 'Sold' } }}
              sold
            />
          </div>
        </div>
      </section>

      {/* History: recently sold */}
      <LotGridSection
        title="Recently sold"
        icon="package"
        to="/search?status=Sold"
        request={{ offset: 0, count: 6, sortBy: [{ columnName: 'endsAt', direction: 'Desc' }], filters: { status: 'Sold' } }}
        sold
      />

      {/* Live: newest */}
      <LotGridSection
        title="New listings"
        icon="plus"
        to="/search?sort=newest"
        request={{ offset: 0, count: 6, sortBy: [{ columnName: 'createdAt', direction: 'Desc' }], filters: { status: 'Active' } }}
      />

      <div className="pb-6" />
      <Footer />
    </div>
  );
}
