import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useAuctionLot } from '@/hooks/useAuctionLot';
import { useAuthStore } from '@/state/useAuthStore';
import { useToastStore } from '@/state/useToastStore';
import { ApiError } from '@/api/fetch';
import { usePlaceBid } from '@/api/bids';
import { auth } from '@/api/auth';
import type { LotDetailModel, LotStatus } from '@/api/lots';
import { formatLocalCompact } from '@/lib/datetime';
import { CountdownTimer } from './CountdownTimer';
import { Icon } from './Icon';
import { formatKopiykasAsUah } from '@/lib/money';
import { minIncrementKopiykas, minNextBidKopiykas } from '@/lib/bidIncrement';

interface BidPanelProps {
  lotId: string;
  sellerId: string;
  status: LotStatus;
  startingPriceUahKopiykas: number;
  currentPriceUahKopiykas: number;
  bidCount: number;
  endsAt: string;
  /** True when the authenticated caller is currently the top bidder. */
  isCallerLeading: boolean;
  winningPriceUahKopiykas?: number | null;
  /** Caller's payment state on this lot (winner-only). null = no payment row yet. */
  callerPaymentId?: string | null;
  callerPaymentStatus?: LotDetailModel['callerPaymentStatus'];
}

/**
 * Right-column bid panel. Subscribes to the auction hub via useAuctionLot (thin-push); when the
 * server signals a change, React Query invalidates ['lot', lotId] and the page re-renders this
 * panel with fresh props. No client-side overlay store needed — props ARE the source of truth.
 */
