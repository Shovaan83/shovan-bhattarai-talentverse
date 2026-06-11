interface BrandLogoProps {
  className?: string;
  iconClassName?: string;
  textClassName?: string;
  showText?: boolean;
}

export function BrandLogo({
  className = "",
  iconClassName = "h-8 w-8",
  textClassName = "text-zinc-900",
  showText = true,
}: BrandLogoProps) {
  return (
    <span className={`inline-flex items-center gap-2 ${className}`}>
      <img
        src="/brand/icon-only-logo -nobg.png"
        alt=""
        aria-hidden="true"
        className={`${iconClassName} shrink-0`}
      />
      {showText && (
        <span className={`font-Sora font-bold tracking-tight ${textClassName}`}>
          Barterly
        </span>
      )}
    </span>
  );
}
