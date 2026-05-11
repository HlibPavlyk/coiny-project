import { useState, useMemo } from 'react';
import { Link, useParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotCard } from '@/components/LotCard';
import { Icon } from '@/components/Icon';
import { useCategoryTree, findCategoryBySlug, type CategoryNode } from '@/api/categories';
import { useLotsByCategory } from '@/api/lots';

const PAGE_SIZE = 12;

type SortKey = 'endsAt-asc' | 'createdAt-desc' | 'price-asc' | 'price-desc';

const sortOptions: { value: SortKey; label: string; sortBy: { columnName: string; direction: 'Asc' | 'Desc' } }[] = [
  { value: 'endsAt-asc', label: 'Ending soon', sortBy: { columnName: 'endsAt', direction: 'Asc' } },
  { value: 'createdAt-desc', label: 'Newest', sortBy: { columnName: 'createdAt', direction: 'Desc' } },
  { value: 'price-asc', label: 'Price ↑', sortBy: { columnName: 'currentPriceUahKopiykas', direction: 'Asc' } },
  { value: 'price-desc', label: 'Price ↓', sortBy: { columnName: 'currentPriceUahKopiykas', direction: 'Desc' } },
];

function buildBreadcrumb(node: CategoryNode, allNodes: CategoryNode[]): CategoryNode[] {
  const byId = new Map<number, CategoryNode>();
  function index(n: CategoryNode) {
    byId.set(n.id, n);
    n.children.forEach(index);
  }
  allNodes.forEach(index);

  const path: CategoryNode[] = [];
  let cursor: CategoryNode | undefined = node;
  while (cursor) {
    path.unshift(cursor);
    const parent = Array.from(byId.values()).find((c) => c.children.some((ch) => ch.id === cursor!.id));
    cursor = parent;
  }
  return path;
}

export default function CategoryPage() {
  const { slug = '' } = useParams<{ slug: string }>();
  const { data: tree, isLoading: treeLoading } = useCategoryTree();
  const node = findCategoryBySlug(tree, slug);

  const [page, setPage] = useState(0);
  const [sort, setSort] = useState<SortKey>('endsAt-asc');

  const sortBy = sortOptions.find((o) => o.value === sort)!.sortBy;

  const { data, isLoading } = useLotsByCategory(node?.id, {
    offset: page * PAGE_SIZE,
    count: PAGE_SIZE,
    sortBy: [sortBy],
  });

  const breadcrumb = useMemo(
    () => (node && tree ? buildBreadcrumb(node, tree.roots) : []),
    [node, tree],
  );

  if (treeLoading) {
    return (
      <div>
        <TopNav />
        <div className="max-w-[1280px] mx-auto px-7 py-10 text-text-3">Loading…</div>
      </div>
    );
  }

  if (!node) {
    return (
      <div>
        <TopNav />
        <div className="max-w-[1280px] mx-auto px-7 py-16 text-center">
          <h1 className="text-2xl font-bold mb-2">Category not found</h1>
          <p className="text-text-3 mb-6">The slug &ldquo;{slug}&rdquo; doesn&apos;t match any category.</p>
          <Link
            to="/"
            className="inline-block rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-2.5 text-sm no-underline"
          >
            Back to home
          </Link>
        </div>
        <Footer />
      </div>
    );
  }

  const total = data?.totalCount ?? 0;
  const pageCount = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div>
      <TopNav />

      {/* Breadcrumb */}
      <div className="max-w-[1280px] mx-auto px-7 pt-5 pb-2">
        <nav className="flex items-center gap-2 text-[12.5px] text-text-3" aria-label="Breadcrumb">
          <Link to="/" className="hover:text-text-2 no-underline">
            Home
          </Link>
          {breadcrumb.map((b, i) => (
            <span key={b.id} className="flex items-center gap-2">
              <Icon name="arrowR" size={11} color="var(--color-text-3)" />
              {i === breadcrumb.length - 1 ? (
                <span className="text-text-2">{b.name}</span>
              ) : (
                <Link to={`/category/${b.slug}`} className="hover:text-text-2 no-underline">
                  {b.name}
                </Link>
              )}
            </span>
          ))}
        </nav>
      </div>

      {/* Header */}
      <div className="max-w-[1280px] mx-auto px-7 pt-3">
        <h1 className="text-[28px] font-bold m-0">{node.name}</h1>
        <p className="text-sm text-text-3 mt-1.5 m-0">
          <span className="mono font-semibold text-text">{total}</span> active{' '}
          {total === 1 ? 'lot' : 'lots'}
        </p>
      </div>

      {/* Layout: sidebar + results */}
      <div
        className="max-w-[1280px] mx-auto px-7 pt-6 grid gap-8"
        style={{ gridTemplateColumns: '240px 1fr' }}
      >
        <aside>
          <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">
            Browse
          </div>
          {node.children.length > 0 ? (
            <ul className="space-y-1.5">
              {node.children.map((child) => (
                <li key={child.id}>
                  <Link
                    to={`/category/${child.slug}`}
                    className="flex items-center justify-between text-sm py-1.5 text-text-2 hover:text-text no-underline"
                  >
                    <span>{child.name}</span>
                    <span className="mono text-xs text-text-3">{child.lotCountActive}</span>
                  </Link>
                </li>
              ))}
            </ul>
          ) : (
            <div className="text-sm text-text-3">No subcategories.</div>
          )}
        </aside>

        <div>
          <div className="flex items-center justify-end mb-4 gap-2">
            <label htmlFor="sort" className="text-[13px] text-text-3">
              Sort:
            </label>
            <select
              id="sort"
              value={sort}
              onChange={(e) => {
                setSort(e.target.value as SortKey);
                setPage(0);
              }}
              className="rounded-md border border-border-strong bg-surface text-sm py-1.5 px-2.5"
            >
              {sortOptions.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>

          {isLoading ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
              {Array.from({ length: PAGE_SIZE }).map((_, i) => (
                <div
                  key={i}
                  className="bg-bg-soft border border-border rounded-lg"
                  style={{ aspectRatio: '0.78' }}
                />
              ))}
            </div>
          ) : data && data.items.length > 0 ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                {data.items.map((lot) => (
                  <LotCard key={lot.id} lot={lot} />
                ))}
              </div>

              {pageCount > 1 && (
                <div className="flex justify-center items-center gap-1.5 mt-9">
                  <button
                    type="button"
                    disabled={page === 0}
                    onClick={() => setPage((p) => Math.max(0, p - 1))}
                    className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Icon name="arrowL" size={14} />
                  </button>
                  <span className="text-sm text-text-3 mx-3">
                    Page <span className="mono font-semibold text-text">{page + 1}</span> of{' '}
                    <span className="mono">{pageCount}</span>
                  </span>
                  <button
                    type="button"
                    disabled={page >= pageCount - 1}
                    onClick={() => setPage((p) => Math.min(pageCount - 1, p + 1))}
                    className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Icon name="arrowR" size={14} />
                  </button>
                </div>
              )}
            </>
          ) : (
            <div className="bg-surface border border-dashed border-border rounded-lg py-12 text-center">
              <p className="text-text-3">No active lots in this category yet.</p>
            </div>
          )}
        </div>
      </div>

      <Footer />
    </div>
  );
}
