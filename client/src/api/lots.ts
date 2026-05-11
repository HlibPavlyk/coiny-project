import { useQuery } from '@tanstack/react-query';
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

export const lots = {
  byCategorySearch: (categoryId: number, paginate: PageRequest) =>
    api<Paginated<LotCardModel>>(`/api/v1/categories/${categoryId}/lots/search`, {
      method: 'POST',
      body: paginate,
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
    api<Paginated<MyLotItem>>(`/api/v1/lots/mine/search`, { method: 'POST', body: request }),
};

/** Hook for paginated lots-in-category listing. */
export function useLotsByCategory(categoryId: number | undefined, paginate: PageRequest) {
  return useQuery({
    queryKey: ['lots', 'by-category', categoryId, paginate],
    queryFn: () => lots.byCategorySearch(categoryId!, paginate),
    enabled: categoryId !== undefined,
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
