import { useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { AvatarLarge } from '@/components/AvatarLarge';
import { LotCard } from '@/components/LotCard';
import { Icon } from '@/components/Icon';
import { users } from '@/api/users';
import { formatKopiykasAsUah } from '@/lib/money';
import { ApiError } from '@/api/fetch';
import { Skeleton, SkeletonLine } from '@/components/Skeleton';

/**
 * Public seller profile per design-brief §2.7. No auth required.
 * Hero (avatar / displayName / trust score / member-since) + 4 stats + Active|Sold|Reviews tabs.
 * Banned users surface as 404 (backend returns NotFound to avoid leaking moderation state).
 */
type TabKey = 'active' | 'sold' | 'reviews';

const TABS: { key: TabKey; label: string }[] = [
  { key: 'active', label: 'Active lots' },
  { key: 'sold', label: 'Sold lots' },
  { key: 'reviews', label: 'Reviews' },
];

export default function PublicProfilePage() {
  const { userId = '' } = useParams<{ userId: string }>();
  const [tab, setTab] = useState<TabKey>('active');

  const profile = useQuery({
    queryKey: ['public-profile', userId],
    queryFn: () => users.getPublicProfile(userId),
    enabled: !!userId,
    retry: (failureCount, err) => !(err instanceof ApiError && err.status === 404) && failureCount < 2,
  });

  const lots = useQuery({
    queryKey: ['public-profile-lots', userId, tab],
    queryFn: () =>
      users.searchLotsBySeller(userId, tab === 'sold' ? 'Sold' : 'Active', {
        offset: 0,
        count: 24,
        sortBy: [{ columnName: tab === 'sold' ? 'EndsAt' : 'EndsAt', direction: 'Asc' }],
      }),
    enabled: !!userId && tab !== 'reviews' && profile.isSuccess,
  });

  const initials = useMemo(() => {
    const name = profile.data?.displayName ?? '';
    return name.trim().slice(0, 2).toUpperCase() || '??';
  }, [profile.data?.displayName]);

  const memberSinceLabel = useMemo(() => {
    if (!profile.data?.memberSince) return '';
    return new Date(profile.data.memberSince).toLocaleDateString('en-US', {
      month: 'long',
      year: 'numeric',
    });
  }, [profile.data?.memberSince]);

  const lastActiveLabel = useMemo(() => {
    if (!profile.data?.lastActiveAt) return '';
    return relativeTime(new Date(profile.data.lastActiveAt));
  }, [profile.data?.lastActiveAt]);

  return (
    <div>
      <TopNav />
      <div className="max-w-[1180px] mx-auto px-4 sm:px-7 py-6 sm:py-10">
        {profile.isLoading && (
          <div className="mt-4 space-y-4">
            <SkeletonLine width="w-1/3" />
            <SkeletonLine width="w-2/3" />
            <Skeleton className="h-32 w-full mt-4" />
          </div>
        )}

        {profile.isError && profile.error instanceof ApiError && profile.error.status === 404 && (
          <div className="mt-8 text-center">
            <h1 className="text-3xl font-bold m-0">User not found</h1>
            <p className="text-text-3 text-[14px] mt-3">
              This profile does not exist, or the user has been deactivated.
            </p>
          </div>
        )}

        {profile.isSuccess && (
          <>
            {/* Hero — on mobile: avatar + name side-by-side (compact), then meta below, then Report at end. */}
            <section className="flex items-start gap-4 sm:gap-6 md:items-center">
              <AvatarLarge initials={initials} size={72} />
              <div className="flex-1 min-w-0">
                <h1 className="text-[22px] sm:text-3xl font-bold m-0 leading-tight break-words">
                  {profile.data.displayName}
                </h1>
                <div className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 text-[12.5px] sm:text-[13.5px] text-text-3">
                  <span className="inline-flex items-center gap-1.5">
                    <Icon name="medal" size={14} stroke={1.8} color="var(--color-accent-deep)" />
                    Trust score <span className="mono font-semibold text-text">{profile.data.trustScore}</span>
                  </span>
                  <span aria-hidden="true">·</span>
                  <span>Member since {memberSinceLabel}</span>
                  {lastActiveLabel && (
                    <>
                      <span aria-hidden="true">·</span>
                      <span>Last active {lastActiveLabel}</span>
                    </>
                  )}
                </div>
                <button
                  type="button"
                  onClick={() => {
                    alert(
                      'To report a seller, please report one of their active lots. Admins review reports tied to specific listings and can ban the seller from the moderation queue.',
                    );
                  }}
                  className="mt-3 sm:hidden inline-flex items-center gap-1.5 rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-3 py-1.5 text-[12.5px]"
                >
                  <Icon name="shield" size={13} stroke={1.8} />
                  Report user
                </button>
              </div>
              {/* Desktop Report button — separate column on md+ so it sits next to the meta. */}
              <button
                type="button"
                onClick={() => {
                  alert(
                    'To report a seller, please report one of their active lots. Admins review reports tied to specific listings and can ban the seller from the moderation queue.',
                  );
                }}
                className="hidden sm:inline-flex self-start items-center gap-1.5 rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-3.5 py-2 text-[12.5px]"
              >
                <Icon name="shield" size={13} stroke={1.8} />
                Report user
              </button>
            </section>

            {/* Stats */}
            <section className="mt-7 grid grid-cols-2 md:grid-cols-4 gap-3.5">
              <StatCell label="Lots sold" value={profile.data.lotsSold.toLocaleString('en-US')} />
              <StatCell label="Active lots" value={profile.data.activeLots.toLocaleString('en-US')} />
              <StatCell
                label="Avg sale price"
                value={
                  profile.data.lotsSold === 0
                    ? '—'
                    : formatKopiykasAsUah(profile.data.avgSalePriceUahKopiykas)
                }
              />
              <StatCell label="Last active" value={lastActiveLabel || '—'} />
            </section>

            {/* Tabs */}
            <nav className="mt-8 border-b border-border flex gap-1">
              {TABS.map((t) => {
                const isActive = t.key === tab;
                return (
                  <button
                    key={t.key}
                    type="button"
                    onClick={() => setTab(t.key)}
                    className="px-4 py-2.5 text-[13.5px] font-medium transition"
                    style={{
                      color: isActive ? 'var(--color-text)' : 'var(--color-text-3)',
                      borderBottom: isActive ? '2px solid var(--color-accent-deep)' : '2px solid transparent',
                      marginBottom: -1,
                    }}
                  >
                    {t.label}
                  </button>
                );
              })}
            </nav>

            {/* Tab content */}
            <div className="mt-6">
              {tab === 'reviews' ? (
                <div className="rounded-lg border border-border bg-surface p-10 text-center">
                  <div
                    className="w-14 h-14 mx-auto mb-3.5 rounded-full flex items-center justify-center"
                    style={{ background: 'var(--color-bg-soft)' }}
                  >
                    <Icon name="star" size={26} color="var(--color-text-3)" stroke={1.4} />
                  </div>
                  <div className="text-[15px] font-semibold">Reviews coming soon</div>
                  <p className="text-[13px] text-text-3 mt-1.5 max-w-[360px] mx-auto leading-relaxed">
                    Buyer & seller ratings will appear here after we ship the reviews feature.
                  </p>
                </div>
              ) : (
                <LotsGrid
                  status={tab}
                  isLoading={lots.isLoading}
                  items={lots.data?.items ?? []}
                />
              )}
            </div>
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}

function StatCell({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-border bg-surface px-4 py-3.5">
      <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3">{label}</div>
      <div className="mono text-[19px] font-bold mt-1" style={{ letterSpacing: '-0.01em' }}>
        {value}
      </div>
    </div>
  );
}

function LotsGrid({
  status,
  isLoading,
  items,
}: {
  status: 'active' | 'sold';
  isLoading: boolean;
  items: { id: string; title: string; coverImageUrl: string; currentPriceUahKopiykas: number; bidCount: number; endsAt: string }[];
}) {
  if (isLoading) {
    return <p className="text-text-3 text-[13.5px]">Loading…</p>;
  }
  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-surface p-10 text-center text-text-3 text-[13.5px]">
        {status === 'active'
          ? 'This seller has no active lots.'
          : 'This seller has not sold any lots yet.'}
      </div>
    );
  }
  return (
    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
      {items.map((lot) => (
        <LotCard key={lot.id} lot={lot} />
      ))}
    </div>
  );
}

// Compact relative-time helper. Localized to en-US per project convention.
function relativeTime(date: Date): string {
  const seconds = Math.max(0, Math.floor((Date.now() - date.getTime()) / 1000));
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months}mo ago`;
  return `${Math.floor(months / 12)}y ago`;
}
