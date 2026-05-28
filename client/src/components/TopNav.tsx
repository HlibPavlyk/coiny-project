import { useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { Logo } from './Logo';
import { Icon } from './Icon';
import { LotImagePlaceholder } from './LotImagePlaceholder';
import { useCategoryTree, type CategoryNode } from '@/api/categories';
import { useSuggestLots, type LotSuggestItem } from '@/api/lots';
import { useAuthStore } from '@/state/useAuthStore';
import { formatKopiykasAsUah } from '@/lib/money';

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
  const canModerate = !!user?.roles.some((r) => r === 'Admin' || r === 'Moderator');

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

  // Typeahead state. `isOpen` toggles dropdown; `highlighted` tracks keyboard cursor. Reset on
  // outside-click and on Escape. The suggestions themselves are produced by a debounced hook.
  const [isOpen, setIsOpen] = useState(false);
  const [highlighted, setHighlighted] = useState(-1);
  const formRef = useRef<HTMLFormElement | null>(null);
  const { data: suggestions } = useSuggestLots(isOpen ? q : '');
  const items = suggestions ?? [];

  useEffect(() => {
    if (!isOpen) return;
    const onPointerDown = (e: PointerEvent) => {
      if (formRef.current && !formRef.current.contains(e.target as Node)) setIsOpen(false);
    };
    document.addEventListener('pointerdown', onPointerDown);
    return () => document.removeEventListener('pointerdown', onPointerDown);
  }, [isOpen]);

  // Close the dropdown on every route change — without this it would survive navigating to a hit.
  useEffect(() => {
    setIsOpen(false);
    setHighlighted(-1);
  }, [location.pathname, location.search]);

  // Whenever the suggestion list shape changes, snap the highlight back to "nothing selected" so
  // Enter falls through to the "See all results" path until the user explicitly arrows down.
  useEffect(() => setHighlighted(-1), [items.length]);

  const goToSearch = (term: string) => {
    const trimmed = term.trim();
    if (onSearch) {
      const next = new URLSearchParams(params);
      if (trimmed) next.set('q', trimmed);
      else next.delete('q');
      next.delete('offset');
      navigate({ pathname: '/search', search: next.toString() });
    } else {
      navigate(trimmed ? `/search?q=${encodeURIComponent(trimmed)}` : '/search');
    }
  };

  const submitSearch = (e: React.FormEvent) => {
    e.preventDefault();
    // If the user pressed Enter on a highlighted suggestion, navigate to its lot page; otherwise
    // run the full search like before.
    if (highlighted >= 0 && items[highlighted]) {
      navigate(`/lot/${items[highlighted].id}`);
    } else {
      goToSearch(q);
    }
    setIsOpen(false);
  };

  const onKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setIsOpen(false);
      setHighlighted(-1);
      return;
    }
    if (!isOpen || items.length === 0) return;
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setHighlighted((h) => (h + 1) % items.length);
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlighted((h) => (h <= 0 ? items.length - 1 : h - 1));
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
            <div key={root.id} className="relative group flex items-center">
              <Link to={`/search?category=${root.slug}`} className={navItemClass(rootActive(root))}>
                {root.name}
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
          <form
            ref={formRef}
            onSubmit={submitSearch}
            className="flex-1 max-w-[380px] ml-auto relative"
            role="search"
          >
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-text-3 pointer-events-none">
              <Icon name="search" size={16} />
            </div>
            <input
              type="search"
              value={q}
              onChange={(e) => {
                setQ(e.target.value);
                setIsOpen(true);
              }}
              onFocus={() => setIsOpen(true)}
              onKeyDown={onKeyDown}
              placeholder="Search lots…"
              aria-label="Search lots"
              aria-autocomplete="list"
              aria-controls="header-search-suggest"
              aria-expanded={isOpen && items.length > 0}
              autoComplete="off"
              className="w-full rounded-md py-2 pl-9 pr-3 text-sm border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
              style={{ borderColor: 'transparent' }}
            />

            {isOpen && q.trim().length >= 2 && (
              <SuggestDropdown
                id="header-search-suggest"
                items={items}
                highlighted={highlighted}
                onHover={setHighlighted}
                onPick={() => setIsOpen(false)}
                onSeeAll={() => {
                  goToSearch(q);
                  setIsOpen(false);
                }}
                query={q.trim()}
              />
            )}
          </form>
        )}

        <div className={`flex items-center gap-2 ${showSearch ? '' : 'ml-auto'}`}>
          {user ? (
            <>
              {canModerate && (
                <Link
                  to="/moderation"
                  aria-label="Moderation"
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md hover:bg-bg-soft transition no-underline text-[13.5px] font-medium text-text-2"
                >
                  <Icon name="shield" size={16} stroke={1.6} />
                  Moderation
                </Link>
              )}
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

interface SuggestDropdownProps {
  id: string;
  items: LotSuggestItem[];
  highlighted: number;
  query: string;
  onHover: (index: number) => void;
  onPick: () => void;
  onSeeAll: () => void;
}

/**
 * Typeahead dropdown rendered below the header search input. Mouse hover and keyboard arrows share
 * the same highlight state; <kbd>Enter</kbd> on a highlighted row navigates to that lot, while
 * <kbd>Enter</kbd> with nothing highlighted falls through to the form submit (full search page).
 */
function SuggestDropdown({ id, items, highlighted, query, onHover, onPick, onSeeAll }: SuggestDropdownProps) {
  return (
    <div
      id={id}
      role="listbox"
      className="absolute left-0 right-0 top-full mt-1.5 bg-surface border border-border rounded-lg z-50 overflow-hidden"
      style={{ boxShadow: 'var(--shadow-card-hover)' }}
    >
      {items.length === 0 ? (
        <div className="px-3.5 py-4 text-[13px] text-text-3">No matches for &ldquo;{query}&rdquo;.</div>
      ) : (
        <>
          <ul className="m-0 p-0 list-none max-h-[420px] overflow-y-auto">
            {items.map((item, idx) => {
              const active = idx === highlighted;
              return (
                <li key={item.id} role="option" aria-selected={active}>
                  <Link
                    to={`/lot/${item.id}`}
                    onClick={onPick}
                    onMouseEnter={() => onHover(idx)}
                    className="flex items-center gap-2.5 px-3 py-2 no-underline text-text"
                    style={{ background: active ? 'var(--color-bg-soft)' : 'transparent' }}
                  >
                    <div className="relative w-10 h-10 rounded bg-bg-soft overflow-hidden flex-shrink-0">
                      {item.coverImageUrl ? (
                        <img src={item.coverImageUrl} alt="" className="w-full h-full object-cover" />
                      ) : (
                        <LotImagePlaceholder kind="coin" variant={item.id.charCodeAt(0) % 6} />
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="text-[13px] font-medium text-text truncate">{item.title}</div>
                      <div className="text-[11.5px] text-text-3 truncate">{item.categoryPath}</div>
                    </div>
                    <div className="mono text-[12.5px] font-semibold text-text-2 flex-shrink-0 tabular-nums">
                      {formatKopiykasAsUah(item.currentPriceUahKopiykas)}
                    </div>
                  </Link>
                </li>
              );
            })}
          </ul>
          <button
            type="button"
            onMouseDown={(e) => {
              // Avoid the input's blur firing before navigation.
              e.preventDefault();
              onSeeAll();
            }}
            className="w-full text-left px-3.5 py-2.5 text-[12.5px] font-semibold text-accent-deep border-t border-border-soft hover:bg-bg-soft transition"
          >
            See all results for &ldquo;{query}&rdquo; →
          </button>
        </>
      )}
    </div>
  );
}
