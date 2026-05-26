import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { Logo } from './Logo';
import { Icon } from './Icon';
import { useCategoryTree, type CategoryNode } from '@/api/categories';
import { useAuthStore } from '@/state/useAuthStore';

interface TopNavProps {
  showSearch?: boolean;
}

/** Does this subtree contain the given category slug? Used to highlight the active top-level nav item. */
function subtreeHasSlug(node: CategoryNode, slug: string): boolean {
  return node.slug === slug || node.children.some((child) => subtreeHasSlug(child, slug));
}

export function TopNav({ showSearch = true }: TopNavProps) {
  const user = useAuthStore((s) => s.user);
  const initials = user ? (user.displayName || user.email).slice(0, 2).toUpperCase() : '';

  const location = useLocation();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const { data: tree } = useCategoryTree();
  const onSearch = location.pathname === '/search';
  const currentCategory = onSearch ? params.get('category') : null;

  const rootActive = (root: CategoryNode) =>
    onSearch && currentCategory != null && subtreeHasSlug(root, currentCategory);

  // Header search is the single global entry point. It mirrors the active query when on /search and,
  // when already there, preserves the other filters; otherwise it starts a fresh search.
  const [q, setQ] = useState('');
  useEffect(() => {
    setQ(onSearch ? (params.get('q') ?? '') : '');
  }, [onSearch, params]);

  const submitSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const term = q.trim();
    if (onSearch) {
      const next = new URLSearchParams(params);
      if (term) next.set('q', term);
      else next.delete('q');
      next.delete('offset');
      navigate({ pathname: '/search', search: next.toString() });
    } else {
      navigate(term ? `/search?q=${encodeURIComponent(term)}` : '/search');
    }
  };

  const navItemClass = (active: boolean) =>
    `px-3 py-1.5 rounded-md text-[13.5px] font-medium no-underline transition ${
      active ? 'text-text bg-bg-soft' : 'text-text-2 hover:bg-bg-soft'
    }`;

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
          <Link to="/" className={navItemClass(location.pathname === '/')}>
            Home
          </Link>
          {tree?.roots.map((root) => (
            <div key={root.id} className="relative group">
              <Link to={`/search?category=${root.slug}`} className={`${navItemClass(rootActive(root))} flex items-center gap-1`}>
                {root.name}
                {root.children.length > 0 && (
                  <span className="inline-block rotate-90 opacity-60">
                    <Icon name="arrowR" size={10} />
                  </span>
                )}
              </Link>
              {root.children.length > 0 && (
                <div className="absolute left-0 top-full pt-1.5 hidden group-hover:block z-50 min-w-[210px]">
                  <div
                    className="bg-surface border border-border rounded-lg py-2"
                    style={{ boxShadow: 'var(--shadow-card-hover)' }}
                  >
                    {root.children.map((child) => (
                      <div key={child.id}>
                        <Link
                          to={`/search?category=${child.slug}`}
                          className="block px-3.5 py-1.5 text-[13px] font-medium text-text-2 hover:text-text hover:bg-bg-soft no-underline"
                        >
                          {child.name}
                        </Link>
                        {child.children.map((grandchild) => (
                          <Link
                            key={grandchild.id}
                            to={`/search?category=${grandchild.slug}`}
                            className="block pl-7 pr-3.5 py-1 text-[12.5px] text-text-3 hover:text-text hover:bg-bg-soft no-underline"
                          >
                            {grandchild.name}
                          </Link>
                        ))}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </nav>

        {showSearch && (
          <form onSubmit={submitSearch} className="flex-1 max-w-[380px] ml-auto relative">
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-text-3 pointer-events-none">
              <Icon name="search" size={16} />
            </div>
            <input
              type="search"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Search lots…"
              aria-label="Search lots"
              className="w-full rounded-md py-2 pl-9 pr-3 text-sm border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
              style={{ borderColor: 'transparent' }}
            />
          </form>
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
