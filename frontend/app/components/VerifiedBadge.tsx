"use client";

import { BadgeCheck } from "lucide-react";

interface VerifiedBadgeProps {
  size?: "sm" | "md" | "lg";
  showText?: boolean;
  className?: string;
}

export default function VerifiedBadge({
  size = "md",
  showText = false,
  className = "",
}: VerifiedBadgeProps) {
  const iconSizes = {
    sm: "w-4 h-4",
    md: "w-5 h-5",
    lg: "w-6 h-6",
  };

  const textSizes = {
    sm: "text-xs",
    md: "text-sm",
    lg: "text-base",
  };

  return (
    <div className={`flex items-center gap-1 ${className}`}>
      <BadgeCheck
        className={`${iconSizes[size]} text-blue-500 fill-blue-500`}
        aria-label="Verified"
      />
      {showText && (
        <span className={`${textSizes[size]} font-medium text-blue-600`}>
          Verified
        </span>
      )}
    </div>
  );
}
