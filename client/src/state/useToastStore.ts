import { create } from 'zustand';

export type ToastKind = 'info' | 'success' | 'warning' | 'danger';

export interface Toast {
  id: string;
  kind: ToastKind;
  title: string;
  description?: string;
  durationMs?: number;
}

interface ToastState {
  toasts: Toast[];
  push: (toast: Omit<Toast, 'id'>) => string;
  dismiss: (id: string) => void;
}

export const useToastStore = create<ToastState>((set, get) => ({
  toasts: [],

  push: (toast) => {
    const id = crypto.randomUUID();
    set((s) => ({ toasts: [...s.toasts, { id, durationMs: 4000, ...toast }] }));
    const duration = toast.durationMs ?? 4000;
    if (duration > 0) {
      setTimeout(() => get().dismiss(id), duration);
    }
    return id;
  },

  dismiss: (id) => set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })),
}));
