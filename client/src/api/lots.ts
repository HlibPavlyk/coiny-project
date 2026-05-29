import { useEffect, useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { api } from './fetch';
import type { LotCardModel } from '@/components/LotCard';

export interface Paginated<T> {
  totalCount: number;
  items: T[];
}

export interface SortByModel {
  columnName: string;
  direction: 'Asc' | 'Desc';
}

export interface PageRequest {
  offset?: number;
  count?: number;
  sortBy?: SortByModel[];
}

export type LotCondition = 'UNC' | 'AU' | 'XF' | 'VF' | 'F' | 'VG' | 'G' | 'Poor' | 'Ungraded';
export type LotStatus = 'Draft' | 'Active' | 'Sold' | 'EndedNoSale' | 'Cancelled';

export interface LotImage {
  id: string;
  publicUrl: string;
  displayOrder: number;
  width: number;
  height: number;
}

export interface LotSeller {
  id: string;
  displayName: string;
  trustScore: number;
}

export interface LotWinningBid {
  id: string;
  bidderDisplayName: string;
  amountUahKopiykas: number;
}

export interface LotCategoryBreadcrumb {
  id: number;
  slug: string;
  namePath: string[];
}

export interface LotDetailModel {
  id: string;
  title: string;
  description: string;
  category: LotCategoryBreadcrumb;
  condition: LotCondition;
  startingPriceUahKopiykas: number;
  currentPriceUahKopiykas: number;
  bidCount: number;
  viewCount: number;
  status: LotStatus;
  startsAt: string | null;
  endsAt: string;
  attributes: Record<string, unknown>;
  images: LotImage[];
  seller: LotSeller;
  winningBid: LotWinningBid | null;
  /** True when the authenticated caller's bid is currently the top bid on this lot. */
  isCallerLeading: boolean;
  /** Populated only for the winning bidder on a Sold lot if they already started or finished payment. */
  callerPaymentId: string | null;
  callerPaymentStatus:
    | 'PendingAuthorization'
    | 'Authorized'
    | 'Captured'
    | 'Cancelled'
    | 'Failed'
    | null;
}

export interface CreateLotPayload {
  title: string;
  description: string;
  categoryId: number;
  condition: LotCondition;
  startingPriceUahKopiykas: number;
  endsAt: string;
  attributes: Record<string, unknown>;
}

export type UpdateLotPayload = CreateLotPayload;

export interface PublishedLotModel {
  id: string;
  status: LotStatus;
  startsAt: string;
  endsAt: string;
}

export interface LotImageUpload {
  id: string;
  publicUrl: string;
  displayOrder: number;
}

export interface MyLotItem {
  id: string;
  title: string;
  coverImageUrl: string;
  currentPriceUahKopiykas: number;
  bidCount: number;
  endsAt: string;
  status: LotStatus;
  deletedAt: string | null;
}

export interface MyLotsRequest extends PageRequest {
  filters?: { status?: LotStatus };
}

export interface PublicLotsRequest extends PageRequest {
  filters?: { categoryId?: number; sellerId?: string; status?: LotStatus };
}

/** One facet bucket returned by Meilisearch: a value present in the results and its count. */
export interface FacetValue {
  value: string;
  count: number;
}

/** Facet distribution keyed by field name (e.g. `metal`, `country`, `condition`). */
export type SearchFacets = Record<string, FacetValue[]>;

export interface SearchLotsFilters {
  categoryId?: number;
  status?: LotStatus;
  searchText?: string;
  condition?: string[];
  metal?: string[];
  country?: string[];
  priceUahKopiykasFrom?: number;
  priceUahKopiykasTo?: number;
  endingBefore?: string;
}

export interface SearchLotsRequest extends PageRequest {
  filters?: SearchLotsFilters;
}

export interface SearchLotsResponse {
  totalCount: number;
  items: LotCardModel[];
  facets: SearchFacets;
}

export const lots = {
  byCategorySearch: (categoryId: number, paginate: PageRequest) =>
    api<Paginated<LotCardModel>>(`/api/v1/lots/list`, {
      method: 'POST',
      body: { ...paginate, filters: { categoryId, status: 'Active' } } satisfies PublicLotsRequest,
    }),
  getLot: (id: string) => api<LotDetailModel>(`/api/v1/lots/${id}`),
  createLot: (payload: CreateLotPayload) =>
    api<{ id: string }>(`/api/v1/lots`, { method: 'POST', body: payload }),
  updateLot: (id: string, payload: UpdateLotPayload) =>
    api<void>(`/api/v1/lots/${id}`, { method: 'PUT', body: payload }),
  publishLot: (id: string) =>
    api<PublishedLotModel>(`/api/v1/lots/${id}/publish`, { method: 'POST' }),
  deleteLot: (id: string) => api<void>(`/api/v1/lots/${id}`, { method: 'DELETE' }),
  uploadImage: (id: string, file: File) => {
    const form = new FormData();
    form.append('file', file);
    return api<LotImageUpload>(`/api/v1/lots/${id}/images`, {
      method: 'POST',
      body: form,
      skipJsonBody: true,
    });
  },
  deleteImage: (id: string, imageId: string) =>
    api<void>(`/api/v1/lots/${id}/images/${imageId}`, { method: 'DELETE' }),
  reorderImages: (id: string, imageIds: string[]) =>
    api<void>(`/api/v1/lots/${id}/images/reorder`, {
      method: 'POST',
      body: { lotId: id, imageIds },
    }),
  myLotsSearch: (request: MyLotsRequest) =>
    api<Paginated<MyLotItem>>(`/api/v1/users/me/lots/list`, { method: 'POST', body: request }),
  search: (request: SearchLotsRequest) =>
    api<SearchLotsResponse>(`/api/v1/lots/search`, { method: 'POST', body: request }),
  publicLots: (request: PublicLotsRequest) =>
    api<Paginated<LotCardModel>>(`/api/v1/lots/list`, { method: 'POST', body: request }),
  suggest: (q: string) =>
    api<LotSuggestItem[]>(`/api/v1/lots/suggest?q=${encodeURIComponent(q)}`),
  reportLot: (lotId: string, reason: ReportReason, note?: string) =>
    api<void>(`/api/v1/lots/${lotId}/report`, {
      method: 'POST',
      body: { lotId, reason, note: note?.trim() || undefined },
    }),
};

/** Mirrors <c>Coiny.Domain.Enums.ReportReason</c>. */
export type ReportReason = 'Counterfeit' | 'NotAsDescribed' | 'Spam' | 'Inappropriate' | 'Other';

/** Minimal projection for the typeahead dropdown — matches `LotSuggestItem` on the server. */
export interface LotSuggestItem {
  id: string;
  title: string;
  coverImageUrl: string;
  categoryPath: string;
  currentPriceUahKopiykas: number;
}

/**
 * Debounced typeahead query. The hook is responsible for pacing the input — the API call fires only
 * after the user has stopped typing for `debounceMs`. An empty/short query is disabled so we don't
 * pummel the search index on backspace-to-empty.
 */
export function useSuggestLots(rawQ: string, debounceMs = 200) {
  const [debounced, setDebounced] = useState('');
  useEffect(() => {
    const trimmed = rawQ.trim();
    if (trimmed.length < 2) {
      setDebounced('');
      return;
    }
    const t = setTimeout(() => setDebounced(trimmed), debounceMs);
    return () => clearTimeout(t);
  }, [rawQ, debounceMs]);

  return useQuery({
    queryKey: ['lots', 'suggest', debounced],
    queryFn: () => lots.suggest(debounced),
    enabled: debounced.length >= 2,
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}

/** Hook for paginated lots-in-category listing. */
export function useLotsByCategory(categoryId: number | undefined, paginate: PageRequest) {
  return useQuery({
    queryKey: ['lots', 'by-category', categoryId, paginate],
    queryFn: () => lots.byCategorySearch(categoryId!, paginate),
    enabled: categoryId !== undefined,
  });
}

/** Hook for a generic public lot listing (any category/seller/status + sort) — used by home sections. */
export function usePublicLots(request: PublicLotsRequest) {
  return useQuery({
    queryKey: ['lots', 'public', request],
    queryFn: () => lots.publicLots(request),
    placeholderData: keepPreviousData,
  });
}

/** Hook for a single lot's full detail. */
export function useLot(id: string | undefined) {
  return useQuery({
    queryKey: ['lot', id],
    queryFn: () => lots.getLot(id!),
    enabled: !!id,
  });
}

/**
 * Hook for the Meilisearch-backed lot search. The full request (paginate + filters) is the cache key,
 * so each filter combination is its own entry. Previous results are kept while a new query loads, so
 * the grid doesn't flash empty (and the layout doesn't shift) when facets/sort change.
 */
export function useSearchLots(request: SearchLotsRequest) {
  return useQuery({
    queryKey: ['lots', 'search', request],
    queryFn: () => lots.search(request),
    placeholderData: keepPreviousData,
  });
}
