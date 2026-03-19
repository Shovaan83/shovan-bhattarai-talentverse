"use client";

import type { BadgeDto } from "@/lib/types/badges";

// Map iconKey strings to emoji representations
const BADGE_ICONS: Record<string, string> = {
  "first-swap": "🤝",
  "five-swaps": "⭐",
  "ten-swaps": "🏆",
  "first-review": "💬",
  "top-rated": "🌟",
  "credit-saver": "💰",
  "generous": "🎁",
  "veteran": "🎖️",
  "pioneer": "🚀",
};

// Tier colors using gold/amber/emerald palette
const TIER_COLORS: Record<string, { bg: string; border: string; text: string; badge: string }> = {
  Bronze: {
    bg: "bg-amber-50",
    border: "border-amber-200",
    text: "text-amber-700",
    badge: "bg-amber-100 text-amber-800",
  },
  Silver: {
    bg: "bg-gray-50",
    border: "border-gray-200",
    text: "text-gray-600",
    badge: "bg-gray-100 text-gray-700",
  },
  Gold: {
    bg: "bg-yellow-50",
    border: "border-yellow-300",
    text: "text-yellow-700",
    badge: "bg-yellow-100 text-yellow-800",
  },
  Platinum: {
    bg: "bg-cyan-50",
    border: "border-cyan-300",
    text: "text-cyan-700",
    badge: "bg-cyan-100 text-cyan-800",
  },
};

interface BadgeGridProps {
  badges: BadgeDto[];
  isLoading?: boolean;
}

export default function BadgeGrid({ badges, isLoading }: BadgeGridProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="h-28 rounded-2xl bg-white/10 animate-pulse"
          />
        ))}
      </div>
    );
  }

  if (!badges || badges.length === 0) {
    return (
      <p className="text-sm text-emerald-300/70 text-center py-6">
        No badges earned yet. Complete swaps and get reviews to unlock them!
      </p>
    );
  }

  const earned = badges.filter((b) => b.isEarned);
  const unearned = badges.filter((b) => !b.isEarned);

  return (
    <div className="space-y-4">
      {earned.length > 0 && (
        <div>
          <p className="text-xs font-bold uppercase tracking-widest text-amber-400 mb-3">
            Earned ({earned.length})
          </p>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {earned.map((badge) => (
              <BadgeCard key={badge.badgeId} badge={badge} />
            ))}
          </div>
        </div>
      )}

      {unearned.length > 0 && (
        <div>
          <p className="text-xs font-bold uppercase tracking-widest text-emerald-400/60 mb-3">
            Locked ({unearned.length})
          </p>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
            {unearned.map((badge) => (
              <BadgeCard key={badge.badgeId} badge={badge} locked />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function BadgeCard({ badge, locked }: { badge: BadgeDto; locked?: boolean }) {
  const tier = TIER_COLORS[badge.tier] ?? TIER_COLORS.Bronze;
  const icon = BADGE_ICONS[badge.iconKey] ?? "🏅";

  if (locked) {
    return (
      <div className="relative flex flex-col items-center gap-2 p-3 rounded-2xl bg-white/5 border border-white/10 opacity-40 select-none">
        <span className="text-3xl grayscale">{icon}</span>
        <div className="text-center">
          <p className="text-xs font-semibold text-white/60 leading-tight">
            {badge.name}
          </p>
        </div>
        {/* Lock overlay */}
        <span className="absolute top-2 right-2 text-[10px]">🔒</span>
      </div>
    );
  }

  return (
    <div
      className={`flex flex-col items-center gap-2 p-3 rounded-2xl border ${tier.bg} ${tier.border} shadow-sm hover:shadow-md transition-shadow`}
      title={badge.description}
    >
      <span className="text-3xl">{icon}</span>
      <div className="text-center">
        <p className={`text-xs font-bold leading-tight ${tier.text}`}>
          {badge.name}
        </p>
        <span
          className={`inline-block mt-1 text-[10px] font-bold px-2 py-0.5 rounded-full ${tier.badge}`}
        >
          {badge.tier}
        </span>
      </div>
      {badge.earnedAt && (
        <p className="text-[10px] text-gray-400">
          {new Date(badge.earnedAt).toLocaleDateString()}
        </p>
      )}
    </div>
  );
}
