"use client";

import { Star } from "lucide-react";

interface ReputationBadgeProps {
  averageRating: number;
  totalReviews: number;
  hasMinimumReviews: boolean;
  size?: "sm" | "md" | "lg";
  showCount?: boolean;
}

export default function ReputationBadge({
  averageRating,
  totalReviews,
  hasMinimumReviews,
  size = "md",
  showCount = true,
}: ReputationBadgeProps) {
  // Don't show rating if user doesn't have minimum reviews (3+)
  if (!hasMinimumReviews) {
    return (
      <div className="flex items-center gap-1.5">
        <span className="text-sm text-gray-500 font-medium">New Member</span>
      </div>
    );
  }

  const sizeClasses = {
    sm: "text-sm",
    md: "text-base",
    lg: "text-lg",
  };

  const starSizes = {
    sm: "w-3.5 h-3.5",
    md: "w-4 h-4",
    lg: "w-5 h-5",
  };

  return (
    <div className={`flex items-center gap-1.5 ${sizeClasses[size]}`}>
      <div className="flex items-center gap-0.5">
        <Star className={`${starSizes[size]} fill-yellow-400 text-yellow-400`} />
        <span className="font-semibold text-gray-900">
          {averageRating.toFixed(1)}
        </span>
      </div>
      {showCount && (
        <span className="text-gray-500">({totalReviews} reviews)</span>
      )}
    </div>
  );
}
