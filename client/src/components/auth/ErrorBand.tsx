import { Icon } from '@/components/Icon';

type Kind = 'danger' | 'warning' | 'success';

interface ErrorBandProps {
  title: string;
  body?: string;
  kind?: Kind;
}

const kindStyles: Record<Kind, { bg: string; border: string; fg: string; icon: 'info' | 'check' }> = {
  danger: { bg: 'var(--color-danger-soft)', border: '#FECACA', fg: '#991B1B', icon: 'info' },
  warning: { bg: 'var(--color-warning-soft)', border: '#FCD34D', fg: '#92400E', icon: 'info' },
  success: { bg: 'var(--color-success-soft)', border: '#BBF7D0', fg: '#166534', icon: 'check' },
};

export function ErrorBand({ title, body, kind = 'danger' }: ErrorBandProps) {
  const s = kindStyles[kind];
  return (
    <div
      role="alert"
      className="flex items-start gap-2.5 rounded-md mb-4"
      style={{
        padding: '11px 14px',
        background: s.bg,
        border: `1px solid ${s.border}`,
      }}
    >
      <div className="mt-0.5 flex-shrink-0">
        <Icon name={s.icon} size={16} color={s.fg} />
      </div>
      <div className="text-sm leading-snug" style={{ color: s.fg }}>
        <div className="font-semibold">{title}</div>
        {body && <div className="mt-0.5 opacity-90">{body}</div>}
      </div>
    </div>
  );
}
