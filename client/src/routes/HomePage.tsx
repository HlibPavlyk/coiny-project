import { Link } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotCard } from '@/components/LotCard';
import { Icon } from '@/components/Icon';
import { useCategoryTree, type CategoryNode } from '@/api/categories';
import { useLotsByCategory } from '@/api/lots';

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
            {root.lotCountActive} lots
          </span>
          <Icon name="arrowR" size={16} color={style.accent} />
        </div>
      </div>
    </Link>
  );
}

function LotsSection({
  title,
  subtitle,
  categoryId,
  sort,
  count,
}: {
  title: string;
  subtitle: string;
  categoryId: number | undefined;
  sort: { columnName: string; direction: 'Asc' | 'Desc' };
  count: number;
}) {
  const { data, isLoading } = useLotsByCategory(categoryId, {
    offset: 0,
    count,
    sortBy: [sort],
  });

  return (
    <section className="max-w-[1280px] mx-auto px-7 pt-10">
      <div className="flex items-baseline justify-between mb-4 flex-wrap gap-3">
        <div>
          <h2 className="text-[22px] font-semibold m-0">{title}</h2>
          <p className="text-[13px] text-text-3 mt-1 m-0">{subtitle}</p>
        </div>
        <Link
          to="/search"
          className="text-[13px] font-medium text-accent-deep hover:underline no-underline inline-flex items-center gap-1"
        >
          View all
          <Icon name="arrowR" size={13} />
        </Link>
      </div>
      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
          {Array.from({ length: count }).map((_, i) => (
            <div key={i} className="bg-bg-soft border border-border rounded-lg" style={{ aspectRatio: '0.78' }} />
          ))}
        </div>
      ) : data && data.items.length > 0 ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
          {data.items.map((lot) => (
            <LotCard key={lot.id} lot={lot} compact />
          ))}
        </div>
      ) : (
        <div className="bg-surface border border-dashed border-border rounded-lg py-10 px-6 text-center">
          <p className="text-text-3 text-sm">No active lots yet — check back soon.</p>
        </div>
      )}
    </section>
  );
}

export default function HomePage() {
  const { data: tree } = useCategoryTree();
  const firstRootId = tree?.roots[0]?.id;

  return (
    <div>
      <TopNav />

      {/* Hero */}
      <section
        className="border-b border-border"
        style={{ background: 'linear-gradient(180deg, #FAFAF7 0%, #F2EEE2 100%)' }}
      >
        <div
          className="max-w-[1280px] mx-auto px-7 grid gap-14 items-center"
          style={{ gridTemplateColumns: '1.15fr 1fr', padding: '56px 28px 64px' }}
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
              className="font-bold m-0 text-text"
              style={{ fontSize: 52, lineHeight: 1.05, letterSpacing: '-0.025em' }}
            >
              Trusted Ukrainian
              <br />
              <span className="text-accent-deep">numismatic auctions</span>
            </h1>
            <p className="text-[17px] text-text-2 mt-4 max-w-[480px]" style={{ lineHeight: 1.55 }}>
              Coins, banknotes, medals and orders with guaranteed payment. Funds are released to
              sellers only after Nova Poshta confirms delivery.
            </p>
            <div className="flex gap-2.5 mt-7">
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
            <div className="flex gap-8 mt-9 pt-6 border-t border-border">
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
          <div className="relative" style={{ height: 420 }}>
            <img
              src="/hero/banknote.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{
                position: 'absolute',
                top: 40,
                left: -20,
                width: 360,
                height: 'auto',
                transform: 'rotate(-6deg)',
                filter: 'drop-shadow(0 22px 40px rgba(60, 40, 20, 0.28))',
                zIndex: 1,
              }}
            />
            <img
              src="/hero/medal.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{
                position: 'absolute',
                top: -20,
                right: -60,
                width: 360,
                height: 360,
                objectFit: 'contain',
                transform: 'rotate(10deg)',
                filter: 'drop-shadow(0 24px 36px rgba(60, 30, 20, 0.32))',
                zIndex: 2,
              }}
            />
            <img
              src="/hero/coin.png"
              alt=""
              loading="eager"
              decoding="async"
              style={{
                position: 'absolute',
                bottom: 0,
                left: 10,
                width: 220,
                height: 220,
                objectFit: 'contain',
                transform: 'rotate(-8deg)',
                filter: 'drop-shadow(0 24px 36px rgba(86, 56, 18, 0.35))',
                zIndex: 3,
              }}
            />
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="max-w-[1280px] mx-auto px-7 pt-10 pb-2">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {tree?.roots.map((root, idx) => (
            <CategoryCard key={root.id} root={root} idx={idx} />
          ))}
        </div>
      </section>

      <LotsSection
        title="Ending soon"
        subtitle="Auctions closing the soonest"
        categoryId={firstRootId}
        sort={{ columnName: 'endsAt', direction: 'Asc' }}
        count={6}
      />

      <LotsSection
        title="New listings"
        subtitle="Recently added by sellers"
        categoryId={firstRootId}
        sort={{ columnName: 'createdAt', direction: 'Desc' }}
        count={6}
      />

      <Footer />
    </div>
  );
}
