import { useEffect, useState } from 'react';
import { Icon } from '@/components/Icon';
import { timeRemaining } from '@/lib/datetime';

interface CountdownTimerProps {
  endsAt: string;
  size?: 'sm' | 'md' | 'lg';
  showIcon?: boolean;
  tone?: 'default' | 'light';
}

export function CountdownTimer({ endsAt, size = 'sm', showIcon = true, tone = 'default' }: CountdownTimerProps) {
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, []);

  const t = timeRemaining(endsAt, now);
  const fontSize = size === 'sm' ? 11 : size === 'md' ? 13 : 16;
  const color = t.expired
    ? tone === 'light' ? 'rgba(255,255,255,0.7)' : 'var(--color-text-3)'
    : t.critical
      ? tone === 'light' ? '#FCA5A5' : 'var(--color-danger)'
      : tone === 'light' ? 'white' : 'var(--color-text-2)';

  if (t.expired) {
    return (
      <span
        className="inline-flex items-center gap-1 font-medium"
        style={{ fontSize, color }}
      >
        {showIcon && <Icon name="clock" size={fontSize + 1} color={color} stroke={1.7} />}
        Ended
      </span>
    );
  }

  let label: string;
  if (t.hours >= 24) {
    const days = Math.floor(t.hours / 24);
    label = `${days}d ${t.hours % 24}h`;
  } else if (t.hours > 0) {
    label = `${t.hours}h ${t.minutes}m`;
  } else if (t.minutes > 0) {
    label = `${t.minutes}m ${t.seconds}s`;
  } else {
    label = `${t.seconds}s`;
  }

  return (
    <span
      className="inline-flex items-center gap-1 font-medium"
      style={{ fontSize, color }}
    >
      {showIcon && <Icon name="clock" size={fontSize + 1} color={color} stroke={1.7} />}
      {label}
    </span>
  );
}
