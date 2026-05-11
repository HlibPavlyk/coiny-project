import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useAuctionLot } from '@/hooks/useAuctionLot';
import { useAuthStore } from '@/state/useAuthStore';
import { useLotPageStore } from '@/state/useLotPageStore';
import { useToastStore } from '@/state/useToastStore';
import { ApiError } from '@/api/fetch';
import { usePlaceBid, type PlaceBidModel } from '@/api/bids';
import { auth } from '@/api/auth';
import type { LotStatus } from '@/api/lots';
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
  winnerDisplayName?: string | null;
  winningPriceUahKopiykas?: number | null;
}

/**
 * Right-column bid panel. Wires SignalR live updates via useAuctionLot, branches by user state,
 * and submits new bids with optimistic UI + 409 outbid handling.
 */
export function BidPanel({
  lotId,
  sellerId,
  status: initialStatus,
  startingPriceUahKopiykas,
  currentPriceUahKopiykas,
  bidCount,
  endsAt,
  winnerDisplayName,
  winningPriceUahKopiykas,
}: BidPanelProps) {
  useAuctionLot(lotId);

  const user = useAuthStore((s) => s.user);
  const pushToast = useToastStore((s) => s.push);
  const queryClient = useQueryClient();
  const live = useLotPageStore();

  const currentPrice = live.liveCurrentPriceUahKopiykas ?? currentPriceUahKopiykas;
  const liveBidCount = live.liveBidCount ?? bidCount;
  const liveEndsAt = live.liveEndsAt ?? endsAt;
  const status: LotStatus = live.liveStatus ?? initialStatus;
  const liveWinner = live.liveWinner;

  const minBidKop = minNextBidKopiykas(currentPrice);
  const minIncrementKop = minIncrementKopiykas(currentPrice);

  const [amountUah, setAmountUah] = useState('');
  const [extensionFlash, setExtensionFlash] = useState(false);
  const [resending, setResending] = useState(false);

  // Pulse the countdown for ~1s whenever liveEndsAt moves (anti-snipe extension fired).
  useEffect(() => {
    if (live.liveEndsAt === null) return;
    setExtensionFlash(true);
    const id = setTimeout(() => setExtensionFlash(false), 1100);
    return () => clearTimeout(id);
  }, [live.liveEndsAt]);

  const placeBid = usePlaceBid(lotId, {
    onSuccess: (model: PlaceBidModel) => {
      // Optimistic overlay until SignalR's BidPlaced reconfirms (~1s).
      useLotPageStore.getState().setSnapshot({
        liveCurrentPriceUahKopiykas: model.newCurrentPriceUahKopiykas,
        liveBidCount: model.newBidCount,
        liveEndsAt: model.newEndsAt,
      });
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
      <div className="text-[12px] text-text-3 font-medium mb-1">Current price</div>
      <div
        className="mono font-bold text-text"
        style={{ fontSize: 36, letterSpacing: '-0.02em', lineHeight: 1.1 }}
        aria-live="polite"
      >
        {formatKopiykasAsUah(currentPrice, { integer: true })}
      </div>
      <div className="text-[12px] text-text-3 mt-1">
        {liveBidCount} {liveBidCount === 1 ? 'bid' : 'bids'} · starts at{' '}
        <span className="mono">
          {formatKopiykasAsUah(startingPriceUahKopiykas, { integer: true })}
        </span>
      </div>

      {status === 'Active' && (
        <div
          className={`mt-4 px-3.5 py-3 rounded-md flex items-center gap-2.5 ${
            extensionFlash ? 'animate-pulse' : ''
          }`}
          style={{
            background: 'var(--color-accent-tint)',
            border: '1px solid var(--color-accent-soft)',
          }}
        >
          <Icon name="clock" size={14} color="var(--color-accent-deep)" />
          <div className="flex-1">
            <div className="text-[10px] uppercase tracking-wider font-semibold text-text-3 mb-0.5">
              Time left
            </div>
            <CountdownTimer endsAt={liveEndsAt} size="md" showIcon={false} />
          </div>
        </div>
      )}

      <div className="mt-4">
        {isClosed ? (
          <ClosedState
            status={status}
            winnerDisplayName={liveWinner?.winnerDisplayName ?? winnerDisplayName ?? null}
            finalPriceUahKopiykas={
              liveWinner?.finalPriceUahKopiykas ?? winningPriceUahKopiykas ?? null
            }
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
                  We sent a link to <span className="mono">{user.email}</span>.
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
  status,
  winnerDisplayName,
  finalPriceUahKopiykas,
}: {
  status: LotStatus;
  winnerDisplayName: string | null;
  finalPriceUahKopiykas: number | null;
}) {
  if (status === 'Sold' && winnerDisplayName) {
    return (
      <div className="rounded-md p-3.5" style={{ background: 'var(--color-success-soft)', border: '1px solid #BBE5C9' }}>
        <div className="text-[11px] uppercase tracking-wider font-semibold" style={{ color: '#166534' }}>
          Sold
        </div>
        <div className="mt-1 text-[14px] font-medium text-text">
          Winner:{' '}
          <span className="mono text-accent-deep">{winnerDisplayName}</span>
        </div>
        {finalPriceUahKopiykas !== null && (
          <div className="mono text-[18px] font-bold mt-1">
            {formatKopiykasAsUah(finalPriceUahKopiykas, { integer: true })}
          </div>
        )}
      </div>
    );
  }

  return (
    <NotePanel
      tone="info"
      title={status === 'Cancelled' ? 'Auction cancelled' : 'Auction ended with no sale'}
      body="Bidding is closed for this lot."
    />
  );
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
