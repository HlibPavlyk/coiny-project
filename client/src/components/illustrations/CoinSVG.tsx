interface CoinSVGProps {
  label?: string;
  year?: string;
  metal?: string;
  dark?: string;
  era?: 'modern' | 'imperial' | 'tryzub';
  size?: number;
}

export function CoinSVG({
  label = '5',
  year = '1992',
  metal = '#C8B380',
  dark = '#8A6A2A',
  era = 'modern',
  size = 240,
}: CoinSVGProps) {
  const id = `coin-${Math.random().toString(36).slice(2, 8)}`;
  return (
    <svg viewBox="0 0 240 240" width={size} height={size} style={{ display: 'block' }}>
      <defs>
        <radialGradient id={`${id}-rg`} cx="40%" cy="35%" r="65%">
          <stop offset="0%" stopColor={metal} />
          <stop offset="55%" stopColor={metal} />
          <stop offset="100%" stopColor={dark} />
        </radialGradient>
        <radialGradient id={`${id}-shine`} cx="35%" cy="30%" r="40%">
          <stop offset="0%" stopColor="#fff" stopOpacity="0.5" />
          <stop offset="100%" stopColor="#fff" stopOpacity="0" />
        </radialGradient>
      </defs>
      <circle cx="120" cy="120" r="108" fill={`url(#${id}-rg)`} />
      <circle cx="120" cy="120" r="108" fill={`url(#${id}-shine)`} />
      <circle cx="120" cy="120" r="108" fill="none" stroke={dark} strokeOpacity="0.4" strokeWidth="2" />
      <circle
        cx="120"
        cy="120"
        r="92"
        fill="none"
        stroke={dark}
        strokeOpacity="0.35"
        strokeWidth="1"
        strokeDasharray="2 3"
      />
      {era === 'tryzub' ? (
        <g transform="translate(120 120)" fill={dark} fillOpacity="0.55">
          <path d="M0 -50 L-4 -30 L-4 5 L-12 0 L-12 -25 L-22 -28 L-22 8 L-30 0 L-30 -32 L-38 -34 L-38 18 L-22 24 L-22 12 L-4 22 L-4 28 L0 30 L4 28 L4 22 L22 12 L22 24 L38 18 L38 -34 L30 -32 L30 0 L22 8 L22 -28 L12 -25 L12 0 L4 5 L4 -30 Z" />
        </g>
      ) : era === 'imperial' ? (
        <g transform="translate(120 120)" fill={dark} fillOpacity="0.55">
          <path d="M-30 -25 L0 -45 L30 -25 L30 25 L0 45 L-30 25 Z" />
        </g>
      ) : (
        <text
          x="120"
          y="138"
          textAnchor="middle"
          fontSize="68"
          fontWeight="700"
          fill={dark}
          fillOpacity="0.55"
          fontFamily="Georgia, serif"
        >
          {label}
        </text>
      )}
      <text
        x="120"
        y="62"
        textAnchor="middle"
        fontSize="14"
        letterSpacing="3"
        fontWeight="600"
        fill={dark}
        fillOpacity="0.55"
      >
        {year}
      </text>
    </svg>
  );
}
