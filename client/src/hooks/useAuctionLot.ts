import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { subscribeToLot, unsubscribeFromLot } from '@/signalr/auctionHubClient';
import { useAuthStore } from '@/state/useAuthStore';
import { useLotPageStore } from '@/state/useLotPageStore';
import { useToastStore } from '@/state/useToastStore';
import { formatKopiykasAsUah } from '@/lib/money';

/**
 * Wires the lot detail page into the SignalR auction hub. Anonymous visitors skip the connection
 * and rely on TanStack Query refetches for stale-while-revalidate freshness.
 *
 * On mount: subscribes to <c>lot:{lotId:N}</c> events and routes them to:
 *   - <c>useLotPageStore</c> setters (overlay the snapshot fetched by <c>useLot</c>)
 *   - toast notifications (BidPlaced shows the new price; AuctionExtended announces the +5 min)
 *   - <c>queryClient.invalidateQueries(['lot', lotId])</c> on AuctionClosed so bid history flips
 *     from anonymized to real display names and the read model picks up the winning bid
 *
 * On unmount: leaves the group and resets the live-state slice for the next lot.
 */
export function useAuctionLot(lotId: string | undefined): void {
  const user = useAuthStore((s) => s.user);
  const setSnapshot = useLotPageStore((s) => s.setSnapshot);
  const resetSnapshot = useLotPageStore((s) => s.reset);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!user || !lotId) return;

    let cancelled = false;

    const handlers = {
      onBidPlaced(e: { currentPriceUahKopiykas: number; bidCount: number; leaderDisplayName: string }) {
        if (cancelled) return;
        setSnapshot({
          liveCurrentPriceUahKopiykas: e.currentPriceUahKopiykas,
          liveBidCount: e.bidCount,
        });
        pushToast({
          kind: 'info',
          title: `New bid: ${formatKopiykasAsUah(e.currentPriceUahKopiykas, { integer: true })}`,
        });
      },
      onAuctionExtended(e: { newEndsAt: string }) {
        if (cancelled) return;
        setSnapshot({ liveEndsAt: e.newEndsAt });
        pushToast({
          kind: 'warning',
          title: 'Auction extended',
          description: 'Anti-snipe added 5 more minutes.',
        });
      },
      onAuctionClosed(e: { finalPriceUahKopiykas: number | null; winnerDisplayName: string | null }) {
        if (cancelled) return;
        setSnapshot({
          liveStatus: e.winnerDisplayName ? 'Sold' : 'EndedNoSale',
          liveWinner: { finalPriceUahKopiykas: e.finalPriceUahKopiykas, winnerDisplayName: e.winnerDisplayName },
        });
        // Refresh the cached lot + bid-history so anonymized names flip to real ones.
        queryClient.invalidateQueries({ queryKey: ['lot', lotId] });
        pushToast({
          kind: e.winnerDisplayName ? 'success' : 'info',
          title: e.winnerDisplayName ? 'Auction won' : 'Auction ended with no sale',
          description: e.winnerDisplayName
            ? `${e.winnerDisplayName} took it at ${formatKopiykasAsUah(e.finalPriceUahKopiykas ?? 0, { integer: true })}`
            : undefined,
        });
      },
      onConnectionLost() {
        if (cancelled) return;
        pushToast({
          kind: 'warning',
          title: 'Live updates disconnected',
          description: 'Refreshing every minute as a fallback.',
        });
      },
    };

    void subscribeToLot(lotId, handlers);

    return () => {
      cancelled = true;
      void unsubscribeFromLot(lotId);
      resetSnapshot();
    };
  }, [lotId, user, setSnapshot, resetSnapshot, pushToast, queryClient]);
}
