import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { moderation } from '@/api/moderation';
import { ApiError } from '@/api/fetch';
import { useToastStore } from '@/state/useToastStore';
import { Icon } from '@/components/Icon';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Direct ban/unban by user id — used when a moderator already has the id from a report context,
 * Stripe dashboard, or a separate report channel. A full search/listing UI requires a backend
 * <c>POST /api/v1/moderation/users/search</c> endpoint that doesn't exist yet (see TODO note below).
 */
export default function ModerationUsersPage() {
  const [userId, setUserId] = useState('');
  const [reason, setReason] = useState('');
  const [busy, setBusy] = useState<'ban' | 'unban' | null>(null);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  const idIsValid = UUID_RE.test(userId.trim());

  const reportError = (action: string, err: unknown) => {
    pushToast({
      kind: 'danger',
      title: `${action} failed`,
      description: err instanceof ApiError ? err.detail ?? err.message : undefined,
    });
  };

  const onBan = async () => {
    if (!idIsValid || !reason.trim()) return;
    setBusy('ban');
    try {
      await moderation.banUser(userId.trim(), reason.trim());
      pushToast({ kind: 'success', title: 'User banned', description: 'Active lots and in-flight payments cancelled.' });
      setUserId('');
      setReason('');
      queryClient.invalidateQueries({ queryKey: ['moderation'] });
    } catch (err) {
      if (err instanceof ApiError && err.status === 502) {
        pushToast({
          kind: 'warning',
          title: 'Partially completed',
          description: err.detail ?? 'Ban committed, but one or more Stripe cancels failed — re-run to retry.',
        });
      } else {
        reportError('Ban', err);
      }
    } finally {
      setBusy(null);
    }
  };

  const onUnban = async () => {
    if (!idIsValid) return;
    setBusy('unban');
    try {
      await moderation.unbanUser(userId.trim());
      pushToast({ kind: 'success', title: 'User unbanned', description: 'Cancelled lots and payments are NOT restored.' });
      setUserId('');
      queryClient.invalidateQueries({ queryKey: ['moderation'] });
    } catch (err) {
      reportError('Unban', err);
    } finally {
      setBusy(null);
    }
  };

  return (
    <>
      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <h3 className="text-sm font-semibold m-0 mb-3.5">User id</h3>
        <input
          type="text"
          value={userId}
          onChange={(e) => setUserId(e.target.value)}
          placeholder="00000000-0000-0000-0000-000000000000"
          className="w-full rounded-md py-2 px-3 text-[13px] mono border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
          style={{ borderColor: 'transparent' }}
          aria-invalid={userId.trim().length > 0 && !idIsValid}
        />
        {userId.trim().length > 0 && !idIsValid && (
          <div className="mt-2 text-[12px]" style={{ color: 'var(--color-danger)' }}>
            Not a valid UUID.
          </div>
        )}
      </section>

      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <div className="flex justify-between items-baseline mb-3.5">
          <h3 className="text-sm font-semibold m-0">Ban</h3>
          <span
            className="inline-flex items-center font-semibold uppercase rounded-full"
            style={{
              fontSize: 11,
              padding: '3px 8px',
              background: 'var(--color-warning-soft)',
              color: '#92400E',
              letterSpacing: '0.04em',
            }}
          >
            Destructive
          </span>
        </div>
        <p className="text-[12.5px] text-text-3 m-0 mb-3 leading-relaxed">
          Flags the user, cancels their active lots, and asks Stripe to cancel any in-flight
          PaymentIntents. JWTs aren't revoked — see <code>THESIS-SCOPE.md</code>.
        </p>
        <textarea
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="Reason for the ban (required)"
          rows={3}
          className="w-full rounded-md py-2 px-3 text-[13px] border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface resize-none"
          style={{ borderColor: 'transparent' }}
        />
        <div className="mt-3 flex justify-end">
          <button
            type="button"
            onClick={onBan}
            disabled={!idIsValid || !reason.trim() || busy !== null}
            className="rounded-md text-white font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed"
            style={{ background: 'var(--color-danger)', cursor: busy ? 'wait' : 'pointer' }}
          >
            {busy === 'ban' ? 'Banning…' : 'Ban user'}
          </button>
        </div>
      </section>

      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <h3 className="text-sm font-semibold m-0 mb-3.5">Unban</h3>
        <p className="text-[12.5px] text-text-3 m-0 mb-3 leading-relaxed">
          Lifts the ban. Cancelled lots and payments are <strong>not</strong> restored — market prices
          and auction timing are already stale by then.
        </p>
        <div className="flex justify-end">
          <button
            type="button"
            onClick={onUnban}
            disabled={!idIsValid || busy !== null}
            className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {busy === 'unban' ? 'Unbanning…' : 'Unban user'}
          </button>
        </div>
      </section>

      <div className="flex gap-2.5 items-start text-[12px] text-text-3 px-1">
        <Icon name="info" size={14} color="var(--color-text-3)" />
        <span>
          A searchable user table needs a backend endpoint
          (<code>POST /api/v1/moderation/users/search</code>) — not built yet. Today you get the id
          from a report or the Stripe dashboard.
        </span>
      </div>
    </>
  );
}
