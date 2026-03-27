"use client";

import { Coins, Star, ArrowRightLeft } from "lucide-react";

interface StatsStripProps {
  swapCredits: number;
  averageRating: number;
  totalReviews: number;
  hasMinimumReviews: boolean;
  totalSwaps: number;
}

export function StatsStrip({
  swapCredits,
  averageRating,
  totalReviews,
  hasMinimumReviews,
  totalSwaps,
}: StatsStripProps) {
  const reputationDisplay = hasMinimumReviews
    ? `${averageRating.toFixed(1)} ★`
    : "New";

  const stats = [
    {
      label: "Swap Credits",
      value: swapCredits,
      icon: Coins,
      valueColor: "text-[#EF9F27]",
    },
    {
      label: hasMinimumReviews ? `Reputation (${totalReviews})` : "Reputation",
      value: reputationDisplay,
      icon: Star,
      valueColor: "text-[#1D9E75]",
    },
    {
      label: "Total Swaps",
      value: totalSwaps,
      icon: ArrowRightLeft,
      valueColor: "text-[#3C2A8A]",
    },
  ];

  return (
    <div className="flex flex-wrap items-center gap-6 sm:gap-8 py-4">
      {stats.map((stat, index) => (
        <div key={stat.label} className="flex items-center gap-6 sm:gap-8">
          <div className="flex items-center gap-3">
            <stat.icon className={`w-5 h-5 ${stat.valueColor}`} />
            <div>
              <p className={`text-2xl font-semibold ${stat.valueColor}`}>
                {stat.value}
              </p>
              <p className="text-xs text-zinc-500 uppercase tracking-wide">
                {stat.label}
              </p>
            </div>
          </div>
          {index < stats.length - 1 && (
            <div className="hidden sm:block w-px h-10 bg-zinc-200" />
          )}
        </div>
      ))}
    </div>
  );
}
