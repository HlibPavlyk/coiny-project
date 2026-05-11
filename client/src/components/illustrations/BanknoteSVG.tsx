interface BanknoteSVGProps {
  value?: string;
  year?: string;
  tone?: 'sepia' | 'blue' | 'green' | 'rose' | 'purple';
  size?: number;
}

const tones = {
  sepia: { bg: '#E9D9B5', ink: '#7A4D1F', accent: '#A8763E' },
  blue: { bg: '#CFDDE6', ink: '#1F4258', accent: '#2A6489' },
  green: { bg: '#CADBC4', ink: '#2C4A2A', accent: '#3F6B3A' },
  rose: { bg: '#E5C9C0', ink: '#742E22', accent: '#9C4332' },
  purple: { bg: '#D5C7DC', ink: '#3F2752', accent: '#604078' },
};

export function BanknoteSVG({ value = '10', year = '1994', tone = 'sepia', size = 240 }: BanknoteSVGProps) {
  const id = `bn-${Math.random().toString(36).slice(2, 8)}`;
  const t = tones[tone];
  return (
    <svg viewBox="0 0 240 130" width={size} height={size * (130 / 240)} style={{ display: 'block' }}>
      <defs>
        <pattern id={`${id}-guilloche`} x="0" y="0" width="6" height="6" patternUnits="userSpaceOnUse">
          <circle cx="3" cy="3" r="2" fill="none" stroke={t.ink} strokeOpacity="0.18" strokeWidth="0.4" />
        </pattern>
      </defs>
      <rect width="240" height="130" fill={t.bg} />
      <rect width="240" height="130" fill={`url(#${id}-guilloche)`} />
      <rect x="3" y="3" width="234" height="124" fill="none" stroke={t.ink} strokeOpacity="0.4" />
      <rect x="14" y="22" width="62" height="86" fill={t.bg} stroke={t.ink} strokeOpacity="0.35" />
      <ellipse cx="45" cy="60" rx="18" ry="22" fill={t.ink} fillOpacity="0.18" />
      <ellipse cx="45" cy="92" rx="22" ry="14" fill={t.ink} fillOpacity="0.14" />
      <text x="190" y="50" textAnchor="middle" fontSize="34" fontWeight="700" fill={t.accent} fontFamily="Georgia, serif">
        {value}
      </text>
      <text x="190" y="105" textAnchor="middle" fontSize="9" letterSpacing="3" fill={t.ink} fillOpacity="0.7">
        {year}
      </text>
      <text x="14" y="20" fontSize="10" fontWeight="700" fill={t.ink} fillOpacity="0.6">
        {value}
      </text>
      <text x="226" y="20" textAnchor="end" fontSize="10" fontWeight="700" fill={t.ink} fillOpacity="0.6">
        {value}
      </text>
    </svg>
  );
}
