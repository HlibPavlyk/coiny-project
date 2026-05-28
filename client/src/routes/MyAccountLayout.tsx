import { Outlet, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { MyAccountSidebar } from '@/components/MyAccountSidebar';
import { useAuthStore } from '@/state/useAuthStore';
import { payments } from '@/api/payments';
import { lots } from '@/api/lots';
import { bids } from '@/api/bids';

/** Static route → page-title map. Keeps the H1 above the grid so the sidebar aligns with content. */
const HEADERS: Record<string, { title: string; subtitle?: string }> = {
  '/profile': {
    title: 'My account',
    subtitle: 'Manage your profile, payouts, and identity.',
  },
  '/my-lots': {
    title: 'My lots',
    subtitle: 'Lots you have listed, in any status — drafts, active auctions, sold, or ended.',
  },
  '/my-bids': {
    title: 'My bids',
    subtitle: 'Auctions you are bidding on, won, or lost.',
  },
  '/my-purchases': {
    title: 'My purchases',
    subtitle: 'Lots you have won — checkout, payment, shipping, and delivery.',
  },
};

/**
 * Shared chrome for every <c>/profile, /my-lots, /my-bids, /my-purchases</c> page.
 *
 * Mounted as the React Router parent element — TopNav, page header, sidebar, and Footer stay alive
 * across child navigations; only the Outlet content swaps. The H1 is rendered ABOVE the grid so the
 * sidebar aligns with the page's primary content. Tiny count-only queries here populate the sidebar
 * badges; React Query caches them across navigations.
 */
export function MyAccountLayout() {
  const { pathname } = useLocation();
  const header = HEADERS[pathname];
  const isAuthed = useAuthStore((s) => !!s.user);

  // Three tiny count-only queries (offset 0, count 1) — we only read `.totalCount` for the badges.
  // 30s staleTime keeps them warm during a normal session without missing fresh data after actions.
  const { data: lotsData } = useQuery({
    queryKey: ['my-lots', 'count'],
    queryFn: () =>
      lots.myLotsSearch({
        offset: 0,
        count: 1,
        sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
      }),
    enabled: isAuthed,
    staleTime: 30_000,
  });
  const { data: bidsData } = useQuery({
    queryKey: ['my-bids', 'count'],
    queryFn: () =>
      bids.myBidsSearch({
        offset: 0,
        count: 1,
        sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
      }),
    enabled: isAuthed,
    staleTime: 30_000,
  });
  const { data: purchasesData } = useQuery({
    queryKey: ['my-purchases', 'count'],
    queryFn: () =>
      payments.myPurchasesSearch({
        offset: 0,
        count: 1,
        sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
      }),
    enabled: isAuthed,
    staleTime: 30_000,
  });

  return (
    <div>
      <TopNav />
      <div className="max-w-[1180px] mx-auto px-7 py-8">
        {header && (
          <div className="mb-4">
            <h1 className="text-[28px] font-bold tracking-tight m-0">{header.title}</h1>
            {header.subtitle && (
              <p className="text-[13.5px] text-text-3 m-0 mt-1">{header.subtitle}</p>
            )}
          </div>
        )}
        <div className="grid gap-5" style={{ gridTemplateColumns: '240px 1fr' }}>
          <MyAccountSidebar
            counts={{
              lots: lotsData?.totalCount,
              bids: bidsData?.totalCount,
              purchases: purchasesData?.totalCount,
            }}
          />
          <div>
            <Outlet />
          </div>
        </div>
      </div>
      <Footer />
    </div>
  );
}
