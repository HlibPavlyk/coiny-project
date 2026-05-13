import type { JSX } from 'react';

const paths: Record<string, JSX.Element> = {
  search: (
    <>
      <circle cx="11" cy="11" r="6" />
      <path d="M20 20l-3.5-3.5" />
    </>
  ),
  clock: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7v5l3 2" />
    </>
  ),
  bell: (
    <>
      <path d="M6 8a6 6 0 1 1 12 0c0 7 3 9 3 9H3s3-2 3-9z" />
      <path d="M10 21a2 2 0 0 0 4 0" />
    </>
  ),
  user: (
    <>
      <circle cx="12" cy="8" r="4" />
      <path d="M4 21c0-4.4 3.6-8 8-8s8 3.6 8 8" />
    </>
  ),
  plus: <path d="M12 5v14M5 12h14" />,
  x: <path d="M6 6l12 12M18 6L6 18" />,
  check: <path d="M5 12.5l4.5 4.5L19 7" />,
  info: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 8h.01M12 12v5" />
    </>
  ),
  shield: <path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z" />,
  list: <path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01" />,
  arrowL: <path d="M19 12H5M11 6l-6 6 6 6" />,
  cards: (
    <>
      <rect x="3" y="3" width="8" height="8" rx="1" />
      <rect x="13" y="3" width="8" height="8" rx="1" />
      <rect x="3" y="13" width="8" height="8" rx="1" />
      <rect x="13" y="13" width="8" height="8" rx="1" />
    </>
  ),
  bid: (
    <>
      <path d="M14 4l-9 9-2 7 7-2 9-9z" />
      <path d="M14 4l4 4M3 21h12" />
    </>
  ),
  package: (
    <>
      <path d="M3 7l9-4 9 4v10l-9 4-9-4z" />
      <path d="M3 7l9 4 9-4M12 11v10" />
    </>
  ),
  truck: (
    <>
      <rect x="2" y="6" width="13" height="11" rx="1" />
      <path d="M15 9h4l3 4v4h-7" />
      <circle cx="6" cy="18" r="2" />
      <circle cx="18" cy="18" r="2" />
    </>
  ),
  edit: <path d="M14 4l6 6L9 21H3v-6z" />,
  star: <path d="M12 3l2.7 5.5 6 .9-4.4 4.3 1 6-5.4-2.8-5.4 2.8 1-6L3 9.4l6-.9z" />,
  arrowR: <path d="M5 12h14M13 6l6 6-6 6" />,
  chevD: <path d="M6 9l6 6 6-6" />,
  chevU: <path d="M6 15l6-6 6 6" />,
  flame: <path d="M12 22c4 0 7-2.5 7-7 0-4-3-5-3-9-2 1-3 4-5 4-1 0-2-2-2-4-3 2-4 5-4 9 0 4.5 3 7 7 7z" />,
  coin: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7.8 L 13.35 10.5 L 16.35 10.95 L 14.18 13.05 L 14.7 16 L 12 14.6 L 9.3 16 L 9.82 13.05 L 7.65 10.95 L 10.65 10.5 Z" />
    </>
  ),
  bill: (
    <>
      <rect x="2" y="6" width="20" height="12" rx="1.5" />
      <circle cx="12" cy="12" r="2.5" />
      <path d="M5 9h0M19 15h0" />
    </>
  ),
  medal: (
    <>
      <circle cx="12" cy="14" r="6" />
      <path d="M8 8L6 2h12l-2 6" />
      <path d="M12 11v6M9 14h6" />
    </>
  ),
  grid: (
    <>
      <rect x="3" y="3" width="7" height="7" rx="1" />
      <rect x="14" y="3" width="7" height="7" rx="1" />
      <rect x="3" y="14" width="7" height="7" rx="1" />
      <rect x="14" y="14" width="7" height="7" rx="1" />
    </>
  ),
  external: (
    <>
      <path d="M14 4h6v6" />
      <path d="M20 4l-9 9" />
      <path d="M19 13v6a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1h6" />
    </>
  ),
};

export type IconName = keyof typeof paths;

interface IconProps {
  name: IconName;
  size?: number;
  color?: string;
  stroke?: number;
  className?: string;
}

export function Icon({ name, size = 16, color = 'currentColor', stroke = 1.6, className }: IconProps) {
  const path = paths[name];
  if (!path) return null;
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke={color}
      strokeWidth={stroke}
      strokeLinecap="round"
      strokeLinejoin="round"
      style={{ flexShrink: 0, display: 'block' }}
      className={className}
      aria-hidden="true"
    >
      {path}
    </svg>
  );
}
