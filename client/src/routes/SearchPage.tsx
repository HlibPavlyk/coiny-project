import { useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotCard } from '@/components/LotCard';
import { Icon } from '@/components/Icon';
import { FacetGroup } from '@/components/FacetGroup';
import { PriceRangeFilter } from '@/components/PriceRangeFilter';
import { CategoryFilter } from '@/components/CategoryFilter';
import { useCategoryTree, findCategoryBySlug } from '@/api/categories';
import {
  useSearchLots,
  type LotStatus,
  type SearchLotsFilters,
  type SearchLotsRequest,
  type SortByModel,
} from '@/api/lots';

const PAGE_SIZE = 12;

// "Ending within" presets → hours from now; the API filter `endingBefore` is computed at query time.
const ENDS_IN_HOURS: Record<string, number> = { '24h': 24, '3d': 72, '7d': 168 };

// Public search only ever shows published lots; the toggle narrows to one of them, or shows both.
const STATUS_OPTIONS: { value: string; label: string; status?: LotStatus }[] = [
  { value: 'all', label: 'All' },
  { value: 'Active', label: 'Active', status: 'Active' },
  { value: 'Sold', label: 'Sold', status: 'Sold' },
];

type SortKey = 'relevance' | 'endsAt' | 'newest' | 'priceAsc' | 'priceDesc';

// Sortable columns are the response model's [Sortable] fields. Relevance is only offered when q != ''.
const SORT_TO_API: Record<SortKey, SortByModel[]> = {
  relevance: [],
  endsAt: [{ columnName: 'endsAt', direction: 'Asc' }],
  newest: [{ columnName: 'createdAt', direction: 'Desc' }],
  priceAsc: [{ columnName: 'currentPriceUahKopiykas', direction: 'Asc' }],
  priceDesc: [{ columnName: 'currentPriceUahKopiykas', direction: 'Desc' }],
};

const csv = (value: string | null): string[] => (value ? value.split(',').filter(Boolean) : []);
const num = (value: string | null): number | undefined =>
  value && !Number.isNaN(Number(value)) ? Number(value) : undefined;

