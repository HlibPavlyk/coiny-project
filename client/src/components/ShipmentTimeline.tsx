import { useQuery } from '@tanstack/react-query';
import { shipments, TERMINAL_SHIPMENT_STATUSES } from '@/api/shipments';
import type { ShipmentStatus } from '@/api/payments';

interface Props {
  paymentId: string;
}

const STATUS_LABEL: Record<ShipmentStatus, string> = {
  PendingTtn: 'Awaiting waybill',
  TtnCreated: 'Waybill created',
  AcceptedByCarrier: 'Accepted by carrier',
  InTransit: 'In transit',
  ArrivedAtDestination: 'Arrived at destination',
  Delivered: 'Delivered',
  Refused: 'Refused',
  Returned: 'Returned',
  Lost: 'Lost',
};

/**
 * Vertical event list for a shipment. Auto-refetches every 30s while the shipment is
 * not in a terminal state (Delivered / Refused / Returned / Lost) so the buyer sees
 * the latest NP poll without manual reload.
 */
export function ShipmentTimeline({ paymentId }: Props) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['shipment', paymentId],
    queryFn: () => shipments.getByPaymentId(paymentId),
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      // Stop polling once shipment reaches a terminal state.
      if (status && TERMINAL_SHIPMENT_STATUSES.includes(status)) return false;
      return 30_000;
    },
    staleTime: 15_000,
  });

  if (isLoading) {
    return <div className="text-[13px] text-text-3">Loading shipment…</div>;
  }
  if (error) {
    return (
      <div className="text-[13px] text-text-3">
        Could not load shipment status. We&apos;ll try again in 30s.
      </div>
    );
  }
  if (!data) return null;

  return (
    <div>
      {/* Header: recipient + TTN */}
      <div className="rounded-md border border-border bg-bg-soft p-3.5">
        <div className="grid gap-2 text-[12.5px] grid-cols-[100px_1fr] sm:grid-cols-[120px_1fr]">
          <div className="text-text-3">Status</div>
          <div className="font-semibold">{STATUS_LABEL[data.status]}</div>

          <div className="text-text-3">Recipient</div>
          <div>
            {data.recipientName} · <span className="mono">{data.recipientPhone}</span>
          </div>

          <div className="text-text-3">Address</div>
          <div>
            {data.recipientCityLabel} → {data.recipientWarehouseLabel}
          </div>

          {data.novaPoshtaTtn && (
            <>
              <div className="text-text-3">Nova Poshta TTN</div>
              <div className="mono">{data.novaPoshtaTtn}</div>
            </>
          )}
        </div>
      </div>

      {/* Timeline */}
      {data.events.length === 0 ? (
        <div className="text-[12.5px] text-text-3 mt-4">No status events yet — first NP poll will arrive shortly.</div>
      ) : (
        <ol className="mt-4 relative pl-5">
          {data.events.map((evt, idx) => {
            const isLast = idx === data.events.length - 1;
            return (
              <li key={evt.id} className="relative pb-4 last:pb-0">
                {/* connector line + dot */}
                <span
                  aria-hidden
                  className="absolute left-[-14px] top-1.5 w-2.5 h-2.5 rounded-full bg-accent"
                />
                {!isLast && (
                  <span
                    aria-hidden
                    className="absolute left-[-9px] top-4 bottom-0 w-px bg-border"
                  />
                )}
                <div className="text-[13px] font-semibold">{STATUS_LABEL[evt.status]}</div>
                <div className="text-[12px] text-text-3 mt-0.5">
                  <span className="mono">{new Date(evt.observedAt).toLocaleString('en-US')}</span>
                  {evt.description && <span> — {evt.description}</span>}
                </div>
              </li>
            );
          })}
        </ol>
      )}
    </div>
  );
}
