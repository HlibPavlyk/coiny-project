import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { useAuthStore } from '@/state/useAuthStore';
import { payments, type ConnectStatusResponse } from '@/api/payments';

/**
 * Lands here from Stripe's redirect. Polls /payments/connect/status every 2s for up to 20s
 * because the `account.updated` webhook can take a moment to arrive after the user finishes
 * the Stripe-hosted form. If it never flips, surface the remaining requirements list so the
 * user can re-open onboarding from the same page.
 */
type PollState =
  | { kind: 'polling' }
  | { kind: 'onboarded'; status: ConnectStatusResponse }
  | { kind: 'incomplete'; status: ConnectStatusResponse }
  | { kind: 'error'; message: string };

const POLL_INTERVAL_MS = 2_000;
const POLL_DEADLINE_MS = 20_000;

export default function SellerOnboardedPage() {
  const refreshAuth = useAuthStore((s) => s.refresh);
  const [state, setState] = useState<PollState>({ kind: 'polling' });

  useEffect(() => {
    let cancelled = false;
    const startedAt = Date.now();

    async function tick() {
      try {
        const status = await payments.connectStatus();
        if (cancelled) return;

        if (status.stripeOnboarded) {
          // Keep the auth store in sync so the rest of the app reflects the flip.
          await refreshAuth();
          if (!cancelled) setState({ kind: 'onboarded', status });
          return;
        }

        if (Date.now() - startedAt >= POLL_DEADLINE_MS) {
          if (!cancelled) setState({ kind: 'incomplete', status });
          return;
        }

        // Still pending — schedule another poll.
        setTimeout(() => {
          if (!cancelled) void tick();
        }, POLL_INTERVAL_MS);
      } catch (err) {
        if (cancelled) return;
        const msg = err instanceof Error ? err.message : 'Could not verify onboarding status.';
        setState({ kind: 'error', message: msg });
      }
    }

    void tick();
    return () => {
      cancelled = true;
    };
  }, [refreshAuth]);

  return (
    <div>
      <TopNav />
      <div className="max-w-[640px] mx-auto px-7 py-16">
        {state.kind === 'polling' && (
          <>
            <h1 className="text-3xl font-bold m-0">Verifying onboarding…</h1>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              We&apos;re waiting for Stripe to confirm your details. This usually takes a few
              seconds.
            </p>
            <div className="mt-6 rounded-lg border border-border bg-surface p-6 flex items-center gap-3">
              <div className="w-3 h-3 rounded-full bg-accent animate-pulse" />
              <span className="text-[13.5px] text-text-2">Polling Stripe…</span>
            </div>
          </>
        )}

        {state.kind === 'onboarded' && (
          <>
            <h1 className="text-3xl font-bold m-0">You&apos;re onboarded 🎉</h1>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Stripe is connected, your payout details are on file, and Coiny is ready to accept
              buyer funds on your behalf. Funds settle to your bank after delivery is confirmed by
              Nova Poshta.
            </p>
            <div className="mt-6 flex gap-2.5">
              <Link
                to="/lots/new"
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
              >
                Create your first lot
              </Link>
              <Link
                to="/profile"
                className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm no-underline"
              >
                Back to profile
              </Link>
            </div>
          </>
        )}

        {state.kind === 'incomplete' && (
          <>
            <h1 className="text-3xl font-bold m-0">Onboarding incomplete</h1>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Stripe still needs a few details from you. You can resume onboarding from where you
              left off — your progress is saved.
            </p>
            {state.status.requirementsRemaining.length > 0 && (
              <div className="mt-6 rounded-lg border border-border bg-surface p-6">
                <div className="text-[13.5px] font-semibold mb-2.5">Still required</div>
                <ul className="text-text-2 text-[13px] leading-relaxed list-disc pl-5 space-y-1 mono">
                  {state.status.requirementsRemaining.map((r) => (
                    <li key={r}>{r}</li>
                  ))}
                </ul>
              </div>
            )}
            <div className="mt-6 flex gap-2.5">
              <Link
                to="/seller/onboarding"
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
              >
                Resume onboarding
              </Link>
              <Link
                to="/profile"
                className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm no-underline"
              >
                Back to profile
              </Link>
            </div>
          </>
        )}

        {state.kind === 'error' && (
          <>
            <h1 className="text-3xl font-bold m-0">Couldn&apos;t verify</h1>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              We hit a snag checking your onboarding status: {state.message}. Try refreshing the
              page, or come back to the onboarding flow.
            </p>
            <Link
              to="/seller/onboarding"
              className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline mt-6"
            >
              Back to onboarding
            </Link>
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}
