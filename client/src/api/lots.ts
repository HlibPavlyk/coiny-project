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

export const lots = {
  byCategorySearch: (categoryId: number, paginate: PageRequest) =>
    api<Paginated<LotCardModel>>(`/api/v1/categories/${categoryId}/lots/search`, {
      method: 'POST',
      body: paginate,
    }),
};

/** Hook for paginated lots-in-category listing. */
export function useLotsByCategory(categoryId: number | undefined, paginate: PageRequest) {
  return useQuery({
    queryKey: ['lots', 'by-category', categoryId, paginate],
    queryFn: () => lots.byCategorySearch(categoryId!, paginate),
    enabled: categoryId !== undefined,
  });
}
