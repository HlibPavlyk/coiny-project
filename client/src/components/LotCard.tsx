import { Link } from 'react-router-dom';
import { ConditionBadge } from './ConditionBadge';
import { CountdownTimer } from './CountdownTimer';
import { Icon } from './Icon';
import { LotImagePlaceholder } from './LotImagePlaceholder';
import { formatKopiykasAsUah } from '@/lib/money';

export interface LotCardModel {
  id: string;
  title: string;
  coverImageUrl: string;
  currentPriceUahKopiykas: number;
  bidCount: number;
  endsAt: string;
  /** Returned by the API for the "Newest" sort; not displayed on the card, so optional for callers. */
  createdAt?: string;
}

interface LotCardProps {
  lot: LotCardModel;
  /** Optional hint for placeholder kind when coverImageUrl is missing (rare for real data). */
  placeholderKind?: 'coin' | 'banknote' | 'medal';
  /** Optional condition to show as a badge — revealed on hover only. */
  condition?: string;
  compact?: boolean;
  /** Render the closed-lot treatment: a "Sold" badge and final price instead of a live countdown. */
  sold?: boolean;
  /** Featured (2×2 bento) variant: the card fills its cell and the image grows to the cell height. */
  large?: boolean;
}

export function LotCard({
  lot,
  placeholderKind = 'coin',
  condition,
  compact = false,
  sold = false,
  large = false,
}: LotCardProps) {
  const hot = !sold && lot.bidCount >= 10;
  return (
    <Link
      to={`/lot/${lot.id}`}
      className={`group flex flex-col bg-surface border border-border rounded-lg overflow-hidden no-underline text-text transition hover:-translate-y-px ${large ? 'h-full' : ''}`}
      style={{ boxShadow: 'transparent' }}
      onMouseEnter={(e) => {
        e.currentTarget.style.boxShadow = 'var(--shadow-card-hover)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.boxShadow = 'transparent';
      }}
    >
      <div className={`relative bg-bg-soft ${large ? 'flex-1 min-h-0' : ''}`} style={large ? undefined : { aspectRatio: '1 / 1' }}>
        {lot.coverImageUrl ? (
          <img
            src={lot.coverImageUrl}
            alt=""
            loading="lazy"
            decoding="async"
            className="absolute inset-0 w-full h-full object-cover"
          />
        ) : (
          <LotImagePlaceholder kind={placeholderKind} variant={lot.id.charCodeAt(0) % 6} />
        )}

        {hot && (
          <div
            className="absolute top-2.5 right-2.5 inline-flex items-center gap-1 rounded-full font-semibold"
            style={{
              padding: '3px 7px',
              background: 'rgba(255,255,255,0.92)',
              backdropFilter: 'blur(4px)',
              fontSize: 10,
              letterSpacing: '0.04em',
              textTransform: 'uppercase',
            }}
          >
            <Icon name="flame" size={11} color="#D97706" />
            Hot
          </div>
        )}

        {sold && (
          <div
            className="absolute top-2.5 left-2.5 inline-flex items-center rounded-full font-semibold"
            style={{
              padding: '3px 8px',
              background: 'rgba(26,26,26,0.85)',
              color: 'white',
              fontSize: 10,
              letterSpacing: '0.05em',
              textTransform: 'uppercase',
            }}
          >
            Sold
          </div>
        )}

        <div
          className="absolute inset-0 pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity duration-150"
          style={{ background: 'linear-gradient(180deg, rgba(0,0,0,0) 55%, rgba(0,0,0,0.78) 100%)' }}
        />

        {!sold && condition && (
          <div className="absolute top-2.5 left-2.5 opacity-0 group-hover:opacity-100 transition-opacity duration-150">
            <ConditionBadge value={condition} />
          </div>
        )}

        <div className="absolute inset-x-0 bottom-0 px-2.5 pb-2 flex items-center justify-between opacity-0 group-hover:opacity-100 transition-opacity duration-150 pointer-events-none">
          {sold ? (
            <span className="font-medium" style={{ fontSize: 11, color: 'white' }}>
              Sold
            </span>
          ) : (
            <CountdownTimer endsAt={lot.endsAt} size="sm" tone="light" />
          )}
          <span className="font-medium" style={{ fontSize: 11, color: 'white' }}>
            {lot.bidCount} {lot.bidCount === 1 ? 'bid' : 'bids'}
          </span>
        </div>
      </div>

      <div className={`flex flex-col gap-1 ${large ? 'shrink-0' : 'flex-1'} ${compact ? 'px-3 pt-2.5 pb-3' : 'px-3.5 pt-3 pb-3.5'}`}>
        <div
          className={`font-medium leading-snug truncate ${large ? 'text-[15px]' : 'text-[13px]'}`}
          title={lot.title}
        >
          {lot.title}
        </div>
        <div
          className={`mono font-bold text-accent-deep ${large ? 'text-[18px]' : 'text-[15px]'}`}
          style={{ letterSpacing: '-0.01em' }}
        >
          {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
        </div>
      </div>
    </Link>
  );
}
