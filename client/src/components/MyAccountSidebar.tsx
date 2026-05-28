import { Link, useLocation } from 'react-router-dom';
import { Icon, type IconName } from '@/components/Icon';
import { useAuthStore } from '@/state/useAuthStore';
import { useNavigate } from 'react-router-dom';

interface NavItem {
  id: string;
  label: string;
  icon: IconName;
  to: string;
  count?: number;
}

const items: NavItem[] = [
  { id: 'profile', label: 'Profile', icon: 'user', to: '/profile' },
  { id: 'lots', label: 'My lots', icon: 'cards', to: '/my-lots' },
  { id: 'bids', label: 'My bids', icon: 'bid', to: '/my-bids' },
  { id: 'purchases', label: 'My purchases', icon: 'package', to: '/my-purchases' },
];

interface MyAccountSidebarProps {
  active?: string;
  /** Per-section totals — rendered as a badge on the matching item when ≥ 1. */
  counts?: Partial<Record<'lots' | 'bids' | 'purchases', number>>;
}

export function MyAccountSidebar({ active, counts }: MyAccountSidebarProps) {
  const location = useLocation();
  const signOut = useAuthStore((s) => s.signOut);
  const navigate = useNavigate();

  const onSignOut = async () => {
    await signOut();
    navigate('/', { replace: true });
  };

  return (
    <aside className="bg-surface border border-border rounded-lg p-2 self-start">
      <div className="px-3.5 pt-3.5 pb-2.5 mb-1.5 border-b border-border-soft">
        <div className="text-[11px] font-semibold uppercase tracking-wider text-text-3">
          My account
        </div>
      </div>

      {items.map((it) => {
        const isActive = active === it.id || location.pathname === it.to;
        const dyn = counts?.[it.id as 'lots' | 'bids' | 'purchases'];
        const count = dyn !== undefined && dyn > 0 ? dyn : it.count;
        return (
          <Link
            key={it.id}
            to={it.to}
            className="flex items-center gap-2.5 px-3 py-2.5 rounded-md text-sm font-medium mb-0.5 transition no-underline"
            style={{
              color: isActive ? 'var(--color-text)' : 'var(--color-text-2)',
              background: isActive ? 'var(--color-bg-soft)' : 'transparent',
            }}
          >
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
          </Link>
        );
      })}

      <div className="h-px bg-border-soft mx-1 my-2" />

      <button
        type="button"
        onClick={onSignOut}
        className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-md text-sm font-medium text-text-2 hover:bg-bg-soft transition"
      >
        <Icon name="arrowL" size={16} color="var(--color-text-3)" />
        Sign out
      </button>
    </aside>
  );
}