export function BidPanel({
  lotId,
  sellerId,
  status,
  startingPriceUahKopiykas,
  currentPriceUahKopiykas,
  bidCount,
  endsAt,
  isCallerLeading,
  winningPriceUahKopiykas,
  callerPaymentStatus,
}: BidPanelProps) {
  useAuctionLot(lotId);

  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();

  const minBidKop = minNextBidKopiykas(currentPriceUahKopiykas);
  const minIncrementKop = minIncrementKopiykas(currentPriceUahKopiykas);

  const [amountUah, setAmountUah] = useState('');
  const [resending, setResending] = useState(false);

  const placeBid = usePlaceBid(lotId, {
    onSuccess: () => {
      // The server emits LotChanged immediately after commit — useAuctionLot will invalidate
      // ['lot', lotId] and the page re-renders with the new state. We just acknowledge.
      pushToast({ kind: 'success', title: 'Your bid was accepted.' });
      setAmountUah('');
    },
    onError: (err: unknown) => {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          pushToast({
            kind: 'warning',
            title: 'Outbid while you were typing',
            description: 'Someone bid first — current price refreshed.',
          });
          queryClient.invalidateQueries({ queryKey: ['lot', lotId] });
          return;
        }
        pushToast({
          kind: 'danger',
          title: 'Bid rejected',
          description: err.detail ?? err.message,
        });
        return;
      }
      pushToast({ kind: 'danger', title: 'Network error — try again.' });
    },
  });

  const onSubmit = (e: FormEvent) => {
    e.preventDefault();
    const parsed = Number(amountUah.replace(',', '.'));
    if (!Number.isFinite(parsed) || parsed <= 0) {
      pushToast({ kind: 'warning', title: 'Enter a positive bid amount.' });
      return;
    }
    const amountKop = Math.round(parsed * 100);
    if (amountKop < minBidKop) {
      pushToast({
        kind: 'warning',
        title: 'Bid below minimum',
        description: `Minimum next bid is ${formatKopiykasAsUah(minBidKop, { integer: true })}.`,
      });
      return;
    }
    placeBid.mutate({ amountUahKopiykas: amountKop });
  };

  const onResend = async () => {
    setResending(true);
    try {
      await auth.resendVerification();
      pushToast({ kind: 'success', title: 'Verification email sent' });
    } catch {
      pushToast({ kind: 'danger', title: 'Could not resend' });
    } finally {
      setResending(false);
    }
  };

  const isAuthenticated = !!user;
  const isSeller = isAuthenticated && user.id === sellerId;
  const isClosed = status !== 'Active';

  return (
    <div
      className="bg-surface border border-border rounded-lg p-5"
      style={{ boxShadow: 'var(--shadow-card)' }}
    >
      <div className="text-[12px] text-text-3 font-medium mb-1">
        {isClosed ? 'Final price' : 'Current price'}
      </div>
      <div
        className="mono font-bold text-text"
        style={{ fontSize: 36, letterSpacing: '-0.02em', lineHeight: 1.1 }}
        aria-live="polite"
      >
        {formatKopiykasAsUah(currentPriceUahKopiykas, { integer: true })}
      </div>
      <div className="text-[12px] text-text-3 mt-1">
        {isClosed ? (
          <>
            {bidCount} {bidCount === 1 ? 'bid' : 'bids'} · ended{' '}
            <span className="mono">{formatLocalCompact(endsAt)}</span>
          </>
        ) : (
          <>
            {bidCount} {bidCount === 1 ? 'bid' : 'bids'} · starts at{' '}
            <span className="mono">
              {formatKopiykasAsUah(startingPriceUahKopiykas, { integer: true })}
            </span>
          </>
        )}
      </div>

      {status === 'Active' && isCallerLeading && !isSeller && (
        <div
          className="mt-4 px-3.5 py-2.5 rounded-md flex items-center gap-2.5"
          style={{
            background: 'var(--color-success-soft)',
            border: '1px solid #BBE5C9',
          }}
        >
          <div
            className="flex items-center justify-center rounded-full text-white font-bold flex-shrink-0"
            style={{ width: 22, height: 22, background: '#16A34A', fontSize: 13, lineHeight: 1 }}
            aria-hidden="true"
          >
            ✓
          </div>
          <div className="flex-1">
            <div className="text-[12.5px] font-semibold" style={{ color: '#166534' }}>
              You're leading
            </div>
            <div className="text-[11.5px]" style={{ color: '#15803D' }}>
              Yours is the top bid right now.
            </div>
          </div>
        </div>
      )}

      {status === 'Active' && (
        <div
          className="mt-4 px-3.5 py-3 rounded-md flex items-center gap-2.5"
          style={{
            background: 'var(--color-accent-tint)',
            border: '1px solid var(--color-accent-soft)',
          }}
        >
          {/* Headline-style countdown that fills the whole card. No absolute date here — it lives
              in the page-meta line ("Ends ...") already, so the bidder gets the at-a-glance
              ticking clock here and the static reference up top. Left-aligned so the icon
              anchors at the card's start edge and the ticking text reads as one unit with it. */}
          <Icon name="clock" size={20} color="var(--color-accent-deep)" />
          <CountdownTimer endsAt={endsAt} size="xl" showIcon={false} showSeconds />
        </div>
      )}

      <div className="mt-4">
        {isClosed ? (
          <ClosedState
            lotId={lotId}
            status={status}
            isCallerWinner={isCallerLeading && status === 'Sold'}
            finalPriceUahKopiykas={winningPriceUahKopiykas ?? null}
            callerPaymentStatus={callerPaymentStatus ?? null}
          />
        ) : !isAuthenticated ? (
          <Link
            to={`/sign-in?return=/lot/${lotId}`}
            className="inline-flex w-full items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-3 text-[14px] no-underline"
          >
            Sign in to bid
          </Link>
        ) : user.isBanned ? (
          <NotePanel
            tone="danger"
            title="Your account is banned"
            body="Contact support if you think this is a mistake."
          />
        ) : !user.emailVerified ? (
          <div className="space-y-2">
            <NotePanel
              tone="warning"
              title="Verify your email to bid"
              body={
                <>
                  We sent a link to <span className="mono break-all">{user.email}</span>.
                </>
              }
            />
            <button
              type="button"
              onClick={onResend}
              disabled={resending}
              className="w-full rounded-md border border-border-strong bg-surface hover:bg-bg-soft px-3 py-2 text-[13px] font-medium disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {resending ? 'Sending…' : 'Resend verification email'}
            </button>
          </div>
        ) : isSeller ? (
          <NotePanel
            tone="info"
            title="You can't bid on your own lot"
            body="Sellers can't place bids. Track activity instead."
          />
        ) : isCallerLeading ? (
          <NotePanel
            tone="info"
            title="Nothing to do — you're already winning"
            body="Wait for another bidder to challenge your bid. We'll update this panel in real time when they do."
          />
        ) : (
          <form onSubmit={onSubmit} className="flex flex-col gap-2">
            <label htmlFor="bid-amount" className="text-[11px] font-semibold uppercase tracking-wider text-text-3">
              Your bid (UAH)
            </label>
            <div className="relative">
              <input
                id="bid-amount"
                type="text"
                inputMode="decimal"
                pattern="[0-9]*[.,]?[0-9]*"
                value={amountUah}
                onChange={(e) => setAmountUah(e.target.value.replace(/[^0-9.,]/g, ''))}
                placeholder={(minBidKop / 100).toFixed(2)}
                className="mono w-full rounded-md border border-border-strong bg-surface px-3 py-2.5 text-sm transition focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/15"
                disabled={placeBid.isPending}
                aria-describedby="bid-min-hint"
              />
              <span
                className="mono pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-text-3 text-sm"
              >
                ₴
              </span>
            </div>
            <div id="bid-min-hint" className="text-[12px] text-text-3" aria-live="polite">
              Min next bid:{' '}
              <span className="mono font-medium text-text-2">
                {formatKopiykasAsUah(minBidKop, { integer: true })}
              </span>{' '}
              · increment{' '}
              <span className="mono">{formatKopiykasAsUah(minIncrementKop, { integer: true })}</span>
            </div>
            <button
              type="submit"
              disabled={placeBid.isPending}
              className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-4 py-3 text-[14px] disabled:opacity-60 disabled:cursor-not-allowed mt-1"
            >
              {placeBid.isPending ? 'Placing…' : 'Place bid'}
            </button>
          </form>
        )}
      </div>

      <div className="flex items-center justify-center gap-1.5 mt-3 text-[11.5px] text-text-3">
        <Icon name="shield" size={11} color="var(--color-accent-deep)" />
        Stripe escrow protects every transaction
      </div>
    </div>
  );
}