export default function SearchPage() {
  const [params, setParams] = useSearchParams();
  const { data: tree } = useCategoryTree();

  // ── URL-driven state ──────────────────────────────────────────────────────
  const q = params.get('q') ?? '';
  const categorySlug = params.get('category');
  const condition = csv(params.get('condition'));
  const metal = csv(params.get('metal'));
  const country = csv(params.get('country'));
  const priceFrom = num(params.get('priceFrom')); // kopiykas
  const priceTo = num(params.get('priceTo')); // kopiykas
  const status = (params.get('status') as LotStatus | null) ?? undefined; // Active | Sold | undefined(all)
  const endsIn = params.get('endsIn'); // '24h' | '3d' | '7d' | null
  const offset = num(params.get('offset')) ?? 0;
  const sortParam = params.get('sort') as SortKey | null;
  const sort: SortKey = sortParam ?? (q ? 'relevance' : 'endsAt');
  const effectiveSort: SortKey = sort === 'relevance' && !q ? 'endsAt' : sort;

  const categoryNode = categorySlug ? findCategoryBySlug(tree, categorySlug) : undefined;

  /** Merge params; clears empty values and resets pagination unless told otherwise. */
  const update = (next: Record<string, string | null>, resetOffset = true) => {
    setParams(
      (prev) => {
        const p = new URLSearchParams(prev);
        for (const [key, value] of Object.entries(next)) {
          if (value === null || value === '') p.delete(key);
          else p.set(key, value);
        }
        if (resetOffset) p.delete('offset');
        return p;
      },
      { replace: true },
    );
  };

  const toggleFacet = (key: 'condition' | 'metal' | 'country', current: string[], value: string) => {
    const next = current.includes(value) ? current.filter((v) => v !== value) : [...current, value];
    update({ [key]: next.length ? next.join(',') : null });
  };

  // ── Query ────────────────────────────────────────────────────────────────
  // Anchor the relative "ending within" preset to an absolute timestamp once per preset change —
  // recomputing every render would change the value and refire the query in a loop.
  const endingBefore = useMemo(
    () =>
      endsIn && ENDS_IN_HOURS[endsIn]
        ? new Date(Date.now() + ENDS_IN_HOURS[endsIn] * 3_600_000).toISOString()
        : undefined,
    [endsIn],
  );

  // Filters shared by the main search and the category-count query (the latter just omits categoryId).
  const baseFilters: SearchLotsFilters = {
    searchText: q || undefined,
    status,
    condition: condition.length ? condition : undefined,
    metal: metal.length ? metal : undefined,
    country: country.length ? country : undefined,
    priceUahKopiykasFrom: priceFrom,
    priceUahKopiykasTo: priceTo,
    endingBefore,
  };

  const request: SearchLotsRequest = {
    offset,
    count: PAGE_SIZE,
    sortBy: SORT_TO_API[effectiveSort],
    filters: { ...baseFilters, categoryId: categoryNode?.id },
  };
  const { data, isLoading, isError } = useSearchLots(request);

  // Category counts come from a separate facet-only query (count: 0) that omits the category filter,
  // so selecting a category doesn't zero out the others — they stay navigable. Same q + filters as the
  // main search, so the counts still react to the rest of the filter set.
  const categoryCountRequest: SearchLotsRequest = { offset: 0, count: 0, filters: baseFilters };
  const { data: categoryFacetData } = useSearchLots(categoryCountRequest);

  const total = data?.totalCount ?? 0;
  const pageCount = Math.max(1, Math.ceil(total / PAGE_SIZE));
  const page = Math.floor(offset / PAGE_SIZE);
  const hasFilters =
    !!q ||
    !!categorySlug ||
    !!status ||
    !!endsIn ||
    condition.length > 0 ||
    metal.length > 0 ||
    country.length > 0 ||
    priceFrom != null ||
    priceTo != null;

  const clearAll = () => setParams(new URLSearchParams(), { replace: true });

  const heading = categoryNode?.name ?? (q ? `“${q}”` : 'All lots');

  // Leaf categoryId → match count (from the category-only facet query above); the picker aggregates
  // these up the tree.
  const categoryCounts = new Map<number, number>(
    (categoryFacetData?.facets.categoryId ?? []).map((f) => [Number(f.value), f.count]),
  );

  return (
    <div>
      <TopNav />

      {/* Header */}
      <div className="max-w-[1280px] mx-auto px-7 pt-7">
        <h1 className="text-[28px] font-bold m-0">{heading}</h1>
        <p className="text-sm text-text-3 mt-1.5 mb-0">
          {isError ? (
            <span>&nbsp;</span>
          ) : (
            <>
              <span className="mono font-semibold text-text">{total}</span> {total === 1 ? 'result' : 'results'}
              {q && categoryNode && (
                <>
                  {' '}
                  for <span className="text-text-2">&ldquo;{q}&rdquo;</span> in {categoryNode.name}
                </>
              )}
            </>
          )}
        </p>
      </div>

      {/* Layout: filters + results. min-height keeps the footer from leaping when a filter shrinks
          the result set to a short list. */}
      <div
        className="max-w-[1280px] mx-auto px-7 pt-6 grid gap-8 min-h-[70vh]"
        style={{ gridTemplateColumns: '240px 1fr' }}
      >
        <aside>
          <div className="flex items-center justify-between mb-4">
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3">Filters</div>
            {hasFilters && (
              <button type="button" onClick={clearAll} className="text-xs text-accent hover:text-accent-deep">
                Clear all
              </button>
            )}
          </div>

          <CategoryFilter
            tree={tree}
            selectedSlug={categorySlug}
            counts={categoryCounts}
            onSelect={(slug) => update({ category: slug })}
          />

          {/* Status — All / Active / Sold */}
          <div className="mb-6">
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Status</div>
            <div className="flex w-full rounded-md border border-border-strong overflow-hidden">
              {STATUS_OPTIONS.map((opt) => {
                const selected = (opt.status ?? undefined) === status;
                return (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => update({ status: opt.status ?? null })}
                    className={`flex-1 text-center px-2.5 py-0.5 text-[13px] border-r border-border-strong last:border-r-0 ${
                      selected ? 'bg-accent text-white font-medium' : 'bg-surface text-text-2 hover:bg-bg-soft'
                    }`}
                  >
                    {opt.label}
                  </button>
                );
              })}
            </div>
          </div>

          <PriceRangeFilter
            fromUah={priceFrom != null ? priceFrom / 100 : undefined}
            toUah={priceTo != null ? priceTo / 100 : undefined}
            onChange={(fromUah, toUah) =>
              update({
                priceFrom: fromUah != null ? String(Math.round(fromUah * 100)) : null,
                priceTo: toUah != null ? String(Math.round(toUah * 100)) : null,
              })
            }
          />

          {/* Ending within — relative presets, computed to an absolute `endingBefore` at query time. */}
          <div className="mb-6">
            <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-3">Ending within</div>
            <select
              value={endsIn ?? ''}
              onChange={(e) => update({ endsIn: e.target.value || null })}
              className="w-full rounded-md border border-border-strong bg-surface text-sm py-1.5 px-2.5"
              aria-label="Ending within"
            >
              <option value="">Any time</option>
              <option value="24h">24 hours</option>
              <option value="3d">3 days</option>
              <option value="7d">7 days</option>
            </select>
          </div>

          <FacetGroup
            title="Condition"
            values={data?.facets.condition ?? []}
            selected={condition}
            onToggle={(v) => toggleFacet('condition', condition, v)}
          />
          <FacetGroup
            title="Metal"
            values={data?.facets.metal ?? []}
            selected={metal}
            onToggle={(v) => toggleFacet('metal', metal, v)}
          />
          <FacetGroup
            title="Country"
            values={data?.facets.country ?? []}
            selected={country}
            onToggle={(v) => toggleFacet('country', country, v)}
          />
        </aside>

        <div>
          <div className="flex items-center justify-end mb-4 gap-2">
            <label htmlFor="sort" className="text-[13px] text-text-3">
              Sort:
            </label>
            <select
              id="sort"
              value={effectiveSort}
              onChange={(e) => update({ sort: e.target.value })}
              className="rounded-md border border-border-strong bg-surface text-sm py-1.5 px-2.5"
            >
              {q && <option value="relevance">Relevance</option>}
              <option value="endsAt">Ending soonest</option>
              <option value="newest">Newest</option>
              <option value="priceAsc">Price low → high</option>
              <option value="priceDesc">Price high → low</option>
            </select>
          </div>

          {isError ? (
            <div className="bg-surface border border-dashed border-border rounded-lg py-12 text-center">
              <p className="text-text-2 font-medium mb-1">Search is temporarily unavailable</p>
              <p className="text-text-3 text-sm">Please try again in a moment.</p>
            </div>
          ) : isLoading && !data ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
              {Array.from({ length: PAGE_SIZE }).map((_, i) => (
                <div key={i} className="bg-bg-soft border border-border rounded-lg" style={{ aspectRatio: '0.78' }} />
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
                    onClick={() => update({ offset: String(Math.max(0, offset - PAGE_SIZE)) }, false)}
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
                    onClick={() => update({ offset: String(offset + PAGE_SIZE) }, false)}
                    className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Icon name="arrowR" size={14} />
                  </button>
                </div>
              )}
            </>
          ) : (
            <div className="bg-surface border border-dashed border-border rounded-lg py-12 text-center">
              <p className="text-text-3 mb-3">No lots match your filters — try clearing them.</p>
              {hasFilters && (
                <button
                  type="button"
                  onClick={clearAll}
                  className="inline-block rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-4 py-2 text-sm"
                >
                  Clear filters
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      <Footer />
    </div>
  );
}
