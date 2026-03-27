interface AvatarProps {
  src?: string | null;
  name: string;
  size?: number;
  className?: string;
  imageClassName?: string;
}

function getInitial(name: string): string {
  return (name || "?").trim().charAt(0).toUpperCase() || "?";
}

export function Avatar({
  src,
  name,
  size = 40,
  className = "",
  imageClassName = "",
}: AvatarProps) {
  const containerStyle = { width: `${size}px`, height: `${size}px` };

  return (
    <div
      className={`flex shrink-0 items-center justify-center overflow-hidden rounded-full ${className}`.trim()}
      style={containerStyle}
      aria-label={name}
    >
      {src ? (
        <img
          src={src}
          alt={name}
          className={`h-full w-full object-cover ${imageClassName}`.trim()}
        />
      ) : (
        <span className="flex h-full w-full items-center justify-center bg-zinc-800 text-brand-teal-300 font-semibold">
          {getInitial(name)}
        </span>
      )}
    </div>
  );
}
