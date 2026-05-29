type Condition = 'UNC' | 'AU' | 'XF' | 'VF' | 'F' | 'VG' | 'G' | 'Poor' | 'Ungraded';

const styles: Record<Condition, { bg: string; fg: string; border?: string }> = {
  UNC: { bg: '#0E2A17', fg: '#9CE0B3' },
  AU: { bg: '#0F3320', fg: '#7CD49E' },
  XF: { bg: '#1A2C52', fg: '#9CB7F0' },
  VF: { bg: '#23365C', fg: '#A8BEE0' },
  F: { bg: '#3D3826', fg: '#D4C28A' },
  VG: { bg: '#4A3320', fg: '#D4A878' },
  G: { bg: '#4A2C20', fg: '#D49878' },
  Poor: { bg: '#3A2A2A', fg: '#C09898' },
  Ungraded: { bg: '#F0EDE5', fg: '#5C5040', border: '#D5CFC0' },
};

interface ConditionBadgeProps {
  value: string;
  size?: 'sm' | 'md';
}

export function ConditionBadge({ value, size = 'sm' }: ConditionBadgeProps) {
  const s = styles[value as Condition] ?? styles.Ungraded;
  const py = size === 'sm' ? 3 : 4;
  const px = size === 'sm' ? 7 : 9;
  const fs = size === 'sm' ? 10 : 11;
  return (
    <span
      className="mono inline-flex items-center font-bold uppercase rounded"
      style={{
        padding: `${py}px ${px}px`,
        background: s.bg,
        color: s.fg,
        border: s.border ? `1px solid ${s.border}` : 'none',
        fontSize: fs,
        letterSpacing: '0.06em',
      }}
    >
      {value}
    </span>
  );
}
