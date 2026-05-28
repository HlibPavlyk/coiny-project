import { useEffect, useState } from 'react';
import { lots, type ReportReason } from '@/api/lots';
import { ApiError } from '@/api/fetch';
import { useToastStore } from '@/state/useToastStore';

interface ReportLotModalProps {
  open: boolean;
  lotId: string;
  onClose: () => void;
}

const REASONS: { id: ReportReason; label: string; hint: string }[] = [
  { id: 'Counterfeit', label: 'Counterfeit', hint: 'The item looks fake or replicates a known piece.' },
  { id: 'NotAsDescribed', label: 'Not as described', hint: 'Photos, attributes, or condition do not match the listing.' },
  { id: 'Spam', label: 'Spam', hint: 'Off-topic, duplicate, or promotional listing.' },
  { id: 'Inappropriate', label: 'Inappropriate', hint: 'Offensive content or violates marketplace policy.' },
  { id: 'Other', label: 'Other', hint: 'Use the note to explain the issue.' },
];

/**
 * Lot-report dialog — pick a reason, optionally add a note (≤500 chars), submit. Anonymous and
 * authenticated users can both report; the backend rate-limits per user (5/h) or per IP (3/h).
 * The submitted report lands in the moderation queue as <c>Open</c>.
 */
export function ReportLotModal({ open, lotId, onClose }: ReportLotModalProps) {
  const [reason, setReason] = useState<ReportReason | null>(null);
  const [note, setNote] = useState('');
  const [busy, setBusy] = useState(false);
  const pushToast = useToastStore((s) => s.push);

  // Reset state when the modal closes so a re-open starts fresh.
  useEffect(() => {
    if (!open) {
      setReason(null);
      setNote('');
      setBusy(false);
    }
  }, [open]);

  // Escape closes the modal (unless we're mid-submit).
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !busy) onClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open, busy, onClose]);

  if (!open) return null;

  const submit = async () => {
    if (!reason) return;
    setBusy(true);
    try {
      await lots.reportLot(lotId, reason, note);
      pushToast({
        kind: 'success',
        title: 'Report submitted',
        description: 'Thanks — a moderator will review this lot.',
      });
      onClose();
    } catch (err) {
      if (err instanceof ApiError && err.status === 429) {
        pushToast({
          kind: 'warning',
          title: 'Too many reports',
          description: err.detail ?? 'Please wait before submitting another report.',
        });
      } else if (err instanceof ApiError && err.status === 404) {
        pushToast({ kind: 'danger', title: 'Lot not found' });
      } else {
        pushToast({
          kind: 'danger',
          title: 'Report failed',
          description: err instanceof ApiError ? err.detail ?? err.message : undefined,
        });
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      style={{ background: 'rgba(15, 23, 42, 0.45)' }}
      onMouseDown={() => {
        if (!busy) onClose();
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Report this lot"
        className="bg-surface border border-border rounded-xl w-full max-w-[480px] p-5"
        style={{ boxShadow: '0 24px 60px -12px rgba(15, 23, 42, 0.35)' }}
        onMouseDown={(e) => e.stopPropagation()}
      >
        <h2 className="text-[17px] font-semibold m-0">Report this lot</h2>
        <p className="text-[13px] text-text-2 mt-2 mb-0 leading-relaxed">
          Pick the closest reason. A moderator reviews every report — they can dismiss it or take action
          (take the lot down, ban the seller). Abuse of the report system can be rate-limited.
        </p>

        <fieldset className="mt-4 flex flex-col gap-1.5 border-0 p-0">
          <legend className="text-[12px] font-medium text-text-2 mb-1.5">Reason</legend>
          {REASONS.map((r) => {
            const checked = reason === r.id;
            return (
              <label
                key={r.id}
                className="flex items-start gap-2.5 px-3 py-2 rounded-md cursor-pointer border transition"
                style={{
                  borderColor: checked ? 'var(--color-accent)' : 'var(--color-border-soft)',
                  background: checked ? 'var(--color-bg-soft)' : 'transparent',
                }}
              >
                <input
                  type="radio"
                  name="report-reason"
                  value={r.id}
                  checked={checked}
                  onChange={() => setReason(r.id)}
                  className="mt-0.5"
                />
                <div className="flex-1 min-w-0">
                  <div className="text-[13px] font-semibold text-text">{r.label}</div>
                  <div className="text-[11.5px] text-text-3 mt-0.5 leading-snug">{r.hint}</div>
                </div>
              </label>
            );
          })}
        </fieldset>

        <div className="mt-4">
          <label className="block text-[12px] font-medium text-text-2 mb-1.5">
            Additional details <span className="text-text-3">(optional, up to 500 characters)</span>
          </label>
          <textarea
            value={note}
            onChange={(e) => setNote(e.target.value)}
            placeholder="Describe what made you flag this lot…"
            rows={3}
            maxLength={500}
            className="w-full rounded-md border border-border-strong bg-surface px-3 py-2 text-[13px] text-text resize-none focus:outline-none focus:border-accent"
          />
          <div className="text-right text-[11px] text-text-3 mt-1 tabular-nums">{note.length}/500</div>
        </div>

        <div className="flex justify-end gap-2 mt-2">
          <button
            type="button"
            onClick={onClose}
            disabled={busy}
            className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-2 text-[13px] disabled:opacity-60"
            style={{ cursor: busy ? 'not-allowed' : 'pointer' }}
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={submit}
            disabled={busy || !reason}
            className="rounded-md font-medium px-4 py-2 text-[13px] text-white bg-accent hover:bg-accent-deep disabled:opacity-60"
            style={{ cursor: busy || !reason ? 'not-allowed' : 'pointer' }}
          >
            {busy ? 'Submitting…' : 'Submit report'}
          </button>
        </div>
      </div>
    </div>
  );
}
