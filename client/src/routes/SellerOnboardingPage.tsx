import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
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

  // Already onboarded → no API call, just show the celebration CTA.
  const alreadyOnboarded = user?.stripeOnboarded === true;

  useEffect(() => {
    if (alreadyOnboarded) return;
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
  }, [alreadyOnboarded, pushToast]);

  return (
    <div>
      <TopNav />
      <div className="max-w-[640px] mx-auto px-7 py-16">
        <h1 className="text-3xl font-bold m-0">Become a seller</h1>

        {alreadyOnboarded ? (
          <div className="mt-6 rounded-lg border border-border bg-surface p-6">
            <div className="text-[14.5px] font-semibold mb-1.5">You&apos;re already onboarded</div>
            <p className="text-text-2 text-[13.5px] leading-relaxed">
              Stripe payouts are connected to your account. You can start listing lots right now.
            </p>
            <Link
              to="/lots/new"
              className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-5"
            >
              Create a lot
            </Link>
          </div>
        ) : (
          <>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Coiny uses Stripe Connect Express to hold buyer funds in escrow and pay you out after
              Nova Poshta confirms delivery. Onboarding takes 2 minutes — you&apos;ll provide your
              name, address, date of birth, and a bank account.
            </p>

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
                disabled={loading || !onboardingUrl}
                onClick={() => {
                  if (onboardingUrl) window.location.href = onboardingUrl;
                }}
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm mt-5 disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {loading ? 'Preparing your link…' : 'Continue to Stripe'}
              </button>
            </div>
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}
