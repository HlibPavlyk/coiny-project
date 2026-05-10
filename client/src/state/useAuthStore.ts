import { create } from 'zustand';
import { auth, type MeModel, type LoginPayload, type RegisterPayload } from '@/api/auth';
import { ApiError } from '@/api/fetch';

interface AuthState {
  user: MeModel | null;
  isLoading: boolean;
  refresh: () => Promise<void>;
  signIn: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  signOut: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isLoading: true,

  refresh: async () => {
    set({ isLoading: true });
    try {
      const me = await auth.me();
      set({ user: me, isLoading: false });
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        set({ user: null, isLoading: false });
        return;
      }
      set({ user: null, isLoading: false });
    }
  },

  signIn: async (payload) => {
    const me = await auth.login(payload);
    set({ user: me });
  },

  register: async (payload) => {
    const me = await auth.register(payload);
    set({ user: me });
  },

  signOut: async () => {
    await auth.logout();
    set({ user: null });
  },
}));
