import { useMutation, useQuery, type UseMutationOptions } from '@tanstack/react-query';
import { api } from './fetch';
import type { LotStatus, Paginated, PageRequest } from './lots';

export interface PlaceBidPayload {
  amountUahKopiykas: number;
}

export interface PlaceBidModel {
  id: string;
  lotId: string;
  amountUahKopiykas: number;
  newCurrentPriceUahKopiykas: number;
  newBidCount: number;
  newEndsAt: string;
}

export interface BidItemModel {
  id: string;
  amountUahKopiykas: number;
  bidderDisplay: string;
  createdAt: string;
}

export interface MyBidItemModel {
  bidId: string;
  amountUahKopiykas: number;
  createdAt: string;
  lot: {
    id: string;
    title: string;
    coverImageUrl: string;
    currentPriceUahKopiykas: number;
    status: LotStatus;
    endsAt: string;
    isCallerLeading: boolean;
  };
}

export const bids = {
  placeBid: (lotId: string, payload: PlaceBidPayload) =>
    api<PlaceBidModel>(`/api/v1/lots/${lotId}/bids`, { method: 'POST', body: payload }),
  bidHistorySearch: (lotId: string, paginate: PageRequest) =>
    api<Paginated<BidItemModel>>(`/api/v1/lots/${lotId}/bids/list`, {
      method: 'POST',
      body: paginate,
    }),
  myBidsSearch: (paginate: PageRequest) =>
    api<Paginated<MyBidItemModel>>(`/api/v1/users/me/bids/list`, {
      method: 'POST',
      body: paginate,
    }),
};

/** Mutation hook for placing a bid. Caller provides onSuccess/onError to wire optimistic UI + toasts. */
export function usePlaceBid(
  lotId: string,
  options?: Omit<
    UseMutationOptions<PlaceBidModel, unknown, PlaceBidPayload>,
    'mutationFn'
  >,
) {
  return useMutation({
    mutationFn: (payload: PlaceBidPayload) => bids.placeBid(lotId, payload),
    ...options,
  });
}

export function useBidHistory(lotId: string | undefined, paginate: PageRequest) {
  return useQuery({
    queryKey: ['bid-history', lotId, paginate],
    queryFn: () => bids.bidHistorySearch(lotId!, paginate),
    enabled: !!lotId,
  });
}

export function useMyBids(paginate: PageRequest) {
  return useQuery({
    queryKey: ['my-bids', paginate],
    queryFn: () => bids.myBidsSearch(paginate),
  });
}
