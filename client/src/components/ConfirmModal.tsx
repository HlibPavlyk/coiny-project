import { useEffect, useRef } from 'react';
import { useFocusTrap } from '@/lib/useFocusTrap';

export interface ModalAction {
  label: string;
  onClick: () => void;
  /** Render as a destructive (red) button. */
  danger?: boolean;
}

interface NoteField {
  value: string;
  onChange: (value: string) => void;
  label?: string;
  placeholder?: string;
  /** When true, actions are disabled until the note is non-empty. */
  required?: boolean;
}

interface ConfirmModalProps {
  open: boolean;
  title: string;
  description?: string;
  /** Optional free-text note (e.g. a resolution / ban reason). */
  note?: NoteField;
  /** Primary action buttons, rendered left-to-right after Cancel. */
  actions: ModalAction[];
  onClose: () => void;
  busy?: boolean;
}

/**
 * Reusable confirmation dialog with an optional note field and one or more action buttons. Closes on
 * Escape or backdrop click (unless busy). Actions are disabled while busy or while a required note is empty.
 */
export function ConfirmModal({ open, title, description, note, actions, onClose, busy }: ConfirmModalProps) {
  const dialogRef = useRef<HTMLDivElement>(null);
  useFocusTrap(dialogRef, open);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !busy) onClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open, busy, onClose]);

  if (!open) return null;

  const noteInvalid = note?.required && note.value.trim().length === 0;
  const actionsDisabled = busy || noteInvalid;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      style={{ background: 'rgba(15, 23, 42, 0.45)' }}
      onMouseDown={() => {
        if (!busy) onClose();
      }}
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-label={title}
        className="bg-surface border border-border rounded-xl w-full max-w-[460px] p-5"
        style={{ boxShadow: '0 24px 60px -12px rgba(15, 23, 42, 0.35)' }}
        onMouseDown={(e) => e.stopPropagation()}
      >
        <h2 className="text-[17px] font-semibold m-0">{title}</h2>
        {description && <p className="text-[13px] text-text-2 mt-2 mb-0 leading-relaxed">{description}</p>}

        {note && (
          <div className="mt-4">
            {note.label && (
              <label className="block text-[12px] font-medium text-text-2 mb-1.5">{note.label}</label>
            )}
            <textarea
              value={note.value}
              onChange={(e) => note.onChange(e.target.value)}
              placeholder={note.placeholder}
              rows={3}
              maxLength={500}
              className="w-full rounded-md border border-border-strong bg-surface px-3 py-2 text-[13px] text-text resize-none focus:outline-none focus:border-accent"
            />
          </div>
        )}

        <div className="flex justify-end gap-2 mt-5">
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-2 text-[13px] disabled:opacity-60"
            style={{ cursor: busy ? 'not-allowed' : 'pointer' }}
          >
            Cancel
          </button>
          {actions.map((action) => (
            <button
              key={action.label}
              type="button"
              onClick={action.onClick}
              disabled={actionsDisabled}
              className={`rounded-md font-medium px-4 py-2 text-[13px] text-white disabled:opacity-60 ${
                action.danger ? 'bg-red-600 hover:bg-red-700' : 'bg-accent hover:bg-accent-deep'
              }`}
              style={{ cursor: actionsDisabled ? 'not-allowed' : 'pointer' }}
            >
              {action.label}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
