import { Link, useLocation } from 'react-router-dom';
import { Icon, type IconName } from '@/components/Icon';
import { useAuthStore } from '@/state/useAuthStore';

interface NavItem {
  id: string;
  label: string;
  icon: IconName;
  to: string;
  /** Render as external (new tab) instead of in-app navigation. */
  external?: boolean;
  /** If set, hide unless user has one of these roles. */
  requireRoles?: string[];
  count?: number;
}

// Hangfire is reverse-proxied via Vite (/hangfire → http://localhost:5000/hangfire) in dev and lives
// at the same origin in prod (Caddy in front of the API), so a relative URL works in both.
const HANGFIRE_URL = '/hangfire';

const items: NavItem[] = [
  { id: 'overview', label: 'Overview', icon: 'shield', to: '/moderation' },
  { id: 'reports', label: 'Reports', icon: 'info', to: '/moderation/reports' },
  { id: 'users', label: 'Users', icon: 'user', to: '/moderation/users' },
  { id: 'lots', label: 'Lots', icon: 'cards', to: '/moderation/lots' },
  { id: 'jobs', label: 'Background jobs', icon: 'external', to: HANGFIRE_URL, external: true, requireRoles: ['Admin'] },
];

interface ModerationSidebarProps {
  active?: string;
  /** Optional badge text rendered next to Reports (e.g. open count). */
  openReportsCount?: number;
}

export function ModerationSidebar({ active, openReportsCount }: ModerationSidebarProps) {
  const location = useLocation();
  const roles = useAuthStore((s) => s.user?.roles ?? []);

  const visibleItems = items.filter((it) => !it.requireRoles || it.requireRoles.some((r) => roles.includes(r)));
  const reportsIdx = visibleItems.findIndex((it) => it.id === 'reports');

  return (
    <aside className="bg-surface border border-border rounded-lg p-2 self-start">
      <div className="px-3.5 pt-3.5 pb-2.5 mb-1.5 border-b border-border-soft">
        <div className="text-[11px] font-semibold uppercase tracking-wider text-text-3">
          Moderation
        </div>
      </div>

      {visibleItems.map((it, idx) => {
        const isActive =
          !it.external && (active === it.id || location.pathname === it.to);
        const count = it.id === 'reports' ? openReportsCount : it.count;

        // Visual divider before the external "Background jobs" link.
        const prevWasInternal = idx > 0 && !visibleItems[idx - 1].external;
        const needsDivider = it.external && prevWasInternal && idx !== reportsIdx;

        const labelMarkup = (
          <>
            <Icon
              name={it.icon}
              size={16}
              color={isActive ? 'var(--color-accent-deep)' : 'var(--color-text-3)'}
            />
            <span className="flex-1">{it.label}</span>
            {count !== undefined && (
              <span
                className="font-semibold rounded-full"
                style={{
                  fontSize: 11,
                  padding: '2px 7px',
                  background: isActive ? 'var(--color-accent-tint)' : 'var(--color-bg-soft)',
                  color: isActive ? 'var(--color-accent-deep)' : 'var(--color-text-3)',
                }}
              >
                {count}
              </span>
            )}
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
