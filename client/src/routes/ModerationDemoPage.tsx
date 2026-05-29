import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { demo, useDemoModeEnabled } from '@/api/demo';
import { ApiError } from '@/api/fetch';
import { useToastStore } from '@/state/useToastStore';
import { Icon, type IconName } from '@/components/Icon';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/**
 * Admin-only demo control surface. After Option A every lot has exactly one Payment and one
 * Shipment, so the operator only needs the lot id — a single input drives all five short-circuit
 * actions. Each action targets a specific stage of the auction → payment → shipment workflow and
 * mutates the precondition just enough for the production job to proceed naturally.
 */
export default function ModerationDemoPage() {
  const { enabled, isLoading } = useDemoModeEnabled();

  if (isLoading) {
    return (
      <section className="bg-surface border border-border rounded-lg p-[22px]">
        <p className="text-[13px] text-text-3 m-0">Checking demo mode…</p>
      </section>
    );
  }

  if (!enabled) {
    return (
      <section className="bg-surface border border-border rounded-lg p-[22px]">
        <div className="flex gap-2.5 items-start">
          <Icon name="info" size={16} color="var(--color-warning)" />
          <div>
            <h3 className="text-sm font-semibold m-0 mb-1">Demo mode is off</h3>
            <p className="text-[12.5px] text-text-3 m-0 leading-relaxed">
              Set <code>DemoMode:Enabled = true</code> in <code>appsettings.json</code> on the
              server to expose the short-circuit endpoints. Production environments keep this off
              by default.
            </p>
          </div>
        </div>
      </section>
    );
  }

  return <DemoControls />;
}

function DemoControls() {
  const [lotId, setLotId] = useState('');
  const [busy, setBusy] = useState<string | null>(null);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  const idIsValid = UUID_RE.test(lotId.trim());

  const run = async (action: DemoAction) => {
    if (!idIsValid) return;
    if (action.confirm && !window.confirm(action.confirm)) return;
    setBusy(action.key);
    try {
      await action.run(lotId.trim());
      pushToast({
        kind: 'success',
        title: action.successTitle,
        description: action.successBody,
      });
      queryClient.invalidateQueries({ queryKey: ['moderation'] });
    } catch (err) {
      pushToast({
        kind: 'danger',
        title: `${action.label} failed`,
        description: err instanceof ApiError ? err.detail ?? err.message : undefined,
      });
    } finally {
      setBusy(null);
    }
  };

  const groups: DemoActionGroup[] = [
    {
      title: 'Stage 1 — Auction close',
      description:
        'Sets Lot.EndsAt to a second ago and enqueues AuctionCloseJob. Needed even with the 1-minute minimum duration: the 5-min AntiSnipeWindow in PlaceBidHandler extends EndsAt on every bid placed close to the end, so a short demo auction with bids cannot end naturally on schedule.',
      actions: [ACTIONS.closeNow],
    },
    {
      title: 'Stage 2 — Payment lifecycle',
      description:
        'Two recurring sweeps scan PendingAuthorization rows on different cadences. The buttons mutate DueAt into the relevant window and trigger the sweep ad-hoc; the rest is real production logic.',
      actions: [ACTIONS.sendReminder, ACTIONS.cancelUnpaid],
    },
    {
      title: 'Stage 3 — Shipment lifecycle',
      description:
        'In production these transitions come from NovaPoshtaPollingJob (every 15 min). The buttons short-circuit that by writing the shipment status directly and enqueuing the same downstream job the poller would.',
      actions: [ACTIONS.forceDelivered, ACTIONS.forceReturned],
    },
  ];

  return (
    <>
      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <div className="flex gap-2.5 items-start mb-3.5">
          <Icon name="info" size={16} color="var(--color-accent-deep)" />
          <p className="text-[12.5px] text-text-3 m-0 leading-relaxed">
            Every action mutates one precondition (EndsAt / DueAt / Shipment.Status) just enough
            to satisfy the real job's guard, then enqueues the same job production runs. No bypass
            logic, no fake state — only the time delay is removed.
          </p>
        </div>

        <label
          htmlFor="demo-lot-id"
          className="block text-[11px] uppercase tracking-wider font-semibold text-text-3 mb-1.5"
        >
          Lot id
        </label>
        <input
          id="demo-lot-id"
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

      {groups.map((group, gi) => (
        <StageCard
          key={group.title}
          order={gi + 1}
          group={group}
          busy={busy}
          idIsValid={idIsValid}
          onRun={run}
        />
      ))}
    </>
  );
}

interface DemoAction {
  key: string;
  label: string;
  variant: 'primary' | 'danger';
  icon: IconName;
  confirm?: string;
  run: (lotId: string) => Promise<unknown>;
  successTitle: string;
  successBody: string;
}

