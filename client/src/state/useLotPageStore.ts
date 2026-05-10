import { create } from 'zustand';

/**
 * Per-lot ephemeral state populated by SignalR pushes (sprint 2).
 * Lives only while the /lot/:id route is mounted; reset on unmount.
 */
interface LotPageState {
  liveCurrentPriceUahKopiykas: number | null;
  liveBidCount: number | null;
  liveEndsAt: string | null;
  reset: () => void;
  setSnapshot: (snapshot: Partial<Omit<LotPageState, 'reset' | 'setSnapshot'>>) => void;
}

export const useLotPageStore = create<LotPageState>((set) => ({
  liveCurrentPriceUahKopiykas: null,
  liveBidCount: null,
  liveEndsAt: null,
  reset: () =>
    set({
      liveCurrentPriceUahKopiykas: null,
      liveBidCount: null,
      liveEndsAt: null,
    }),
  setSnapshot: (snapshot) => set(snapshot),
}));
