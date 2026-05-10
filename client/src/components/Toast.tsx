import { useToastStore } from '@/state/useToastStore';
import clsx from 'clsx';

const kindStyles: Record<string, string> = {
  info: 'bg-info-soft text-info border-info/20',
  success: 'bg-success-soft text-success border-success/20',
  warning: 'bg-warning-soft text-warning border-warning/20',
  danger: 'bg-danger-soft text-danger border-danger/20',
};

export function ToastViewport() {
  const toasts = useToastStore((s) => s.toasts);
  const dismiss = useToastStore((s) => s.dismiss);

  return (
    <div
      role="region"
      aria-live="polite"
      aria-label="Notifications"
      className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 max-w-sm pointer-events-none"
    >
      {toasts.map((toast) => (
        <div
          key={toast.id}
          role="status"
          className={clsx(
            'pointer-events-auto rounded-lg border px-4 py-3 shadow-2 bg-surface',
            kindStyles[toast.kind],
          )}
        >
          <div className="flex items-start gap-3">
            <div className="flex-1 min-w-0">
              <div className="font-semibold text-sm">{toast.title}</div>
              {toast.description && (
                <div className="text-sm text-text-2 mt-1">{toast.description}</div>
              )}
            </div>
            <button
              type="button"
              aria-label="Close"
              onClick={() => dismiss(toast.id)}
              className="text-text-3 hover:text-text leading-none"
            >
              ×
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}
