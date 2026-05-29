import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { moderation } from '@/api/moderation';
import { ApiError } from '@/api/fetch';
import { useToastStore } from '@/state/useToastStore';
import { Icon } from '@/components/Icon';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Direct lot takedown by id — used when a moderator already has the id from a report context or a
 * separate channel. A full takedown log / recently-taken-down list requires a backend listing
 * endpoint that doesn't exist yet.
 */
export default function ModerationLotsPage() {
  const [lotId, setLotId] = useState('');
  const [busy, setBusy] = useState(false);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  const idIsValid = UUID_RE.test(lotId.trim());

  const onTakedown = async () => {
    if (!idIsValid) return;
    setBusy(true);
    try {
      await moderation.takedownLot(lotId.trim());
      pushToast({
        kind: 'success',
        title: 'Lot taken down',
        description: 'Removed from public listings; drops from search within 15s.',
      });
      setLotId('');
      queryClient.invalidateQueries({ queryKey: ['moderation'] });
    } catch (err) {
      pushToast({
        kind: 'danger',
        title: 'Takedown failed',
        description: err instanceof ApiError ? err.detail ?? err.message : undefined,
      });
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <h3 className="text-sm font-semibold m-0 mb-3.5">Lot id</h3>
        <input
          type="text"
          value={lotId}
          onChange={(e) => setLotId(e.target.value)}
          placeholder="00000000-0000-0000-0000-000000000000"
          className="w-full rounded-md py-2 px-3 text-[13px] mono border bg-bg-soft transition focus:outline-none focus:border-accent focus:bg-surface"
          style={{ borderColor: 'transparent' }}
          aria-invalid={lotId.trim().length > 0 && !idIsValid}
        />
        {lotId.trim().length > 0 && !idIsValid && (
          <div className="mt-2 text-[12px]" style={{ color: 'var(--color-danger)' }}>
            Not a valid UUID.
          </div>
        )}
      </section>

      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <div className="flex justify-between items-baseline mb-3.5">
          <h3 className="text-sm font-semibold m-0">Takedown</h3>
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
          Soft-delete. The lot stays in Postgres for audit (with <code>DeletedAt</code> /
          <code> DeletedByUserId</code>) but disappears from public listings and Meilisearch within
          ~15 seconds (search-index flush job).
        </p>
        <div className="flex justify-end">
          <button
            type="button"
            onClick={onTakedown}
            disabled={!idIsValid || busy}
            className="rounded-md text-white font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed"
            style={{ background: 'var(--color-danger)', cursor: busy ? 'wait' : 'pointer' }}
          >
            {busy ? 'Taking down…' : 'Take down lot'}
          </button>
        </div>
      </section>

      <div className="flex gap-2.5 items-start text-[12px] text-text-3 px-1">
        <Icon name="info" size={14} color="var(--color-text-3)" />
        <span>
          A recent-takedowns log needs a backend listing endpoint — not built yet. Reach Meilisearch
          and Postgres directly via <code>/hangfire</code> to confirm the sync ran.
        </span>
      </div>
    </>
  );
}
