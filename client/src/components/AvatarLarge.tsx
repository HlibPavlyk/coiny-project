import { Icon } from '@/components/Icon';

interface AvatarLargeProps {
  initials: string;
  size?: number;
  showBadge?: boolean;
  verified?: boolean;
}

export function AvatarLarge({ initials, size = 96, showBadge = false, verified = false }: AvatarLargeProps) {
  return (
    <div className="relative flex-shrink-0" style={{ width: size, height: size }}>
      <div
        className="flex items-center justify-center text-white font-bold rounded-full"
        style={{
          width: size,
          height: size,
          background: 'linear-gradient(135deg, #D6A86A 0%, #8C5F2E 100%)',
          fontSize: size * 0.36,
          letterSpacing: '-0.02em',
          boxShadow:
            'inset 0 -2px 6px rgba(0,0,0,0.15), inset 0 2px 6px rgba(255,255,255,0.2)',
        }}
      >
        {initials.toUpperCase()}
      </div>
      {showBadge && (
        <div
          className="absolute flex items-center justify-center rounded-full"
          style={{
            bottom: 2,
            right: 2,
            width: size * 0.28,
            height: size * 0.28,
            background: verified ? 'var(--color-success)' : 'var(--color-warning)',
            border: '3px solid var(--color-surface)',
          }}
        >
          <Icon name={verified ? 'check' : 'info'} size={size * 0.16} color="#fff" stroke={2.5} />
        </div>
      )}
    </div>
  );
}
