interface MedalSVGProps {
  ribbon?: string;
  accent?: string;
  dark?: string;
  size?: number;
}

export function MedalSVG({
  ribbon = '#7B1F1F',
  accent = '#C9A24E',
  dark = '#7C5A1F',
  size = 240,
}: MedalSVGProps) {
  return (
    <svg viewBox="0 0 240 240" width={size} height={size} style={{ display: 'block' }}>
      <path d="M70 20 L120 110 L100 30 Z" fill={ribbon} />
      <path d="M170 20 L120 110 L140 30 Z" fill={ribbon} opacity="0.85" />
      <path d="M100 30 L120 110 L140 30 Z" fill={ribbon} opacity="0.7" />
      <g transform="translate(120 145)">
        <polygon
          points="0,-60 14,-18 58,-18 22,8 36,52 0,26 -36,52 -22,8 -58,-18 -14,-18"
          fill={accent}
          stroke={dark}
          strokeWidth="1.5"
        />
        <polygon
          points="0,-40 9,-12 38,-12 14,5 24,34 0,17 -24,34 -14,5 -38,-12 -9,-12"
          fill="#A0381A"
        />
        <circle cx="0" cy="0" r="14" fill={accent} stroke={dark} strokeWidth="1.2" />
      </g>
    </svg>
  );
}
