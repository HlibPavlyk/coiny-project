import { Link } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { Icon } from '@/components/Icon';

const HANGFIRE_URL = `${import.meta.env.VITE_API_BASE_URL || ''}/hangfire`;

export default function AdminLandingPage() {
  return (
    <div>
      <TopNav />
      <div className="max-w-[1080px] mx-auto px-7 pt-8 pb-16">
        <h1 className="text-[28px] font-bold m-0 mb-1">Moderation</h1>
        <p className="text-[14px] text-text-3 mt-0 mb-6">Review reports and act on lots and users.</p>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Link
            to="/admin/reports"
            className="bg-surface border border-border rounded-lg p-5 no-underline text-text transition hover:-translate-y-px hover:border-border-strong"
          >
            <div className="flex items-center gap-2.5 mb-1.5">
              <Icon name="info" size={18} color="var(--color-accent-deep)" />
              <span className="text-[16px] font-semibold">Reports</span>
            </div>
            <p className="text-[13px] text-text-3 m-0">Open reports, dismiss or take action (delete lot / ban seller).</p>
          </Link>

          <a
            href={HANGFIRE_URL}
            target="_blank"
            rel="noreferrer"
            className="bg-surface border border-border rounded-lg p-5 no-underline text-text transition hover:-translate-y-px hover:border-border-strong"
          >
            <div className="flex items-center gap-2.5 mb-1.5">
              <Icon name="external" size={18} color="var(--color-accent-deep)" />
              <span className="text-[16px] font-semibold">Background jobs</span>
            </div>
            <p className="text-[13px] text-text-3 m-0">Hangfire dashboard — inspect recurring and failed jobs.</p>
          </a>
        </div>
      </div>
      <Footer />
    </div>
  );
}
