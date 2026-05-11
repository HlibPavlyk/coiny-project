import { useEffect } from 'react';
import { useToastStore, type ToastKind } from '@/state/useToastStore';

interface ToastVisual {
  bg: string;
  border: string;
  iconColor: string;
  icon: string;
}

const KIND_VISUALS: Record<ToastKind, ToastVisual> = {
  info: { bg: '#EFF6FF', border: '#BFDBFE', iconColor: '#1E40AF', icon: 'i' },
  success: { bg: '#ECFDF5', border: '#A7F3D0', iconColor: '#15803D', icon: '✓' },
  warning: { bg: '#FFFBEB', border: '#FCD34D', iconColor: '#B45309', icon: '!' },
  danger: { bg: '#FEF2F2', border: '#FCA5A5', iconColor: '#B91C1C', icon: '×' },
};

const TITLE_COLOR: Record<ToastKind, string> = {
  info: '#1E3A8A',
  success: '#14532D',
  warning: '#7C2D12',
  danger: '#7F1D1D',
};

export function ToastViewport() {
  const toasts = useToastStore((s) => s.toasts);

  return (
    <div
      role="region"
      aria-live="polite"
      aria-label="Notifications"
      className="fixed top-5 left-1/2 -translate-x-1/2 z-50 flex flex-col gap-2.5 pointer-events-none"
      style={{ width: 'min(440px, calc(100vw - 32px))' }}
    >
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} />
      ))}
    </div>
  );
}

function ToastItem({ toast }: { toast: ReturnType<typeof useToastStore.getState>['toasts'][number] }) {
  const dismiss = useToastStore((s) => s.dismiss);
  const v = KIND_VISUALS[toast.kind];

  // Slide-in animation on mount via CSS class toggle. Pure CSS, no extra library.
  useEffect(() => {
    // Animation runs via the data-state attribute below
  }, []);

  return (
    <div
      role="status"
      className="pointer-events-auto rounded-lg border px-4 py-3 toast-enter"
      style={{
        background: v.bg,
        borderColor: v.border,
        boxShadow: '0 10px 32px -8px rgba(15, 23, 42, 0.18), 0 4px 12px -4px rgba(15, 23, 42, 0.12)',
      }}
    >
      <div className="flex items-start gap-3">
        <div
          className="flex-shrink-0 flex items-center justify-center rounded-full font-bold"
          style={{
            width: 24,
            height: 24,
            background: v.iconColor,
            color: '#fff',
            fontSize: 14,
            lineHeight: 1,
          }}
          aria-hidden="true"
        >
          {v.icon}
        </div>
        <div className="flex-1 min-w-0">
          <div
            className="font-semibold text-[14px] leading-snug"
            style={{ color: TITLE_COLOR[toast.kind] }}
          >
            {toast.title}
          </div>
          {toast.description && (
            <div className="text-[13px] mt-1 text-text-2 leading-relaxed">{toast.description}</div>
          )}
        </div>
        <button
          type="button"
          aria-label="Close"
          onClick={() => dismiss(toast.id)}
          className="flex-shrink-0 text-text-3 hover:text-text leading-none text-lg -mr-1 -mt-0.5 px-1"
        >
          ×
        </button>
      </div>
    </div>
  );
}
