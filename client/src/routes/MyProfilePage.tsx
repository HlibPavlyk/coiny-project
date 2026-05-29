import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '@/state/useAuthStore';
import { useToastStore } from '@/state/useToastStore';
import { auth } from '@/api/auth';
import { payments } from '@/api/payments';
import { ApiError } from '@/api/fetch';
import { AvatarLarge } from '@/components/AvatarLarge';
import { VerificationStatusPill } from '@/components/VerificationStatusPill';
import { Icon } from '@/components/Icon';

function StatTile({ label, value, sub }: { label: string; value: string; sub?: string }) {
  return (
    <div className="flex-1 min-w-0">
      <div className="text-[10px] sm:text-[11px] uppercase tracking-wider font-semibold text-text-3 leading-tight">
        {label}
      </div>
      <div className="mono text-[17px] sm:text-[22px] font-bold mt-1 truncate" style={{ letterSpacing: '-0.01em' }}>
        {value}
      </div>
      {sub && <div className="text-[11.5px] text-text-3 mt-0.5 truncate">{sub}</div>}
    </div>
  );
}

function ProfileSection({
  title,
  action,
  children,
}: {
  title: string;
  action?: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <section className="bg-surface border border-border rounded-lg p-[22px] mb-3.5">
      <div className="flex justify-between items-baseline mb-3.5">
        <h3 className="text-sm font-semibold m-0">{title}</h3>
        {action}
      </div>
      {children}
    </section>
  );
}