function ClosedState({
  lotId,
  status,
  isCallerWinner,
  finalPriceUahKopiykas,
  callerPaymentStatus,
}: {
  lotId: string;
  status: LotStatus;
  isCallerWinner: boolean;
  finalPriceUahKopiykas: number | null;
  callerPaymentStatus: LotDetailModel['callerPaymentStatus'];
}) {
  // Winner view — branch by payment state so the buyer cannot re-enter checkout for a lot
  // they already paid for. Cancelled/Failed counts as "no live payment" so they can retry.
  if (status === 'Sold' && isCallerWinner) {
    const hasLivePayment =
      callerPaymentStatus === 'PendingAuthorization' ||
      callerPaymentStatus === 'Authorized' ||
      callerPaymentStatus === 'Captured';

    const { label, copy, href } = (() => {
      if (callerPaymentStatus === 'Captured') {
        return {
          label: 'Paid' as const,
          copy: 'Funds are held in Stripe escrow until Nova Poshta confirms delivery.',
          href: '/my-purchases' as const,
        };
      }
      if (callerPaymentStatus === 'Authorized') {
        return {
          label: 'Payment in progress' as const,
          copy: 'Card authorized. Stripe holds the funds; the seller is preparing your shipment.',
          href: '/my-purchases' as const,
        };
      }
      if (callerPaymentStatus === 'PendingAuthorization') {
        return {
          label: 'Awaiting authorization' as const,
          copy: 'We are waiting for Stripe to confirm the card hold.',
          href: '/my-purchases' as const,
        };
      }
      // null, Cancelled, Failed → still need to pay.
      return {
        label: null,
        copy: 'Pay within 96 hours to secure the lot. Funds are held in Stripe escrow until Nova Poshta confirms delivery.',
        href: `/my-purchases/${lotId}/pay` as const,
      };
    })();

    const ctaText = hasLivePayment ? 'View in My Purchases' : 'Complete checkout';

    return (
      <div className="rounded-md p-3.5" style={{ background: 'var(--color-success-soft)', border: '1px solid #BBE5C9' }}>
        <div className="text-[11px] uppercase tracking-wider font-semibold" style={{ color: '#166534' }}>
          {label ?? 'You won 🎉'}
        </div>
        {finalPriceUahKopiykas !== null && (
          <div className="mono text-[20px] font-bold mt-1">
            {formatKopiykasAsUah(finalPriceUahKopiykas, { integer: true })}
          </div>
        )}
        <div className="text-[12.5px] text-text-2 mt-1">{copy}</div>
        <Link
          to={href}
          className="inline-flex items-center justify-center rounded-md bg-accent hover:bg-accent-deep text-white font-medium px-5 py-2.5 text-sm no-underline mt-3 w-full"
        >
          {ctaText}
        </Link>
      </div>
    );
  }

  // Non-winner view of a Sold lot, plus the EndedNoSale / Cancelled cases — all share
  // the same restrained gray NotePanel. Tone differs only in the copy: a sold lot reads
  // as "auction closed in a sale", a no-sale lot as "no bids were placed", and a
  // cancelled lot as "bidding was cancelled".
  const body =
    status === 'Sold'
      ? 'The auction ended in a sale. Bidding is closed for this lot.'
      : status === 'Cancelled'
        ? 'The auction was cancelled. Bidding is closed for this lot.'
        : 'No bids were placed before the auction closed.';

  const title =
    status === 'Cancelled' ? 'Auction cancelled' : 'Auction closed';

  return <NotePanel tone="info" title={title} body={body} />;
}

function NotePanel({
  tone,
  title,
  body,
}: {
  tone: 'info' | 'warning' | 'danger';
  title: string;
  body: React.ReactNode;
}) {
  const styles: Record<typeof tone, { bg: string; border: string; titleColor: string }> = {
    info: { bg: 'var(--color-bg-soft)', border: 'var(--color-border)', titleColor: 'var(--color-text)' },
    warning: { bg: 'var(--color-warning-soft)', border: '#FCD34D', titleColor: '#92400E' },
    danger: { bg: '#FEF1EC', border: '#FCD9C9', titleColor: '#7C2A11' },
  };
  const s = styles[tone];
  return (
    <div
      className="rounded-md p-3.5"
      style={{ background: s.bg, border: `1px solid ${s.border}` }}
    >
      <div className="text-[13px] font-semibold" style={{ color: s.titleColor }}>
        {title}
      </div>
      <div className="text-[12.5px] mt-1 text-text-2">{body}</div>
    </div>
  );
}
