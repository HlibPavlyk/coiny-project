interface LogoProps {
  size?: number;
}

export function Logo({ size = 22 }: LogoProps) {
  return (
    <div className="flex items-center gap-2">
      <svg width={size + 4} height={size + 4} viewBox="0 0 28 28" aria-hidden="true">
        <defs>
          <radialGradient id="coiny-logo-rg" cx="35%" cy="35%" r="65%">
            <stop offset="0%" stopColor="#D6A86A" />
            <stop offset="100%" stopColor="#8C5F2E" />
          </radialGradient>
        </defs>
        <circle cx="14" cy="14" r="12" fill="url(#coiny-logo-rg)" />
        <circle cx="14" cy="14" r="12" fill="none" stroke="#6F4520" strokeOpacity="0.5" strokeWidth="0.6" />
        <text
          x="14"
          y="18"
          textAnchor="middle"
          fontSize="13"
          fontWeight="700"
          fill="#3D2810"
          fillOpacity="0.7"
          fontFamily="Georgia, serif"
        >
          ₴
        </text>
      </svg>
      <span
        className="font-bold text-text"
        style={{ fontSize: size, letterSpacing: '-0.02em' }}
      >
        Coiny
      </span>
    </div>
  );
}
