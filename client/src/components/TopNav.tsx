import { useEffect, useRef, useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { Logo } from './Logo';
import { Icon } from './Icon';
import { LotImagePlaceholder } from './LotImagePlaceholder';
import { useCategoryTree, type CategoryNode } from '@/api/categories';
import { useSuggestLots, type LotSuggestItem } from '@/api/lots';
import { useAuthStore } from '@/state/useAuthStore';
import type { MeModel } from '@/api/auth';
import { formatKopiykasAsUah } from '@/lib/money';
import { useFocusTrap } from '@/lib/useFocusTrap';

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

  // Mobile drawer state. Closes on route change so the user lands on the new page with chrome reset.
  const [drawerOpen, setDrawerOpen] = useState(false);
  useEffect(() => setDrawerOpen(false), [location.pathname, location.search]);

  // Mobile search-panel state. Opens via the magnifier icon on mobile; the same `q` / `isOpen`
  // state powers the desktop inline search so suggestions look identical in both modes.
  const [mobileSearchOpen, setMobileSearchOpen] = useState(false);
  useEffect(() => setMobileSearchOpen(false), [location.pathname, location.search]);
  // Esc closes the panel.
  useEffect(() => {
    if (!mobileSearchOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMobileSearchOpen(false);
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [mobileSearchOpen]);

  return (
    <>
      <header
        className="sticky top-0 z-50 border-b border-border"
        style={{ background: 'rgba(250, 250, 247, 0.92)', backdropFilter: 'blur(10px)' }}
      >
        {/* Single-row header on all viewports. Mobile gets a search-icon button that opens a
            dropdown panel below the header (rendered just after `</header>` further down). */}
        <div className="max-w-[1280px] mx-auto px-4 sm:px-7 py-2.5 sm:py-3.5 flex items-center gap-3 md:gap-8">
          {/* Mobile hamburger (visible < md). */}
          <button
            type="button"
            onClick={() => setDrawerOpen(true)}
            aria-label="Open menu"
            className="md:hidden p-2 -ml-1 rounded-md hover:bg-bg-soft transition"
          >
            <Icon name="list" size={20} stroke={1.8} />
          </button>

          <Link to="/" className="no-underline flex-shrink-0">
            <Logo />
          </Link>

          {/* Categories nav — desktop only. Hover-dropdowns don't work on touch; mobile uses drawer. */}
          <nav className="hidden md:flex gap-1 ml-2">
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

          {/* Desktop inline search. Mobile gets an icon-button instead (see actions block below). */}
          {showSearch && (
            <form
              ref={formRef}
              onSubmit={submitSearch}
              className="hidden md:block flex-1 max-w-[380px] ml-auto relative"
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

          <div className={`flex items-center gap-1 sm:gap-2 ${showSearch ? 'ml-auto md:ml-0' : 'ml-auto'}`}>
            {/* Mobile-only search trigger. Tap → opens the search dropdown panel below the header. */}
            {showSearch && (
              <button
                type="button"
                onClick={() => setMobileSearchOpen((o) => !o)}
                aria-label="Search lots"
                aria-expanded={mobileSearchOpen}
                className="md:hidden p-2 rounded-md hover:bg-bg-soft transition"
              >
                <Icon name="search" size={20} stroke={1.8} />
              </button>
            )}
            {user ? (
              <>
                {canModerate && (
                  <Link
                    to="/moderation"
                    aria-label="Moderation"
                    className="hidden md:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md hover:bg-bg-soft transition no-underline text-[13.5px] font-medium text-text-2"
                  >
                    <Icon name="shield" size={16} stroke={1.6} />
                    Moderation
                  </Link>
                )}
                <Link
                  to="/my-bids"
                  className="hidden lg:inline-flex rounded-md hover:bg-bg-soft px-3 py-1.5 text-[13.5px] font-medium text-text-2 no-underline"
                >
                  My bids
                </Link>
                <Link
                  to="/lots/new"
                  className="inline-flex items-center gap-1.5 rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-2.5 sm:px-3 py-1.5 sm:py-1.5 text-[13.5px] no-underline transition"
                  aria-label="Create lot"
                >
                  <Icon name="plus" size={14} stroke={2} color="#fff" />
                  <span className="hidden sm:inline">Create lot</span>
                </Link>
                <Link
                  to="/profile"
                  className="ml-1 w-9 h-9 sm:w-8 sm:h-8 rounded-full text-white text-xs font-semibold flex items-center justify-center no-underline flex-shrink-0"
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
                  className="rounded-md hover:bg-bg-soft px-2.5 sm:px-3 py-1.5 text-[13.5px] font-medium text-text-2 no-underline whitespace-nowrap"
                >
                  Sign in
                </Link>
                <Link
                  to="/sign-up"
                  className="hidden sm:inline-block rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3 py-1.5 text-[13.5px] no-underline transition whitespace-nowrap"
                >
                  Create account
                </Link>
              </>
            )}
          </div>
        </div>
      </header>

      {/* Mobile search dropdown panel — appears below the header sticky on top of page content.
          Tap-outside or Esc closes. The same `formRef` / `q` / suggest state powers it as the
          desktop inline search, so suggestions look identical. */}
      {showSearch && mobileSearchOpen && (
        <MobileSearchPanel
          formRef={formRef}
          q={q}
          setQ={setQ}
          isOpen={isOpen}
          setIsOpen={setIsOpen}
          items={items}
          highlighted={highlighted}
          setHighlighted={setHighlighted}
          onKeyDown={onKeyDown}
          submitSearch={submitSearch}
          goToSearch={goToSearch}
          onClose={() => setMobileSearchOpen(false)}
        />
      )}

      {/* Mobile slide-out drawer. Closed by default; opens from the left when hamburger is tapped. */}
      <MobileDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        tree={tree}
        user={user}
        canModerate={canModerate}
      />
    </>
  );
}

interface MobileDrawerProps {
  open: boolean;
  onClose: () => void;
  tree: ReturnType<typeof useCategoryTree>['data'];
  user: MeModel | null;
  canModerate: boolean;
}

/**
 * Off-canvas mobile drawer with the navigation that doesn't fit in the compact header. Backdrop and
 * Escape close. Each link closes the drawer via the route-change effect in TopNav.
 */
function MobileDrawer({ open, onClose, tree, user, canModerate }: MobileDrawerProps) {
  const signOut = useAuthStore((s) => s.signOut);
  const navigate = useNavigate();
  const drawerRef = useRef<HTMLDivElement>(null);
  useFocusTrap(drawerRef, open);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  // Lock body scroll while drawer is open so the page behind doesn't scroll under it.
  useEffect(() => {
    if (!open) return;
    const original = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = original;
    };
  }, [open]);

  if (!open) return null;

  const linkCls = 'block px-3 py-3 rounded-md text-[15px] font-medium text-text-2 hover:bg-bg-soft no-underline transition';
  const subLinkCls = 'block px-3 py-2 rounded-md text-[14px] text-text-3 hover:bg-bg-soft hover:text-text no-underline transition';

  return (
    <div className="md:hidden fixed inset-0 z-[100]" role="dialog" aria-modal="true" aria-label="Navigation menu">
      <div
        className="absolute inset-0"
        style={{ background: 'rgba(15, 23, 42, 0.45)' }}
        onClick={onClose}
      />
      <div
        ref={drawerRef}
        className="absolute left-0 top-0 bottom-0 w-[85vw] max-w-[340px] bg-surface flex flex-col overflow-y-auto"
        style={{ boxShadow: '0 10px 40px rgba(15, 12, 8, 0.18)' }}
      >
        <div className="flex items-center justify-between px-4 py-3 border-b border-border">
          <Logo />
          <button
            type="button"
            onClick={onClose}
            aria-label="Close menu"
            className="p-2 -mr-1 rounded-md hover:bg-bg-soft transition"
          >
            <Icon name="x" size={20} stroke={1.8} />
          </button>
        </div>

        <nav className="px-3 py-3 flex-1">
          <Link to="/" className={linkCls}>
            Home
          </Link>
          {tree?.roots.map((root) => (
            <div key={root.id} className="mb-1">
              <Link to={`/search?category=${root.slug}`} className={linkCls}>
                {root.name}
              </Link>
              {root.children.map((child) => (
                <Link key={child.id} to={`/search?category=${child.slug}`} className={`pl-7 ${subLinkCls}`}>
                  {child.name}
                </Link>
              ))}
            </div>
          ))}

          {user && (
            <>
              <div className="h-px bg-border-soft my-3" />
              <Link to="/my-bids" className={linkCls}>
                My bids
              </Link>
              <Link to="/my-lots" className={linkCls}>
                My lots
              </Link>
              <Link to="/my-purchases" className={linkCls}>
                My purchases
              </Link>
              {canModerate && (
                <Link to="/moderation" className={`${linkCls} flex items-center gap-2`}>
                  <Icon name="shield" size={14} />
                  Moderation
                </Link>
              )}
            </>
          )}
        </nav>

        <div className="px-3 py-3 border-t border-border">
          {user ? (
            <button
              type="button"
              onClick={async () => {
                await signOut();
                navigate('/', { replace: true });
                onClose();
              }}
              className="w-full flex items-center justify-center gap-2 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-3 text-[14px]"
            >
              <Icon name="arrowL" size={14} />
              Sign out
            </button>
          ) : (
            <div className="flex flex-col gap-2">
              <Link
                to="/sign-up"
                className="rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-3 text-[14px] no-underline text-center"
              >
                Create account
              </Link>
              <Link
                to="/sign-in"
                className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-3 text-[14px] no-underline text-center"
              >
                Sign in
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
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
                    className="flex items-center gap-2.5 px-3.5 py-2.5 no-underline text-text"
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

interface MobileSearchPanelProps {
  formRef: React.RefObject<HTMLFormElement | null>;
  q: string;
  setQ: (v: string) => void;
  isOpen: boolean;
  setIsOpen: (v: boolean) => void;
  items: LotSuggestItem[];
  highlighted: number;
  setHighlighted: (i: number) => void;
  onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  submitSearch: (e: React.FormEvent) => void;
  goToSearch: (term: string) => void;
  onClose: () => void;
}

/**
 * Mobile search panel rendered below the header. Backdrop is semi-transparent so the user still
 * sees that this is part of the page (not a separate screen). Input gets focus on mount.
 */
function MobileSearchPanel({
  formRef,
  q,
  setQ,
  isOpen,
  setIsOpen,
  items,
  highlighted,
  setHighlighted,
  onKeyDown,
  submitSearch,
  goToSearch,
  onClose,
}: MobileSearchPanelProps) {
  const inputRef = useRef<HTMLInputElement | null>(null);
  useEffect(() => {
    inputRef.current?.focus();
    setIsOpen(true);
  }, [setIsOpen]);

  return (
    <div className="md:hidden fixed inset-0 z-40 pointer-events-none" role="dialog" aria-modal="false" aria-label="Search">
      {/* Backdrop — covers below the header (top:56px ≈ header height). pointer-events on so outside-tap closes. */}
      <button
        type="button"
        aria-label="Close search"
        onClick={onClose}
        className="absolute inset-x-0 top-[56px] bottom-0 pointer-events-auto"
        style={{ background: 'rgba(15, 23, 42, 0.45)' }}
      />
      {/* Panel itself — floats below the header as a rounded card with margins from the screen
          edges (not flush). Looks like a dropdown, not a full-width banner. */}
      <div
        className="absolute left-3 right-3 top-[64px] bg-surface border border-border rounded-lg pointer-events-auto"
        style={{ boxShadow: '0 8px 24px rgba(15, 12, 8, 0.12), 0 2px 6px rgba(15, 12, 8, 0.06)' }}
      >
        <form
          ref={formRef}
          onSubmit={(e) => {
            submitSearch(e);
            onClose();
          }}
          className="p-3 relative"
          role="search"
        >
          <div className="relative">
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-text-3 pointer-events-none">
              <Icon name="search" size={16} />
            </div>
            <input
              ref={inputRef}
              type="search"
              value={q}
              onChange={(e) => {
                setQ(e.target.value);
                setIsOpen(true);
              }}
              onKeyDown={onKeyDown}
              placeholder="Search lots…"
              aria-label="Search lots"
              autoComplete="off"
              className="w-full rounded-md py-2.5 pl-9 pr-10 text-sm border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
              style={{ borderColor: 'transparent' }}
            />
            <button
              type="button"
              onClick={() => {
                if (q) setQ('');
                else onClose();
              }}
              aria-label={q ? 'Clear' : 'Close'}
              className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 rounded-md hover:bg-bg-soft text-text-3"
            >
              <Icon name="x" size={14} stroke={2} />
            </button>
          </div>

          {isOpen && q.trim().length >= 2 && (
            <div className="mt-2 -mx-0">
              <SuggestDropdown
                id="mobile-search-suggest"
                items={items}
                highlighted={highlighted}
                onHover={setHighlighted}
                onPick={onClose}
                onSeeAll={() => {
                  goToSearch(q);
                  onClose();
                }}
                query={q.trim()}
              />
            </div>
          )}
        </form>
      </div>
    </div>
  );
}
