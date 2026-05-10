import { Icon } from '@/components/Icon';

interface VerificationStatusPillProps {
  verified: boolean;
}

export function VerificationStatusPill({ verified }: VerificationStatusPillProps) {
  return (
    <span
      className="inline-flex items-center gap-1 rounded-full font-semibold uppercase"
      style={{
        padding: '3px 9px',
        fontSize: 11,
        letterSpacing: '0.03em',
        background: verified ? 'var(--color-success-soft)' : 'var(--color-warning-soft)',
        color: verified ? '#166534' : '#92400E',
      }}
    >
      <Icon name={verified ? 'check' : 'info'} size={11} stroke={2.4} />
      {verified ? 'Verified' : 'Unverified'}
    </span>
  );
}