export default function MyProfilePage() {
  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const [resending, setResending] = useState(false);
  const [dashboardLoading, setDashboardLoading] = useState(false);

  // Mint a single-use Stripe Express dashboard URL and open it in a new tab. Login links
  // expire fast (~5 min) and are single-use, so we re-fetch on every click rather than caching.
  const openStripeDashboard = async () => {
    setDashboardLoading(true);
    try {
      const r = await payments.expressDashboardLink();
      window.open(r.url, '_blank', 'noopener,noreferrer');
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Could not open Stripe dashboard.';
      pushToast({ kind: 'danger', title: 'Stripe dashboard unavailable', description: msg });
    } finally {
      setDashboardLoading(false);
    }
  };

  if (!user) return null; // RequireAuth handles redirect

  const initials = (user.displayName || user.email).slice(0, 2).toUpperCase();
  const memberSince = 'May 2026';

  const onResend = async () => {
    setResending(true);
    try {
      await auth.resendVerification();
      pushToast({
        kind: 'success',
        title: 'Verification email sent',
        description: 'Check your inbox in a minute.',
      });
    } catch (err) {
      if (err instanceof ApiError && err.status === 429) {
        pushToast({
          kind: 'warning',
          title: 'Slow down',
          description: err.detail ?? 'Please wait before requesting another email.',
        });
      } else if (err instanceof ApiError && err.status === 409) {
        pushToast({ kind: 'info', title: 'Already verified' });
      } else {
        pushToast({ kind: 'danger', title: 'Could not send email' });
      }
    } finally {
      setResending(false);
    }
  };

  return (
    <>
      {/* Email verification block */}
            {!user.emailVerified && (
              <div
                className="rounded-lg p-4 mb-3.5 flex gap-3.5 items-start"
                style={{ border: '1px solid #FCD34D', background: 'var(--color-warning-soft)' }}
              >
                <div
                  className="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0"
                  style={{ background: '#FCD34D' }}
                >
                  <Icon name="info" size={18} color="#92400E" />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-semibold" style={{ color: '#92400E' }}>
                    Verify your email to start bidding
                  </div>
                  <div
                    className="text-[13px] mt-1 leading-relaxed"
                    style={{ color: '#78350F' }}
                  >
                    We sent a verification link to <strong>{user.email}</strong>. Check your inbox
                    (and spam folder). Without verification you can browse but not place bids.
                  </div>
                </div>
                <button
                  type="button"
                  onClick={onResend}
                  disabled={resending}
                  className="rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-1.5 text-xs font-medium disabled:opacity-60 disabled:cursor-not-allowed flex-shrink-0 whitespace-nowrap"
                >
                  {resending ? 'Sending…' : 'Resend verification email'}
                </button>
              </div>
            )}

            {/* Identity card. Mobile: avatar + name+email row, stats span full width below.
                Desktop: avatar | (name+email+stats) two-column. */}
            <section className="bg-surface border border-border rounded-lg p-4 sm:p-[22px] mb-3.5">
              <div className="flex flex-col sm:flex-row sm:gap-5 sm:items-center">
                <div className="flex items-center gap-3 sm:gap-5">
                  <AvatarLarge initials={initials} size={64} />
                  <div className="flex-1 min-w-0 sm:hidden">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="mono text-[17px] font-bold break-all">{user.displayName}</span>
                      <VerificationStatusPill verified={user.emailVerified} />
                    </div>
                    <div className="text-[12.5px] text-text-3 mt-0.5 break-all">{user.email}</div>
                  </div>
                </div>
                <div className="hidden sm:block flex-1 min-w-0">
                  <div className="flex items-center gap-2.5 flex-wrap">
                    <span className="mono text-[19px] font-bold break-all">{user.displayName}</span>
                    <VerificationStatusPill verified={user.emailVerified} />
                  </div>
                  <div className="text-[13.5px] text-text-3 mt-1 break-all">{user.email}</div>
                </div>
                <div className="grid grid-cols-3 gap-3 mt-4 sm:mt-3.5 pt-4 sm:pt-3.5 border-t border-border-soft sm:hidden">
                  <StatTile label="TrustScore" value={String(user.trustScore)} />
                  <StatTile label="Member since" value={memberSince} />
                  <StatTile
                    label="Roles"
                    value={String(user.roles.length)}
                    sub={user.roles.join(' · ')}
                  />
                </div>
              </div>
              <div className="hidden sm:flex gap-4 mt-3.5 pt-3.5 border-t border-border-soft">
                <StatTile label="TrustScore" value={String(user.trustScore)} />
                <StatTile label="Member since" value={memberSince} />
                <StatTile
                  label="Roles"
                  value={String(user.roles.length)}
                  sub={user.roles.join(' · ')}
                />
              </div>
            </section>

            {/* Stripe seller block */}
            {!user.stripeOnboarded ? (
              <ProfileSection
                title="Seller payouts"
                action={
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
                    Action required
                  </span>
                }
              >
                <div className="flex gap-3.5 items-center">
                  <div
                    className="w-11 h-11 rounded-md text-white flex items-center justify-center flex-shrink-0 font-bold mono"
                    style={{ background: '#635BFF', fontSize: 18 }}
                  >
                    S
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-[13.5px] font-semibold">
                      Connect Stripe to receive payouts
                    </div>
                    <div className="text-[12.5px] text-text-3 mt-0.5">
                      Required to publish lots. Funds released after buyer confirms delivery.
                    </div>
                  </div>
                  <Link
                    to="/seller/onboarding"
                    className="rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-3.5 py-1.5 text-xs transition no-underline"
                  >
                    Connect Stripe
                  </Link>
                </div>
              </ProfileSection>
            ) : (
              <ProfileSection
                title="Seller payouts"
                action={
                  <span
                    className="inline-flex items-center gap-1 font-semibold uppercase rounded-full"
                    style={{
                      fontSize: 11,
                      padding: '3px 8px',
                      background: 'var(--color-success-soft)',
                      color: '#166534',
                      letterSpacing: '0.04em',
                    }}
                  >
                    <Icon name="check" size={11} stroke={2.5} />
                    Connected
                  </span>
                }
              >
                <div className="text-[13px] text-text-2">
                  Stripe is connected. You can publish lots; funds settle after delivery.
                </div>
                <div className="mt-5 flex flex-wrap gap-2.5">
                  <Link
                    to="/lots/new"
                    className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
                  >
                    Create a lot
                  </Link>
                  <button
                    type="button"
                    onClick={openStripeDashboard}
                    disabled={dashboardLoading}
                    className="inline-flex items-center gap-1.5 rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm disabled:opacity-60 disabled:cursor-not-allowed"
                  >
                    <Icon name="external" size={14} stroke={2} />
                    {dashboardLoading ? 'Opening…' : 'Open Stripe dashboard'}
                  </button>
                </div>
              </ProfileSection>
            )}

            {/* Reviews placeholder */}
            <ProfileSection title="Reviews">
              <div className="text-center py-7">
                <div
                  className="w-14 h-14 mx-auto mb-3.5 rounded-full flex items-center justify-center"
                  style={{ background: 'var(--color-bg-soft)' }}
                >
                  <Icon name="star" size={26} color="var(--color-text-3)" stroke={1.4} />
                </div>
                <div className="text-[14.5px] font-semibold">Reviews coming soon</div>
                <div className="text-[13px] text-text-3 mt-1.5 max-w-[320px] mx-auto leading-relaxed">
                  Once we ship the reviews feature, your buyer & seller ratings will live here.
                </div>
              </div>
      </ProfileSection>
    </>
  );
}
