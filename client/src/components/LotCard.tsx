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
}

interface LotCardProps {
  lot: LotCardModel;
  /** Optional hint for placeholder kind when coverImageUrl is missing (rare for real data). */
  placeholderKind?: 'coin' | 'banknote' | 'medal';
  /** Optional condition to show as a badge — revealed on hover only. */
  condition?: string;
  compact?: boolean;
}

export function LotCard({ lot, placeholderKind = 'coin', condition, compact = false }: LotCardProps) {
  const hot = lot.bidCount >= 10;
  return (
    <Link
      to={`/lot/${lot.id}`}
      className="group flex flex-col bg-surface border border-border rounded-lg overflow-hidden no-underline text-text transition hover:-translate-y-px"
      style={{ boxShadow: 'transparent' }}
      onMouseEnter={(e) => {
        e.currentTarget.style.boxShadow = 'var(--shadow-card-hover)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.boxShadow = 'transparent';
      }}
    >
      <div className="relative bg-bg-soft" style={{ aspectRatio: '1 / 1' }}>
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

        <div
          className="absolute inset-0 pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity duration-150"
          style={{ background: 'linear-gradient(180deg, rgba(0,0,0,0) 55%, rgba(0,0,0,0.78) 100%)' }}
        />

        {condition && (
          <div className="absolute top-2.5 left-2.5 opacity-0 group-hover:opacity-100 transition-opacity duration-150">
            <ConditionBadge value={condition} />
          </div>
        )}

        <div className="absolute inset-x-0 bottom-0 px-2.5 pb-2 flex items-center justify-between opacity-0 group-hover:opacity-100 transition-opacity duration-150 pointer-events-none">
          <CountdownTimer endsAt={lot.endsAt} size="sm" tone="light" />
          <span className="font-medium" style={{ fontSize: 11, color: 'white' }}>
            {lot.bidCount} {lot.bidCount === 1 ? 'bid' : 'bids'}
          </span>
        </div>
      </div>

      <div className={`flex flex-col gap-1 flex-1 ${compact ? 'px-3 pt-2.5 pb-3' : 'px-3.5 pt-3 pb-3.5'}`}>
        <div
          className="text-[13px] font-medium leading-snug truncate"
          title={lot.title}
        >
          {lot.title}
        </div>
        <div
          className="mono text-[15px] font-bold text-accent-deep"
          style={{ letterSpacing: '-0.01em' }}
        >
          {formatKopiykasAsUah(lot.currentPriceUahKopiykas, { integer: true })}
        </div>
      </div>
    </Link>
  );
}
