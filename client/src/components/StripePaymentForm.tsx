import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { loadStripe, type Stripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useElements, useStripe } from '@stripe/react-stripe-js';
import { config } from '@/api/np';
import { payments, type CreatePaymentIntentResponse } from '@/api/payments';
import { useToastStore } from '@/state/useToastStore';
import { ApiError } from '@/api/fetch';

interface Props {
  lotId: string;
  /** Where Stripe should send the user after a 3DS challenge. Should include `?paid=1`. */
  returnUrl: string;
}

/**
 * Renders the Stripe-hosted payment element (card input + 3DS) given a freshly minted
 * PaymentIntent's client_secret. Splits in two: the outer fetches the intent and the
 * publishable key, the inner uses the Stripe.js context to drive confirmation.
 */
export function StripePaymentForm({ lotId, returnUrl }: Props) {
  const pushToast = useToastStore((s) => s.push);

  const publicCfg = useQuery({
    queryKey: ['public-config'],
    queryFn: () => config.getPublic(),
    staleTime: 5 * 60_000,
  });

  // The intent is created exactly once per page load; we keep it in state so an HMR
  // rerender doesn't accidentally mint a second one (server-side this is idempotent on
  // lotId anyway — but client-side noise is worth avoiding).
  const [intent, setIntent] = useState<CreatePaymentIntentResponse | null>(null);
  const [intentError, setIntentError] = useState<string | null>(null);

  const [alreadyExists, setAlreadyExists] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const r = await payments.createIntent(lotId);
        if (!cancelled) setIntent(r);
      } catch (err) {
        if (cancelled) return;
        // 409 Payment.AlreadyExists: the buyer already minted an intent (possibly during a
        // previous attempt that errored mid-3DS). We can't reuse the existing client_secret
        // here without re-fetching from Stripe, so send them to My Purchases — the row is
        // there with the right CTA.
        if (err instanceof ApiError && err.status === 409) {
          setAlreadyExists(true);
          return;
        }
        const msg =
          err instanceof ApiError
            ? err.detail ?? err.message
            : err instanceof Error
              ? err.message
              : 'Could not create payment intent.';
        setIntentError(msg);
        pushToast({ kind: 'danger', title: 'Payment setup failed', description: msg });
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [lotId, pushToast]);

  const stripePromise: Promise<Stripe | null> | null = useMemo(() => {
    if (!publicCfg.data?.stripePublishableKey) return null;
    return loadStripe(publicCfg.data.stripePublishableKey);
  }, [publicCfg.data?.stripePublishableKey]);

  if (alreadyExists) {
    return (
      <div className="rounded-md p-4" style={{ background: 'var(--color-bg-soft)', border: '1px solid var(--color-border)' }}>
        <div className="text-[13.5px] font-semibold">A payment is already in progress</div>
        <p className="text-[12.5px] text-text-2 mt-1.5 leading-relaxed">
          You already started paying for this lot. Open <strong>My Purchases</strong> to find the
          row and resume the card confirmation — your delivery details are saved.
        </p>
        <Link
          to="/my-purchases"
          className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-2 text-[13px] no-underline mt-3"
        >
          Go to My Purchases
        </Link>
      </div>
    );
  }

  if (intentError) {
    return (
      <div
        className="rounded-md p-4 text-[13px]"
        style={{ background: 'var(--color-danger-soft)', color: '#7F1D1D' }}
      >
        {intentError}
      </div>
    );
  }

  if (!intent || !stripePromise) {
    return <div className="text-[13.5px] text-text-3">Preparing secure payment form…</div>;
  }

  // Append our internal paymentId to the return URL so PayLotPage can identify which
  // Payment row to poll after Stripe redirects back. Stripe also appends its own
  // payment_intent / payment_intent_client_secret / redirect_status — those don't
  // identify our Payment row (Stripe knows `pi_…`, we key by Guid).
  const fullReturnUrl = appendQuery(returnUrl, { paymentId: intent.paymentId });

  return (
    <Elements stripe={stripePromise} options={{ clientSecret: intent.clientSecret }}>
      <PaymentInner intent={intent} returnUrl={fullReturnUrl} />
    </Elements>
  );
}

function appendQuery(url: string, params: Record<string, string>): string {
  const sep = url.includes('?') ? '&' : '?';
  const qs = Object.entries(params)
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
    .join('&');
  return `${url}${sep}${qs}`;
}

function PaymentInner({
  intent,
  returnUrl,
}: {
  intent: CreatePaymentIntentResponse;
  returnUrl: string;
}) {
  const stripe = useStripe();
  const elements = useElements();
  const pushToast = useToastStore((s) => s.push);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!stripe || !elements) return;

    setSubmitting(true);
    setSubmitError(null);
    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: { return_url: returnUrl },
    });

    // If `error` is set, the user stayed on this page — Stripe didn't redirect.
    // Otherwise the browser is already navigating to `return_url`.
    if (error) {
      const msg = error.message ?? 'Payment failed. Please check your card details.';
      setSubmitError(msg);
      pushToast({ kind: 'danger', title: 'Payment failed', description: msg });
      setSubmitting(false);
    }
  }

  const uahDisplay = (intent.amountUahKopiykasDisplay / 100).toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
  const usdCharged = (intent.amountUsdCentsCharged / 100).toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div className="rounded-md border border-border bg-bg-soft px-4 py-3 text-[13.5px]">
        <div className="flex items-baseline justify-between">
          <span className="text-text-3">Amount due</span>
          <span className="mono font-semibold text-[16px]">{uahDisplay} UAH</span>
        </div>
        <div
          className="mt-1.5 text-[11.5px] text-text-3"
          title={`Test mode: 1 USD ≈ ${intent.rateUsedUahPerUsd} UAH`}
        >
          Charged in test mode as ${usdCharged} USD (1 USD ≈ {intent.rateUsedUahPerUsd} UAH).
        </div>
      </div>

      <PaymentElement options={{ layout: 'tabs' }} />

      {submitError && (
        <div
          className="rounded-md p-3 text-[13px]"
          style={{ background: 'var(--color-danger-soft)', color: '#7F1D1D' }}
        >
          {submitError}
        </div>
      )}

      <button
        type="submit"
        disabled={!stripe || submitting}
        className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-3 text-sm disabled:opacity-60 disabled:cursor-not-allowed w-full"
      >
        {submitting ? 'Confirming…' : `Pay ${uahDisplay} UAH`}
      </button>
    </form>
  );
}
