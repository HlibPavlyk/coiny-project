import { Link, NavLink } from 'react-router-dom';
import { Logo } from './Logo';
import { Icon } from './Icon';
import { useAuthStore } from '@/state/useAuthStore';

const navItems = [
  { id: 'home', label: 'Home', to: '/' },
  { id: 'coins', label: 'Coins', to: '/category/coins' },
  { id: 'banknotes', label: 'Banknotes', to: '/category/banknotes' },
  { id: 'medals', label: 'Medals & orders', to: '/category/medals' },
];

interface TopNavProps {
  active?: string;
  showSearch?: boolean;
}

export function TopNav({ showSearch = true }: TopNavProps) {
  const user = useAuthStore((s) => s.user);
  const initials = user ? (user.displayName || user.email).slice(0, 2).toUpperCase() : '';

  return (
    <header
      className="sticky top-0 z-50 border-b border-border"
      style={{ background: 'rgba(250, 250, 247, 0.92)', backdropFilter: 'blur(10px)' }}
    >
      <div className="max-w-[1280px] mx-auto px-7 py-3.5 flex items-center gap-8">
        <Link to="/" className="no-underline">
          <Logo />
        </Link>
        <nav className="flex gap-1 ml-2">
          {navItems.map((it) => (
            <NavLink
              key={it.id}
              to={it.to}
              end={it.to === '/'}
              className={({ isActive }) =>
                `px-3 py-1.5 rounded-md text-[13.5px] font-medium no-underline transition ${
                  isActive ? 'text-text bg-bg-soft' : 'text-text-2 hover:bg-bg-soft'
                }`
              }
            >
              {it.label}
            </NavLink>
          ))}
        </nav>

        {showSearch && (
          <div className="flex-1 max-w-[380px] ml-auto relative">
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-text-3">
              <Icon name="search" size={16} />
            </div>
            <input
              type="search"
              placeholder="Search lots…"
              className="w-full rounded-md py-2 pl-9 pr-3 text-sm border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
              style={{ borderColor: 'transparent' }}
            />
          </div>
        )}

        <div className={`flex items-center gap-2 ${showSearch ? '' : 'ml-auto'}`}>
          {user ? (
            <>
              <button
                type="button"
                aria-label="Notifications"
                className="relative p-2 rounded-md hover:bg-bg-soft transition"
              >
                <Icon name="bell" size={18} stroke={1.6} />
                <span
                  className="absolute top-1.5 right-1.5 w-1.5 h-1.5 rounded-full"
                  style={{ background: 'var(--color-accent)', border: '1.5px solid var(--color-bg)' }}
                />
              </button>
              <Link
                to="/my-bids"
                className="rounded-md hover:bg-bg-soft px-3 py-1.5 text-[13.5px] font-medium text-text-2 no-underline"
              >
                My bids
              </Link>
              <Link
                to="/lots/new"
                className="inline-flex items-center gap-1.5 rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3 py-1.5 text-[13.5px] no-underline transition"
              >
                <Icon name="plus" size={14} stroke={2} color="#fff" />
                Create lot
              </Link>
              <Link
                to="/profile"
                className="ml-1 w-8 h-8 rounded-full text-white text-xs font-semibold flex items-center justify-center no-underline"
                style={{ background: 'linear-gradient(135deg, #C8B380, #8A6A2A)' }}
                aria-label="My profile"
              >
                {initials}
              </Link>
            </>
          ) : (
            <>
              <Link
                to="/sign-in"
                className="rounded-md hover:bg-bg-soft px-3 py-1.5 text-[13.5px] font-medium text-text-2 no-underline"
              >
                Sign in
              </Link>
              <Link
                to="/sign-up"
                className="rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3 py-1.5 text-[13.5px] no-underline transition"
              >
                Create account
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
