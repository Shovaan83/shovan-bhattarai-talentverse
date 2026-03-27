"use client";

interface GradientBorderProps {
  children: React.ReactNode;
  className?: string;
  gradientColors?: string[];
  borderWidth?: number;
  borderRadius?: number;
}

/**
 * Card wrapper with animated gradient border effect.
 * Uses CSS conic-gradient rotation for a sweeping border animation.
 */
export function GradientBorder({
  children,
  className = "",
  gradientColors = ["#1D9E75", "#3C2A8A", "#5DCAA5", "#534AB7"],
  borderWidth = 2,
  borderRadius = 12,
}: GradientBorderProps) {
  const gradient = `conic-gradient(from var(--gradient-angle), ${gradientColors.join(", ")})`;

  return (
    <div
      className={`relative ${className}`}
      style={{
        borderRadius: `${borderRadius}px`,
        padding: `${borderWidth}px`,
        background: gradient,
        // @ts-expect-error CSS custom property
        "--gradient-angle": "0deg",
        animation: "gradient-rotate 4s linear infinite",
      }}
    >
      <div
        className="h-full w-full bg-white"
        style={{ borderRadius: `${borderRadius - borderWidth}px` }}
      >
        {children}
      </div>
    </div>
  );
}
