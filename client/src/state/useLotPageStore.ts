import { create } from 'zustand';
import type { LotStatus } from '@/api/lots';

export interface LiveWinner {
  finalPriceUahKopiykas: number | null;
  winnerDisplayName: string | null;
}

/**
 * Per-lot ephemeral state populated by SignalR pushes (sprint 2).
 * Lives only while the /lot/:id route is mounted; reset on unmount.
 */
interface LotPageState {
  liveCurrentPriceUahKopiykas: number | null;
  liveBidCount: number | null;
  liveEndsAt: string | null;
  liveStatus: LotStatus | null;
  liveWinner: LiveWinner | null;
  reset: () => void;
  setSnapshot: (snapshot: Partial<Omit<LotPageState, 'reset' | 'setSnapshot'>>) => void;
}

export const useLotPageStore = create<LotPageState>((set) => ({
  liveCurrentPriceUahKopiykas: null,
  liveBidCount: null,
  liveEndsAt: null,
  liveStatus: null,
  liveWinner: null,
  reset: () =>
    set({
      liveCurrentPriceUahKopiykas: null,
      liveBidCount: null,
      liveEndsAt: null,
      liveStatus: null,
      liveWinner: null,
    }),
  setSnapshot: (snapshot) => set(snapshot),
}));
