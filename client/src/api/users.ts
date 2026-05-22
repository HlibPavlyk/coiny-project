import { api } from './fetch';
import type { Paginated, PageRequest, LotStatus, PublicLotsRequest } from './lots';
import type { LotCardModel } from '@/components/LotCard';

export interface PublicProfileModel {
  id: string;
  displayName: string;
  trustScore: number;
  memberSince: string;
  lastActiveAt: string;
  lotsSold: number;
  activeLots: number;
  avgSalePriceUahKopiykas: number;
}

export const users = {
  getPublicProfile: (userId: string) =>
    api<PublicProfileModel>(`/api/v1/users/${userId}/public`),

  searchLotsBySeller: (userId: string, status: Extract<LotStatus, 'Active' | 'Sold'>, paginate: PageRequest) =>
    api<Paginated<LotCardModel>>(`/api/v1/lots/search`, {
      method: 'POST',
      body: { ...paginate, filters: { sellerId: userId, status } } satisfies PublicLotsRequest,
    }),
};
