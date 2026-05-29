import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { subscribeToLot, unsubscribeFromLot } from '@/signalr/auctionHubClient';
import { useAuthStore } from '@/state/useAuthStore';

/**
 * Subscribes the lot detail page to the auction hub. On every <c>LotChanged</c> signal the hook
 * invalidates the lot + bid-history caches so React Query re-fetches authoritative state from
 * REST. Anonymous visitors skip the connection entirely (they get stale-while-revalidate
 * refreshes from React Query's defaults instead of live ticks).
 */
export function useAuctionLot(lotId: string | undefined): void {
  const user = useAuthStore((s) => s.user);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!user || !lotId) return;

    let cancelled = false;

    const handlers = {
      onLotChanged() {
        if (cancelled) return;
        queryClient.invalidateQueries({ queryKey: ['lot', lotId] });
        queryClient.invalidateQueries({ queryKey: ['bid-history', lotId] });
      },
    };

    void subscribeToLot(lotId, handlers);

    return () => {
      cancelled = true;
      void unsubscribeFromLot(lotId);
    };
  }, [lotId, user, queryClient]);
}
