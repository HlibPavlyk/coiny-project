import { Outlet, useLocation } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { ModerationSidebar } from '@/components/ModerationSidebar';
import { useReports } from '@/api/moderation';

/** Static route → page-title map. Keeps the H1 above the grid so the sidebar aligns with content. */
const HEADERS: Record<string, { title: string; subtitle?: string }> = {
  '/moderation': { title: 'Moderation', subtitle: 'Review reports and act on lots and users.' },
  '/moderation/reports': { title: 'Reports', subtitle: 'Triage open reports — dismiss or take action.' },
  '/moderation/users': { title: 'Users', subtitle: 'Ban or unban an account directly.' },
  '/moderation/lots': { title: 'Lots', subtitle: 'Take a lot down without going through a report.' },
  '/moderation/demo': { title: 'Demo controls', subtitle: 'Short-circuit time-based triggers to walk the full workflow on stage.' },
};

/**
 * Shared chrome for every <c>/moderation/*</c> page.
 *
 * Mounted as the React Router parent element — TopNav, page header, sidebar, and Footer stay alive
 * across child navigations; only the Outlet content swaps. The H1 is rendered ABOVE the grid so the
 * sidebar visually aligns with the page's content cards. The Reports-queue badge is fetched once
 * here and stays cached for the whole moderation session.
 */
export function ModerationLayout() {
  const { pathname } = useLocation();
  const header = HEADERS[pathname];

  const { data: openReports } = useReports({
    offset: 0,
    count: 1,
    sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
    filters: { status: 'Open' },
  });

  return (
    <div>
      <TopNav />
      <div className="max-w-[1180px] mx-auto px-4 sm:px-7 py-6 sm:py-8">
        {header && (
          <div className="mb-4">
            <h1 className="text-title-sm sm:text-title font-bold tracking-tight m-0">{header.title}</h1>
            {header.subtitle && (
              <p className="text-[13px] sm:text-[13.5px] text-text-3 m-0 mt-1">{header.subtitle}</p>
            )}
          </div>
        )}
        <div className="md:hidden mb-4">
          <ModerationSidebar openReportsCount={openReports?.totalCount} variant="topbar" />
        </div>
        <div className="md:grid md:gap-5 md:grid-cols-[240px_1fr]">
          <div className="hidden md:block">
            <ModerationSidebar openReportsCount={openReports?.totalCount} />
          </div>
          <div>
            <Outlet />
          </div>
        </div>
      </div>
      <Footer />
    </div>
  );
}