interface DemoActionGroup {
  title: string;
  description: string;
  actions: DemoAction[];
}

/**
 * Per-stage card. Header + description on top, action buttons below — buttons inherit the same
 * lot-id input from the outer DemoControls. Disabling cascades from `idIsValid` so the controls
 * stay coherent when the operator clears the field.
 */
function StageCard({
  order,
  group,
  busy,
  idIsValid,
  onRun,
}: {
  order: number;
  group: DemoActionGroup;
  busy: string | null;
  idIsValid: boolean;
  onRun: (action: DemoAction) => void;
}) {
  return (
    <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
      <div className="flex items-baseline gap-2.5 mb-2">
        <span
          className="mono inline-flex items-center justify-center rounded-full font-semibold"
          style={{
            width: 22,
            height: 22,
            fontSize: 11,
            background: 'var(--color-accent-tint)',
            color: 'var(--color-accent-deep)',
          }}
        >
          {order}
        </span>
        <h3 className="text-sm font-semibold m-0">{group.title}</h3>
      </div>

      <p className="text-[12.5px] text-text-3 m-0 mb-3.5 leading-relaxed">{group.description}</p>

      <div className="flex flex-wrap gap-2 justify-end">
        {group.actions.map((a, i) => {
          const isPrimary = a.variant === 'primary' && i === 0;
          const className = isPrimary
            ? 'inline-flex items-center gap-2 rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed'
            : a.variant === 'danger'
              ? 'inline-flex items-center gap-2 rounded-md text-white font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed'
              : 'inline-flex items-center gap-2 rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-4 py-2 text-[13px] disabled:opacity-50 disabled:cursor-not-allowed';
          const inlineStyle =
            a.variant === 'danger' && !isPrimary
              ? { background: 'var(--color-danger)', cursor: busy === a.key ? 'wait' : 'pointer' }
              : { cursor: busy === a.key ? 'wait' : 'pointer' };

          return (
            <button
              key={a.key}
              type="button"
              onClick={() => onRun(a)}
              disabled={!idIsValid || busy !== null}
              className={className}
              style={inlineStyle as React.CSSProperties}
            >
              <Icon name={a.icon} size={14} color="currentColor" />
              {busy === a.key ? 'Running…' : a.label}
            </button>
          );
        })}
      </div>
    </section>
  );
}

/**
 * Canonical action definitions — single source of truth for copy + endpoint binding. Defined
 * outside the component so the closures stay referentially stable.
 */
const ACTIONS: Record<
  'closeNow' | 'sendReminder' | 'cancelUnpaid' | 'forceDelivered' | 'forceReturned',
  DemoAction
> = {
  closeNow: {
    key: 'close-now',
    label: 'Close auction now',
    variant: 'primary',
    icon: 'clock',
    confirm: 'Force-close this auction and pick the winning bid?',
    run: (id) => demo.closeLotNow(id),
    successTitle: 'Auction closed',
    successBody: 'AuctionCloseJob enqueued. Lot is now Sold/EndedNoSale; Payment row pre-created.',
  },
  sendReminder: {
    key: 'send-reminder',
    label: 'Send won-pay reminder',
    variant: 'primary',
    icon: 'bell',
    run: (id) => demo.sendPaymentReminderNow(id),
    successTitle: 'Reminder enqueued',
    successBody: "Payment.DueAt set to now+48h so it falls into PaymentReminderJob's window.",
  },
  cancelUnpaid: {
    key: 'cancel-unpaid',
    label: 'Trigger non-payment cancel',
    variant: 'danger',
    icon: 'info',
    confirm: 'Void the Stripe hold and end the lot as EndedNoSale (−10 TrustScore for buyer)?',
    run: (id) => demo.cancelUnpaidNow(id),
    successTitle: 'Cancel sweep triggered',
    successBody:
      'Payment.DueAt pushed to the past. NonPaymentCancelJob will void via Stripe and flip the lot to EndedNoSale.',
  },
  forceDelivered: {
    key: 'force-delivered',
    label: 'Force → Delivered',
    variant: 'primary',
    icon: 'package',
    confirm: 'Mark shipment as Delivered and capture the Stripe hold?',
    run: (id) => demo.forceShipmentDelivered(id),
    successTitle: 'Capture enqueued',
    successBody:
      'Shipment flipped to Delivered. CapturePaymentJob will charge the buyer card and credit the seller via Stripe Connect.',
  },
  forceReturned: {
    key: 'force-returned',
    label: 'Force → Returned',
    variant: 'danger',
    icon: 'truck',
    confirm: 'Mark shipment as Returned and void the Stripe hold?',
    run: (id) => demo.forceShipmentReturned(id),
    successTitle: 'Void enqueued',
    successBody:
      'Shipment flipped to Returned. CancelPaymentJob will release the hold back to the buyer.',
  },
};
