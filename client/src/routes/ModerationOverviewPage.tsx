import { Link } from 'react-router-dom';
import { useReports } from '@/api/moderation';
import { Icon } from '@/components/Icon';

function StatTile({ label, value, sub }: { label: string; value: string | number; sub?: string }) {
  return (
    <div className="flex-1 min-w-0">
      <div className="text-[11px] uppercase tracking-wider font-semibold text-text-3">{label}</div>
      <div className="mono text-[22px] font-bold mt-1" style={{ letterSpacing: '-0.01em' }}>
        {value}
      </div>
      {sub && <div className="text-[11.5px] text-text-3 mt-0.5">{sub}</div>}
    </div>
  );
}

export default function ModerationOverviewPage() {
  // Three lightweight count-only queries: each fetches a single row just to read `.total`. React
  // Query caches them independently from the reports table on /moderation/reports.
  const { data: open } = useReports({
    offset: 0,
    count: 1,
    sortBy: [{ columnName: 'createdAt', direction: 'Desc' }],
    filters: { status: 'Open' },
  });
  const { data: resolved } = useReports({
    offset: 0,
    count: 1,
    sortBy: [{ columnName: 'resolvedAt', direction: 'Desc' }],
    filters: { status: 'ActionTaken' },
  });
  const { data: dismissed } = useReports({
    offset: 0,
    count: 1,
    sortBy: [{ columnName: 'resolvedAt', direction: 'Desc' }],
    filters: { status: 'Dismissed' },
  });

  return (
    <>
      <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
        <div className="flex gap-4">
          <StatTile label="Open reports" value={open?.totalCount ?? '—'} sub="needs review" />
          <StatTile label="Action taken" value={resolved?.totalCount ?? '—'} sub="lifetime" />
          <StatTile label="Dismissed" value={dismissed?.totalCount ?? '—'} sub="lifetime" />
        </div>
      </section>

      <section className="bg-surface border border-border rounded-lg p-[22px]">
        <div className="flex justify-between items-baseline mb-3.5">
          <h3 className="text-sm font-semibold m-0">Where to start</h3>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <Link
            to="/moderation/reports"
            className="border border-border rounded-md p-4 no-underline text-text transition hover:border-border-strong hover:bg-bg-soft"
          >
            <div className="flex items-center gap-2.5 mb-1">
              <Icon name="info" size={16} color="var(--color-accent-deep)" />
              <span className="text-[14px] font-semibold">Reports queue</span>
            </div>
            <p className="text-[12.5px] text-text-3 m-0 leading-relaxed">
              Dismiss or take action on reports filed by buyers. Action records the decision and runs
              the takedown or ban.
            </p>
          </Link>

          <Link
            to="/moderation/users"
            className="border border-border rounded-md p-4 no-underline text-text transition hover:border-border-strong hover:bg-bg-soft"
          >
            <div className="flex items-center gap-2.5 mb-1">
              <Icon name="user" size={16} color="var(--color-accent-deep)" />
              <span className="text-[14px] font-semibold">Users</span>
            </div>
            <p className="text-[12.5px] text-text-3 m-0 leading-relaxed">
              Ban or unban by user id (outside of a report). Banning cancels active lots and
              in-flight payments.
            </p>
          </Link>

          <Link
            to="/moderation/lots"
            className="border border-border rounded-md p-4 no-underline text-text transition hover:border-border-strong hover:bg-bg-soft"
          >
            <div className="flex items-center gap-2.5 mb-1">
              <Icon name="cards" size={16} color="var(--color-accent-deep)" />
              <span className="text-[14px] font-semibold">Lots</span>
            </div>
            <p className="text-[12.5px] text-text-3 m-0 leading-relaxed">
              Take a lot down by id. Soft-delete — the lot stays in Postgres for audit but is
              removed from public listings and search.
            </p>
          </Link>
        </div>
      </section>
    </>
  );
}
