import { useEffect, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { CheckoutDetailsForm } from '@/components/CheckoutDetailsForm';
import { StripePaymentForm } from '@/components/StripePaymentForm';
import { useAuthStore } from '@/state/useAuthStore';
import { useToastStore } from '@/state/useToastStore';
import { payments, type PaymentDetailModel } from '@/api/payments';
import { ApiError } from '@/api/fetch';

/**
 * The buyer-facing payment flow.
 *
 * Route: `/my-purchases/:lotId/pay`. Lot id is in the path because at the start of the flow
 * the buyer doesn't yet have a payment id — the Payment row is created when the Stripe
 * intent is minted (StripePaymentForm). After Stripe's 3DS redirect the URL carries
 * `?paid=1&paymentId=…`, which switches us into the "confirming" state.
 *
 * State machine:
 *  - `loading`             initial — fetching shipment to see if checkout-details are already saved
 *  - `needs-details`       no shipment yet → show CheckoutDetailsForm
 *  - `needs-payment`       shipment staged → mount StripePaymentForm (creates the PaymentIntent)
 *  - `confirming`          back from Stripe with ?paid=1 → poll /payments/{id} until Authorized
 *  - `done`                Status = Authorized; render success card
 *  - `error`               unrecoverable; offer retry
 */
type ViewState =
  | { kind: 'loading' }
  | { kind: 'needs-details' }
  | { kind: 'needs-payment' }
  | { kind: 'confirming'; paymentId: string }
  | { kind: 'done'; payment: PaymentDetailModel }
  | { kind: 'error'; message: string };

const POLL_INTERVAL_MS = 2_000;
const POLL_DEADLINE_MS = 30_000;

export default function PayLotPage() {
  const { lotId = '' } = useParams<{ lotId: string }>();
  const [search] = useSearchParams();
  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const [state, setState] = useState<ViewState>({ kind: 'loading' });

  const paidParam = search.get('paid');
  const paymentIdParam = search.get('paymentId');

  // Entry point — decide initial state.
  useEffect(() => {
    if (paidParam === '1' && paymentIdParam) {
      setState({ kind: 'confirming', paymentId: paymentIdParam });
      return;
    }

    // No `paid=1` — check whether the buyer has already staged shipment details. We do
    // that by attempting to fetch any existing Payment for the lot via /my-bids or the
    // payment id; but since we only have lotId here, the simplest signal is: try to
    // /payments/{lotId}/intent and see if it 409s. That's destructive (creates an intent
    // on the first try). Instead, render the checkout-details form by default — if the
    // user has already submitted, the submit will 409 and we surface the error.
    setState({ kind: 'needs-details' });
  }, [paidParam, paymentIdParam]);

  // Polling loop for the `confirming` state.
  useEffect(() => {
    if (state.kind !== 'confirming') return;
    let cancelled = false;
    const startedAt = Date.now();
    const paymentId = state.paymentId;

    async function tick() {
      try {
        const p = await payments.getById(paymentId);
        if (cancelled) return;

        if (p.status === 'Authorized' || p.status === 'Captured') {
          setState({ kind: 'done', payment: p });
          return;
        }
        if (p.status === 'Cancelled' || p.status === 'Failed') {
          setState({
            kind: 'error',
            message: `Payment finalized as ${p.status}. Try again or contact support.`,
          });
          return;
        }

        if (Date.now() - startedAt >= POLL_DEADLINE_MS) {
          // Webhook may have been slow; show a "still pending" message but keep polling option.
          setState({
            kind: 'error',
            message:
              'We have not received confirmation from Stripe yet. Refresh in a minute or check My Purchases.',
          });
          return;
        }

        setTimeout(() => {
          if (!cancelled) void tick();
        }, POLL_INTERVAL_MS);
      } catch (err) {
        if (cancelled) return;
        const msg =
          err instanceof ApiError
            ? err.detail ?? err.message
            : err instanceof Error
              ? err.message
              : 'Could not verify payment status.';
        setState({ kind: 'error', message: msg });
      }
    }

    void tick();
    return () => {
      cancelled = true;
    };
  }, [state]);

  return (
    <div>
      <TopNav />
      <div className="max-w-[640px] mx-auto px-7 py-12">
        <h1 className="text-3xl font-bold m-0">Complete your purchase</h1>

        {state.kind === 'loading' && (
          <p className="text-text-3 mt-4">Loading…</p>
        )}

        {state.kind === 'needs-details' && (
          <>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Step 1 of 2 — tell us where to send the lot. Cities and branches are pulled live
              from Nova Poshta.
            </p>
            <div className="mt-6 rounded-lg border border-border bg-surface p-6">
              <CheckoutDetailsForm
                lotId={lotId}
                defaultName={user?.displayName ?? ''}
                onSubmitted={() => {
                  pushToast({ kind: 'success', title: 'Delivery details saved' });
                  setState({ kind: 'needs-payment' });
                }}
              />
            </div>
          </>
        )}

        {state.kind === 'needs-payment' && (
          <>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Step 2 of 2 — pay with a card. Funds are held by Stripe in escrow and released to
              the seller after Nova Poshta confirms delivery.
            </p>
            <div className="mt-6 rounded-lg border border-border bg-surface p-6">
              <StripePaymentForm
                lotId={lotId}
                returnUrl={`${window.location.origin}/my-purchases/${lotId}/pay?paid=1`}
              />
            </div>
          </>
        )}

        {state.kind === 'confirming' && (
          <>
            <p className="text-text-2 text-[14.5px] mt-3 leading-relaxed">
              Confirming your payment with Stripe…
            </p>
            <div className="mt-6 rounded-lg border border-border bg-surface p-6 flex items-center gap-3">
              <div className="w-3 h-3 rounded-full bg-accent animate-pulse" />
              <span className="text-[13.5px] text-text-2">Waiting for Stripe webhook…</span>
            </div>
          </>
        )}

        {state.kind === 'done' && (
          <>
            <h2 className="text-[20px] font-semibold mt-6">Payment confirmed 🎉</h2>
            <p className="text-text-2 text-[14.5px] mt-2 leading-relaxed">
              Funds are held in escrow. We&apos;ll create the Nova Poshta waybill shortly and
              email you the tracking number. After Nova Poshta marks delivery, Stripe releases the
              funds to the seller.
            </p>
            <div className="mt-6 rounded-lg border border-border bg-surface p-6">
              <div className="text-[12px] uppercase tracking-wider font-semibold text-text-3">
                Payment summary
              </div>
              <div className="mt-2 text-[14px] space-y-1">
                <div>
                  Amount: <span className="mono font-semibold">{(state.payment.amountUahKopiykas / 100).toLocaleString('en-US', { minimumFractionDigits: 2 })} UAH</span>
                </div>
                <div className="text-text-3 text-[12.5px]">
                  Stripe intent: <span className="mono">{state.payment.stripePaymentIntentId}</span>
                </div>
              </div>
            </div>
            <div className="mt-6 flex gap-2.5">
              <Link
                to="/my-purchases"
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm no-underline"
              >
                Go to My Purchases
              </Link>
              <Link
                to="/"
                className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm no-underline"
              >
                Back to home
              </Link>
            </div>
          </>
        )}

        {state.kind === 'error' && (
          <>
            <h2 className="text-[20px] font-semibold mt-6">Couldn&apos;t finish payment</h2>
            <p className="text-text-2 text-[14.5px] mt-2 leading-relaxed">{state.message}</p>
            <div className="mt-6 flex gap-2.5">
              <button
                type="button"
                onClick={() => setState({ kind: 'needs-details' })}
                className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm"
              >
                Try again
              </button>
              <Link
                to="/my-purchases"
                className="inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft font-medium px-5 py-3 text-sm no-underline"
              >
                My Purchases
              </Link>
            </div>
          </>
        )}
      </div>
      <Footer />
    </div>
  );
}
