import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { Footer } from '@/components/Footer';
import { CheckoutDetailsForm } from '@/components/CheckoutDetailsForm';
import { StripePaymentForm } from '@/components/StripePaymentForm';
import { Skeleton, SkeletonLine } from '@/components/Skeleton';
import { Icon } from '@/components/Icon';
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
  // `code` carries the server's Error.Code (e.g. "Lot.NotWinner") so ErrorCard can route to the
  // right copy + CTAs. `message` is the human-readable fallback when no code matches.
  | { kind: 'error'; message: string; code?: string };

const POLL_INTERVAL_MS = 2_000;
const POLL_DEADLINE_MS = 30_000;

export default function PayLotPage() {
  const { lotId = '' } = useParams<{ lotId: string }>();
  const [search] = useSearchParams();
  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const navigate = useNavigate();
  const [state, setState] = useState<ViewState>({ kind: 'loading' });

  const paidParam = search.get('paid');
  const paymentIdParam = search.get('paymentId');
  // Stripe always appends `redirect_status` on its return — we use it as a fallback
  // signal when our own paymentId is missing (e.g., older intents minted before the
  // returnUrl carried it). In that case the buyer can't be polled here, so we punt
  // to /my-purchases where the row is listed with its actual status.
  const stripeRedirectStatus = search.get('redirect_status');

  // Entry point — decide initial state based on server-side progress, not just the URL.
  //
  // Why a state probe: a returning buyer (closed the tab, clicked "pay" again from My Purchases
  // or the Lot detail page) used to be force-marched through the delivery form a second time,
  // only to hit a "payment already in progress" dead-end on step 2. We now ask the server what
  // already exists and jump straight to the right step.
  useEffect(() => {
    if (paidParam === '1' && paymentIdParam) {
      setState({ kind: 'confirming', paymentId: paymentIdParam });
      return;
    }

    // Stripe redirected back but we have no paymentId in the URL (legacy intent with
    // a returnUrl that didn't carry it). The Payment row exists server-side; the row
    // on /my-purchases is the source of truth.
    if (paidParam === '1' && stripeRedirectStatus) {
      navigate('/my-purchases', { replace: true });
      return;
    }

    let cancelled = false;
    (async () => {
      try {
        const probe = await payments.getLotPaymentState(lotId);
        if (cancelled) return;

        // No shipment yet → step 1.
        if (!probe.shipmentExists) {
          setState({ kind: 'needs-details' });
          return;
        }

        // Shipment exists, no Payment row → step 2 (StripePaymentForm will mint the intent).
        if (!probe.payment) {
          setState({ kind: 'needs-payment' });
          return;
        }

        // Payment exists — branch on its terminal-ness.
        switch (probe.payment.status) {
          case 'PendingAuthorization':
            // Buyer started but didn't finish the card step. Step 2 will re-fetch the existing
            // intent via the now-idempotent createIntent endpoint and remount Elements.
            setState({ kind: 'needs-payment' });
            return;
          case 'Authorized':
          case 'Captured':
            // Already paid — flip into the polling state which will fetch the row and render
            // the "Payment confirmed" success card.
            setState({ kind: 'confirming', paymentId: probe.payment.id });
            return;
          case 'Cancelled':
          case 'Failed':
            setState({
              kind: 'error',
              code: `Payment.${probe.payment.status}`,
              message: `This payment is in ${probe.payment.status}. Contact support to restart.`,
            });
            return;
        }
      } catch (err) {
        if (cancelled) return;
        // Lot not Sold, caller isn't the winner, or auth issue. ErrorCard maps the code to a
        // tailored title + CTAs; we still ship the server's detail as the fallback message.
        const code = err instanceof ApiError ? err.code : undefined;
        const msg =
          err instanceof ApiError
            ? err.detail ?? err.message
            : err instanceof Error
              ? err.message
              : 'Could not load payment state.';
        setState({ kind: 'error', code, message: msg });
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [lotId, paidParam, paymentIdParam, stripeRedirectStatus, navigate]);

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
            code: `Payment.${p.status}`,
            message: `Payment finalized as ${p.status}. Try again or contact support.`,
          });
          return;
        }

        if (Date.now() - startedAt >= POLL_DEADLINE_MS) {
          // Webhook may have been slow; show a "still pending" message but keep polling option.
          setState({
            kind: 'error',
            code: 'Payment.PollingDeadline',
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
        const code = err instanceof ApiError ? err.code : undefined;
        const msg =
          err instanceof ApiError
            ? err.detail ?? err.message
            : err instanceof Error
              ? err.message
              : 'Could not verify payment status.';
        setState({ kind: 'error', code, message: msg });
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
          // Placeholder mirrors the eventual step-1 layout: subtitle line + form card with
          // four field rows + submit button. Matching the real shape keeps the page from
          // jumping when the state probe resolves.
          <>
            <SkeletonLine width="w-3/4" className="mt-4" />
            <SkeletonLine width="w-1/2" className="mt-2" />
            <div className="mt-6 rounded-lg border border-border bg-surface p-6 space-y-5">
              <FieldSkeleton />
              <FieldSkeleton />
              <FieldSkeleton />
              <FieldSkeleton />
              <Skeleton className="h-11 w-40 mt-2" />
            </div>
          </>
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
                {state.payment.stripePaymentIntentId && (
                  <div className="text-text-3 text-[12.5px]">
                    Stripe intent: <span className="mono">{state.payment.stripePaymentIntentId}</span>
                  </div>
                )}
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
          <ErrorCard
            code={state.code}
            fallbackMessage={state.message}
            lotId={lotId}
            onRetry={() => setState({ kind: 'loading' })}
          />
        )}
      </div>
      <Footer />
    </div>
  );
}

/** Label + input placeholder pair used in the initial-loading skeleton. */
function FieldSkeleton() {
  return (
    <div>
      <SkeletonLine width="w-24" className="h-2.5" />
      <Skeleton className="h-10 w-full mt-2" />
    </div>
  );
}

interface ErrorPreset {
  /** Headline. Should answer "what happened?" in 2–5 words. */
  title: string;
  /** Body copy. May override the server-provided fallback message when the code is known. */
  body?: string;
  /** Visual cue — small, restrained. We avoid emoji + over-the-top "alert" badges; the page is
   *  numismatic-marketplace, not a status console. */
  tone: 'neutral' | 'caution';
  /** Per-error CTAs. Primary first; ErrorCard renders it as the filled accent button. */
  actions: Array<
    | { label: string; to: string; kind: 'link' }
    | { label: string; kind: 'retry' }
  >;
}

function pickPreset(code: string | undefined, lotId: string): ErrorPreset {
  switch (code) {
    case 'Lot.NotWinner':
      return {
        title: 'Not your auction',
        body:
          'This lot was won by another buyer. You can browse other auctions or review the lots you have purchased.',
        tone: 'neutral',
        actions: [
          { kind: 'link', label: 'Browse auctions', to: '/' },
          { kind: 'link', label: 'My purchases', to: '/my-purchases' },
        ],
      };
    case 'Lot.NotSold':
      return {
        title: 'Auction is still open',
        body:
          'This lot is still accepting bids. Come back to pay after it closes — the winner gets a notification by email.',
        tone: 'neutral',
        actions: [
          { kind: 'link', label: 'View lot', to: `/lot/${lotId}` },
          { kind: 'link', label: 'Browse auctions', to: '/' },
        ],
      };
    case 'Lot.NotFound':
      return {
        title: 'Lot not found',
        body: 'This lot no longer exists. It may have been taken down by moderation.',
        tone: 'neutral',
        actions: [{ kind: 'link', label: 'Browse auctions', to: '/' }],
      };
    case 'Auth.NotAuthenticated':
      return {
        title: 'Session expired',
        body: 'Sign in again to resume your purchase.',
        tone: 'caution',
        actions: [
          { kind: 'link', label: 'Sign in', to: '/sign-in' },
          { kind: 'link', label: 'Home', to: '/' },
        ],
      };
    case 'Payment.Cancelled':
    case 'Payment.Failed':
      return {
        title: 'Payment terminated',
        body:
          'This payment was cancelled or failed and cannot be resumed. Contact support to retry the purchase.',
        tone: 'caution',
        actions: [{ kind: 'link', label: 'My purchases', to: '/my-purchases' }],
      };
    case 'Payment.PollingDeadline':
      return {
        title: 'Still confirming with Stripe',
        body:
          'Stripe is taking longer than usual to confirm your payment. Refresh in a minute, or check My Purchases for the final status.',
        tone: 'neutral',
        actions: [
          { kind: 'retry', label: 'Refresh' },
          { kind: 'link', label: 'My purchases', to: '/my-purchases' },
        ],
      };
    default:
      return {
        title: 'Couldn’t finish payment',
        tone: 'caution',
        actions: [
          { kind: 'retry', label: 'Try again' },
          { kind: 'link', label: 'My purchases', to: '/my-purchases' },
        ],
      };
  }
}

interface ErrorCardProps {
  code?: string;
  fallbackMessage: string;
  lotId: string;
  onRetry: () => void;
}

/**
 * Calm, on-brand error state. Avoids the "alarm-bell" pattern (red gradients, emoji) in favour of
 * a single surface card with restrained typography — the same visual language as the success
 * "Payment confirmed" card, so the page never feels schizophrenic between branches.
 */
function ErrorCard({ code, fallbackMessage, lotId, onRetry }: ErrorCardProps) {
  const preset = pickPreset(code, lotId);
  const body = preset.body ?? fallbackMessage;

  const accentBg = preset.tone === 'caution' ? 'var(--color-warning-soft)' : 'var(--color-bg-soft)';
  const accentColor = preset.tone === 'caution' ? 'var(--color-warning)' : 'var(--color-text-3)';

  return (
    <section
      role="alert"
      aria-live="polite"
      className="mt-6 rounded-lg border border-border bg-surface p-6"
    >
      <div className="flex items-start gap-4">
        <span
          aria-hidden="true"
          className="inline-flex items-center justify-center rounded-full flex-shrink-0"
          style={{ width: 40, height: 40, background: accentBg }}
        >
          <Icon
            name={preset.tone === 'caution' ? 'info' : 'shield'}
            size={18}
            color={accentColor}
          />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-[18px] font-semibold m-0 tracking-tight">{preset.title}</h2>
          <p className="text-text-2 text-[14px] mt-2 leading-relaxed m-0">{body}</p>
          {code && (
            <div className="mt-3 text-[11px] uppercase tracking-wider font-semibold text-text-4 mono">
              {code}
            </div>
          )}
        </div>
      </div>

      <div className="mt-5 flex flex-wrap gap-2.5 sm:justify-end">
        {preset.actions.map((action, i) => {
          const isPrimary = i === 0;
          const className = isPrimary
            ? 'inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-2.5 text-[13.5px] no-underline'
            : 'inline-flex items-center justify-center rounded-md border border-border-strong bg-surface hover:bg-bg-soft text-text font-medium px-5 py-2.5 text-[13.5px] no-underline';

          if (action.kind === 'retry') {
            return (
              <button key={action.label} type="button" onClick={onRetry} className={className}>
                {action.label}
              </button>
            );
          }
          return (
            <Link key={action.label} to={action.to} className={className}>
              {action.label}
            </Link>
          );
        })}
      </div>
    </section>
  );
}
