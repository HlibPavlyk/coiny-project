import { Link, useLocation } from 'react-router-dom';
import { Icon, type IconName } from '@/components/Icon';
import { useAuthStore } from '@/state/useAuthStore';

interface NavItem {
  id: string;
  label: string;
  icon: IconName;
  to: string;
  external?: boolean;
  requireRoles?: string[];
  count?: number;
  /** Explicit marker for "section start" — renders a divider above this item. Used to group the
   *  admin-only Demo + Background-jobs items as one "tools" section. */
  sectionStart?: boolean;
}

// Hangfire is reverse-proxied via Vite in dev and same-origin in prod, so a relative URL works in both.
const HANGFIRE_URL = '/hangfire';

const items: NavItem[] = [
  { id: 'overview', label: 'Overview', icon: 'shield', to: '/moderation' },
  { id: 'reports', label: 'Reports', icon: 'info', to: '/moderation/reports' },
  { id: 'users', label: 'Users', icon: 'user', to: '/moderation/users' },
  { id: 'lots', label: 'Lots', icon: 'cards', to: '/moderation/lots' },
  // Admin-only "tools" group — Demo and Background jobs sit together under a single divider so
  // moderators visually understand the surface above is everyday moderation and below is admin
  // utilities. Demo's own page + the server endpoints both re-check Admin role, so the sidebar
  // visibility is the third layer of guard (defence in depth).
  { id: 'demo', label: 'Demo', icon: 'clock', to: '/moderation/demo', requireRoles: ['Admin'], sectionStart: true },
  { id: 'jobs', label: 'Background jobs', icon: 'external', to: HANGFIRE_URL, external: true, requireRoles: ['Admin'] },
];

interface ModerationSidebarProps {
  active?: string;
  openReportsCount?: number;
  variant?: 'sidebar' | 'topbar';
}

export function ModerationSidebar({ active, openReportsCount, variant = 'sidebar' }: ModerationSidebarProps) {
  const roles = useAuthStore((s) => s.user?.roles ?? []);
  const visibleItems = items.filter((it) => !it.requireRoles || it.requireRoles.some((r) => roles.includes(r)));

  if (variant === 'topbar') return <TopbarVariant items={visibleItems} openReportsCount={openReportsCount} />;
  return <SidebarVariant items={visibleItems} active={active} openReportsCount={openReportsCount} />;
}

function SidebarVariant({
  items,
  active,
  openReportsCount,
}: {
  items: NavItem[];
  active?: string;
  openReportsCount?: number;
}) {
  const location = useLocation();

  return (
    <aside className="bg-surface border border-border rounded-lg p-2 self-start">
      <div className="px-3.5 pt-3.5 pb-2.5 mb-1.5 border-b border-border-soft">
        <div className="text-[11px] font-semibold uppercase tracking-wider text-text-3">Moderation</div>
      </div>

      {items.map((it) => {
        const isActive = !it.external && (active === it.id || location.pathname === it.to);
        const count = it.id === 'reports' ? openReportsCount : it.count;
        // Divider only on `sectionStart`. The legacy "external after internal" heuristic was
        // emitting a second divider before Background jobs (since Demo before it is internal),
        // splitting the Demo+Jobs admin-tools group into two visually. sectionStart on Demo alone
        // is enough to mark the start of that section; Jobs flows continuously after it.
        const needsDivider = !!it.sectionStart;

        const labelMarkup = (
          <>
            <Icon name={it.icon} size={16} color={isActive ? 'var(--color-accent-deep)' : 'var(--color-text-3)'} />
            <span className="flex-1">{it.label}</span>
            {count !== undefined && <CountPill value={count} active={isActive} />}
          </>
        );

        const cls = 'flex items-center gap-2.5 px-3 py-2.5 rounded-md text-sm font-medium mb-0.5 transition no-underline';
        const style = {
          color: isActive ? 'var(--color-text)' : 'var(--color-text-2)',
          background: isActive ? 'var(--color-bg-soft)' : 'transparent',
        } as const;

        return (
          <div key={it.id}>
            {needsDivider && <div className="h-px bg-border-soft mx-1 my-2" />}
            {it.external ? (
              <a href={it.to} target="_blank" rel="noreferrer" className={cls} style={style}>
                {labelMarkup}
              </a>
            ) : (
              <Link to={it.to} className={cls} style={style}>
                {labelMarkup}
              </Link>
            )}
          </div>
        );
      })}
    </aside>
  );
}

function TopbarVariant({ items, openReportsCount }: { items: NavItem[]; openReportsCount?: number }) {
  const location = useLocation();
  return (
    <nav
      aria-label="Moderation sections"
      className="bg-surface border border-border rounded-lg p-1 overflow-x-auto"
      style={{ scrollbarWidth: 'none' }}
    >
      <div className="flex gap-1 min-w-max">
        {items.map((it) => {
          const isActive = !it.external && location.pathname === it.to;
          const count = it.id === 'reports' ? openReportsCount : it.count;
          const cls = 'flex items-center gap-2 px-3 py-2.5 rounded-md text-[13px] font-medium whitespace-nowrap transition no-underline';
          const style = {
            color: isActive ? 'var(--color-text)' : 'var(--color-text-2)',
            background: isActive ? 'var(--color-bg-soft)' : 'transparent',
          } as const;
          const content = (
            <>
              <Icon name={it.icon} size={14} color={isActive ? 'var(--color-accent-deep)' : 'var(--color-text-3)'} />
              {it.label}
              {count !== undefined && <CountPill value={count} active={isActive} />}
            </>
          );
          return it.external ? (
            <a key={it.id} href={it.to} target="_blank" rel="noreferrer" className={cls} style={style}>
              {content}
            </a>
          ) : (
            <Link key={it.id} to={it.to} className={cls} style={style}>
              {content}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}

function CountPill({ value, active }: { value: number; active: boolean }) {
  return (
    <span
      className="font-semibold rounded-full"
      style={{
        fontSize: 11,
        padding: '2px 7px',
        background: active ? 'var(--color-accent-tint)' : 'var(--color-bg-soft)',
        color: active ? 'var(--color-accent-deep)' : 'var(--color-text-3)',
      }}
    >
      {value}
    </span>
  );
}
