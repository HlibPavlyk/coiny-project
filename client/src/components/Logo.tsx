interface LogoProps {
  size?: number;
}

export function Logo({ size = 22 }: LogoProps) {
  const coinSize = size + 6;
  return (
    <div className="flex items-center gap-2">
      <svg width={coinSize} height={coinSize} viewBox="0 0 28 28" aria-hidden="true">
        {/* Back disc — slightly darker gold ring */}
        <circle cx="14" cy="14" r="12" fill="#F0B53C" />
        {/* Front face — bright gold, offset for 3D depth */}
        <circle cx="13" cy="13" r="11" fill="#FFCB2E" />
        {/* Hryvnia symbol — large, deep gold */}
        <text
          x="13"
          y="19"
          textAnchor="middle"
          fontSize="18"
          fontWeight="800"
          fill="#C5811A"
          fontFamily="Georgia, 'Times New Roman', serif"
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
