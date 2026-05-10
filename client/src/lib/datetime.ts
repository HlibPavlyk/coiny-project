/**
 * Render a UTC ISO string in the user's local timezone, en-US locale.
 */
export function formatLocal(utcIso: string): string {
  const date = new Date(utcIso);
  return new Intl.DateTimeFormat('en-US', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(date);
}

/**
 * Compute hours/minutes/seconds remaining until <endsAt>. Negative when expired.
 */
export interface RemainingTime {
  totalSeconds: number;
  hours: number;
  minutes: number;
  seconds: number;
  expired: boolean;
  critical: boolean;
}

export function timeRemaining(endsAtIso: string, nowMs?: number): RemainingTime {
  const end = new Date(endsAtIso).getTime();
  const now = nowMs ?? Date.now();
  const totalSeconds = Math.floor((end - now) / 1000);
  if (totalSeconds <= 0) {
    return { totalSeconds: 0, hours: 0, minutes: 0, seconds: 0, expired: true, critical: false };
  }
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  return {
    totalSeconds,
    hours,
    minutes,
    seconds,
    expired: false,
    critical: hours === 0 && minutes < 60,
  };
}
