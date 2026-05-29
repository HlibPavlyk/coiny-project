/**
 * Render a UTC ISO string in the user's local timezone AND the user's locale. Passing `undefined`
 * to <see cref="Intl.DateTimeFormat"/> picks the browser's default locale (the same one
 * Chrome/Firefox use to render `<input type="datetime-local">`), so display and native inputs
 * stay in lock-step:
 *   • en-US → "May 29, 2026, 2:34 PM"
 *   • en-GB → "29 May 2026, 14:34"
 *   • uk-UA → "29 трав. 2026 р., 14:34"
 * No `hour12` override — we let each locale's convention decide.
 */
export function formatLocal(utcIso: string): string {
  const date = new Date(utcIso);
  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}

/**
 * Compact variant for table rows / panels — no year. Same locale-following rule as
 * <see cref="formatLocal"/>.
 */
export function formatLocalCompact(utcIso: string): string {
  const date = new Date(utcIso);
  return new Intl.DateTimeFormat(undefined, {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
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
