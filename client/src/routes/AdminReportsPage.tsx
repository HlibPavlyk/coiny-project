import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { LotImagePlaceholder } from '@/components/LotImagePlaceholder';
import { ConfirmModal, type ModalAction } from '@/components/ConfirmModal';
import { admin, useReports, type ReportItemModel, type ReportStatus } from '@/api/admin';
import { lots } from '@/api/lots';
import { ApiError } from '@/api/fetch';
import { useToastStore } from '@/state/useToastStore';

type Tab = ReportStatus | 'All';

const TABS: { id: Tab; label: string }[] = [
  { id: 'Open', label: 'Open' },
  { id: 'Dismissed', label: 'Dismissed' },
  { id: 'ActionTaken', label: 'Action taken' },
  { id: 'All', label: 'All' },
];

type ActiveModal = { type: 'dismiss' | 'takeAction'; report: ReportItemModel } | null;

function humanizeReason(reason: string): string {
  return reason.replace(/([A-Z])/g, ' $1').trim();
}

function timeAgo(iso: string): string {
  const diffMs = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diffMs / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.floor(hours / 24)}d ago`;
}

const STATUS_COLORS: Record<ReportStatus, { bg: string; color: string }> = {
  Open: { bg: '#FEF3C7', color: '#92400E' },
  Dismissed: { bg: 'var(--color-bg-soft)', color: 'var(--color-text-2)' },
  ActionTaken: { bg: '#DCFCE7', color: '#166534' },
};

const GRID = '44px minmax(0, 1.6fr) 110px minmax(0, 1fr) 80px 96px 188px';

export default function AdminReportsPage() {
  const [tab, setTab] = useState<Tab>('Open');
  const [modal, setModal] = useState<ActiveModal>(null);
  const [note, setNote] = useState('');
  const [busy, setBusy] = useState(false);
  const queryClient = useQueryClient();
  const pushToast = useToastStore((s) => s.push);

  const { data, isLoading } = useReports({
    offset: 0,
    count: 50,
    sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
    filters: tab === 'All' ? {} : { status: tab },
  });

  const closeModal = () => {
    setModal(null);
    setNote('');
  };

  /** Run a mutation with toast + invalidate. A 502 (partial Stripe cancel) is surfaced as a warning,
   *  not a hard failure, because the ban/delete itself did commit. */
  const run = async (fn: () => Promise<void>, successTitle: string) => {
    setBusy(true);
    try {
      await fn();
      pushToast({ kind: 'success', title: successTitle });
      closeModal();
      queryClient.invalidateQueries({ queryKey: ['admin', 'reports'] });
    } catch (err) {
      if (err instanceof ApiError && err.status === 502) {
        pushToast({ kind: 'warning', title: 'Partially completed', description: err.detail ?? err.message });
        closeModal();
        queryClient.invalidateQueries({ queryKey: ['admin', 'reports'] });
      } else {
        pushToast({
          kind: 'danger',
          title: 'Action failed',
          description: err instanceof ApiError ? err.detail ?? err.message : undefined,
        });
      }
    } finally {
      setBusy(false);
    }
  };

  const dismissActions = (report: ReportItemModel): ModalAction[] => [
    { label: 'Dismiss report', onClick: () => run(() => admin.dismissReport(report.id, note.trim() || undefined), 'Report dismissed') },
  ];

  const takeActionActions = (report: ReportItemModel): ModalAction[] => [
    {
      label: 'Take down lot',
      danger: true,
      onClick: () =>
        run(async () => {
          await admin.takeAction(report.id, note.trim());
          await admin.takedownLot(report.lot.id);
        }, 'Lot taken down · report resolved'),
    },
    {
      label: 'Ban seller',
      danger: true,
      onClick: () =>
        run(async () => {
          // The report carries the lot but not its seller — resolve the seller from lot detail first.
          const detail = await lots.getLot(report.lot.id);
          await admin.takeAction(report.id, note.trim());
          await admin.banUser(detail.seller.id, note.trim());
        }, 'Seller banned · report resolved'),
    },
  ];

  const items = data?.items ?? [];

  return (
    <div>
      <TopNav />
      <div className="max-w-[1180px] mx-auto px-7 pt-8 pb-16">
        <h1 className="text-[28px] font-bold m-0 mb-5">Reports</h1>

        <div className="border-b border-border flex gap-1 mb-2">
          {TABS.map((t) => {
            const active = tab === t.id;
            return (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className="px-4 py-2.5 text-[13px] font-medium"
                style={{
                  color: active ? 'var(--color-accent-deep)' : 'var(--color-text-3)',
                  borderBottom: active ? '2px solid var(--color-accent)' : '2px solid transparent',
                  marginBottom: -1,
                  background: 'transparent',
                  cursor: 'pointer',
                }}
              >
                {t.label}
              </button>
            );
          })}
        </div>

        <div className="bg-surface border border-border rounded-lg overflow-hidden">
          {/* Header */}
          <div
            className="grid items-center gap-3 px-4 py-2.5 border-b border-border text-[11px] uppercase tracking-wider font-semibold text-text-3"
            style={{ gridTemplateColumns: GRID }}
          >
            <span />
            <span>Lot · reporter</span>
            <span>Reason</span>
            <span>Note</span>
            <span>Age</span>
            <span>Status</span>
            <span className="text-right">Actions</span>
          </div>

          {isLoading ? (
            <div className="py-10 text-center text-text-3 text-sm">Loading…</div>
          ) : items.length === 0 ? (
            <div className="py-12 text-center text-text-3 text-sm">No reports here.</div>
          ) : (
            items.map((report) => (
              <div
                key={report.id}
                className="grid items-center gap-3 px-4 py-3 border-b border-border-soft last:border-b-0"
                style={{ gridTemplateColumns: GRID }}
              >
                <div className="relative w-10 h-10 rounded bg-bg-soft overflow-hidden">
                  {report.lot.coverImageUrl ? (
                    <img src={report.lot.coverImageUrl} alt="" className="w-full h-full object-cover" />
                  ) : (
                    <LotImagePlaceholder kind="coin" variant={report.lot.id.charCodeAt(0) % 6} />
                  )}
                </div>

                <div className="min-w-0">
                  <Link
                    to={`/lot/${report.lot.id}`}
                    className="text-[13px] font-medium text-text hover:text-accent-deep no-underline truncate block"
                  >
                    {report.lot.title}
                  </Link>
                  <div className="text-[11px] text-text-3 mt-0.5 truncate">
                    {report.reporterDisplayName ?? (report.reporterIp ? `IP ${report.reporterIp}` : 'Anonymous')}
                  </div>
                </div>

                <div className="text-[12px] text-text-2">{humanizeReason(report.reason)}</div>

                <div className="text-[12px] text-text-3 truncate" title={report.note ?? undefined}>
                  {report.note || '—'}
                </div>

                <div className="text-[12px] text-text-3">{timeAgo(report.createdAt)}</div>

                <div>
                  <span
                    className="inline-block rounded-full font-semibold text-[10px]"
                    style={{
                      padding: '3px 9px',
                      background: STATUS_COLORS[report.status].bg,
                      color: STATUS_COLORS[report.status].color,
                      letterSpacing: '0.03em',
                    }}
                  >
                    {report.status === 'ActionTaken' ? 'Action taken' : report.status}
                  </span>
                </div>

                <div className="flex gap-1.5 justify-end">
                  {report.status === 'Open' ? (
                    <>
                      <button
                        type="button"
                        onClick={() => {
                          setNote('');
                          setModal({ type: 'dismiss', report });
                        }}
                        className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-3 py-1.5 text-[12px]"
                        style={{ cursor: 'pointer' }}
                      >
                        Dismiss
                      </button>
                      <button
                        type="button"
                        onClick={() => {
                          setNote('');
                          setModal({ type: 'takeAction', report });
                        }}
                        className="rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3 py-1.5 text-[12px]"
                        style={{ cursor: 'pointer' }}
                      >
                        Take action
                      </button>
                    </>
                  ) : (
                    <span className="text-[11px] text-text-3">
                      {report.resolvedAt ? timeAgo(report.resolvedAt) : ''}
                    </span>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      </div>
      <Footer />

      <ConfirmModal
        open={modal?.type === 'dismiss'}
        title="Dismiss report"
        description="Mark this report as reviewed with no further action."
        note={{ value: note, onChange: setNote, label: 'Resolution note (optional)', placeholder: 'Why is this dismissed?' }}
        actions={modal?.type === 'dismiss' ? dismissActions(modal.report) : []}
        onClose={closeModal}
        busy={busy}
      />

      <ConfirmModal
        open={modal?.type === 'takeAction'}
        title="Take action"
        description="Records the resolution, then performs the action. Deleting the lot hides it everywhere; banning the seller also cancels their active lots and in-flight payments."
        note={{ value: note, onChange: setNote, label: 'Resolution note', placeholder: 'Reason for the action (required)', required: true }}
        actions={modal?.type === 'takeAction' ? takeActionActions(modal.report) : []}
        onClose={closeModal}
        busy={busy}
      />
    </div>
  );
}
