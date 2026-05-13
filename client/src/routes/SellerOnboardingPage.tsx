import { useEffect, useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { useAuthStore } from '@/state/useAuthStore';
import { useToastStore } from '@/state/useToastStore';
import { payments } from '@/api/payments';

export default function SellerOnboardingPage() {
  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const [loading, setLoading] = useState(false);
  const [onboardingUrl, setOnboardingUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Already onboarded — there's nothing to do here; the profile's "Seller payouts" section
  // is the single source of truth for Stripe-dashboard access and listing creation. We can't
  // delete this route outright because Stripe's `RefreshUrl` in appsettings.json points at it,
  // but for already-onboarded users we forward to /profile to avoid duplicate UI.
  const alreadyOnboarded = user?.stripeOnboarded === true;
  // Stripe Connect ties the financial account to this email — onboarding for an unverified
  // address is blocked server-side in ConnectOnboardHandler. We mirror the rule here so the
  // user sees a self-explanatory panel + "Verify email" CTA instead of a generic error toast.
  const emailVerified = user?.emailVerified === true;

  useEffect(() => {
    if (alreadyOnboarded || !emailVerified) return;
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError(null);
      try {
        const r = await payments.connectOnboard();
        if (!cancelled) setOnboardingUrl(r.onboardingUrl);
      } catch (err) {
        if (cancelled) return;
        const msg = err instanceof Error ? err.message : 'Could not start onboarding.';
        setError(msg);
        pushToast({ kind: 'danger', title: 'Onboarding failed', description: msg });
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [alreadyOnboarded, emailVerified, pushToast]);

  // Already-onboarded users get the same actions (open dashboard, create lot) on /profile —
  // forward there so we don't render two parallel surfaces for the same state.
  if (alreadyOnboarded) {
    return <Navigate to="/profile" replace />;
  }

  return (
    <div>
      <TopNav />
      <div className="max-w-[640px] mx-auto px-7 py-16">
        <h1 className="text-3xl font-bold m-0">Become a seller</h1>

        <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
          Coiny uses Stripe Connect Express to hold buyer funds in escrow and pay you out after
          Nova Poshta confirms delivery. Onboarding takes 2 minutes — you&apos;ll provide your
          name, address, date of birth, and a bank account.
        </p>

        {!emailVerified && (
          <div
            className="mt-6 rounded-lg border p-5"
            style={{
              background: 'var(--color-warning-soft, #FEF3C7)',
              borderColor: 'var(--color-warning, #F59E0B)',
            }}
          >
            <div className="text-[14px] font-semibold" style={{ color: '#78350F' }}>
              Verify your email first
            </div>
            <p className="text-[13px] leading-relaxed mt-1.5" style={{ color: '#92400E' }}>
              Stripe Connect creates a financial account tied to <span className="mono">{user?.email}</span>.
              Verify your email so payout notifications and account-recovery requests land in
              an inbox you control.
            </p>
            <Link
              to="/verify-email"
              className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-2 text-[13px] no-underline mt-3.5"
            >
              Verify email
            </Link>
          </div>
        )}

        <div className="mt-6 rounded-lg border border-border bg-surface p-6">
          <div className="text-[13.5px] font-semibold mb-1.5">What happens next</div>
          <ol className="text-text-2 text-[13px] leading-relaxed list-decimal pl-5 space-y-1">
            <li>You&apos;ll be redirected to a Stripe-hosted form.</li>
            <li>Stripe verifies your identity and bank details.</li>
            <li>Stripe redirects you back to Coiny when done.</li>
          </ol>

          {error && (
            <div
              className="mt-5 rounded-md p-3 text-[13px]"
              style={{ background: 'var(--color-danger-soft)', color: '#7F1D1D' }}
            >
              {error}
            </div>
          )}

          <button
            type="button"
            disabled={!emailVerified || loading || !onboardingUrl}
            onClick={() => {
              if (onboardingUrl) window.location.href = onboardingUrl;
            }}
            className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm mt-5 disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {!emailVerified
              ? 'Verify email to continue'
              : loading
                ? 'Preparing your link…'
                : 'Continue to Stripe'}
          </button>
        </div>
      </div>
      <Footer />
    </div>
  );
}
