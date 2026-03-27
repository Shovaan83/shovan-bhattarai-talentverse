'use client';

import React from "react";
import { ArrowRight } from "lucide-react";
import Link from "next/link";
import type { PublicUserDto } from "@/lib/types/marketplace";
import VerifiedBadge from "../VerifiedBadge";
import { Avatar } from "@/app/components/ui/Avatar";

interface UserCardProps {
  user: PublicUserDto;
}

export function UserCard({ user }: UserCardProps) {
  const offerSkills = user.offeredSkills.slice(0, 3).map(s => s.skillName);
  const wantSkills = user.wantedSkills.slice(0, 3).map(s => s.skillName);

  return (
    <Link href={`/users/${user.id}`} className="block">
      <div className="flex items-center gap-4 px-5 py-4 hover:bg-zinc-50 transition-colors">
        {/* Avatar */}
        <Avatar
          src={user.profilePictureUrl}
          name={user.displayName}
          size={48}
        />

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="font-semibold text-zinc-900 truncate">{user.displayName}</h3>
            {user.isVerified && <VerifiedBadge size="sm" />}
            <span className="text-zinc-400 text-sm">@{user.userName}</span>
          </div>

          {/* Skills inline */}
          <div className="flex items-center gap-2 mt-1 text-sm">
            {offerSkills.length > 0 && (
              <span className="text-[#1D9E75]">
                Offers: {offerSkills.join(', ')}{user.offeredSkills.length > 3 && ` +${user.offeredSkills.length - 3}`}
              </span>
            )}
            {offerSkills.length > 0 && wantSkills.length > 0 && (
              <span className="text-zinc-300">→</span>
            )}
            {wantSkills.length > 0 && (
              <span className="text-[#3C2A8A]">
                Seeks: {wantSkills.join(', ')}{user.wantedSkills.length > 3 && ` +${user.wantedSkills.length - 3}`}
              </span>
            )}
          </div>
        </div>

        {/* Right side */}
        <div className="flex items-center gap-3 text-zinc-400">
          <span className="text-xs">{user.completedSwaps} swaps</span>
          <ArrowRight className="w-4 h-4" />
        </div>
      </div>
    </Link>
  );
}

// Loading Skeleton
export function UserCardSkeleton() {
  return (
    <div className="flex items-center gap-4 px-5 py-4 animate-pulse">
      {/* Avatar skeleton */}
      <div className="w-12 h-12 rounded-full bg-zinc-200" />

      {/* Content skeleton */}
      <div className="flex-1 min-w-0 space-y-2">
        <div className="flex items-center gap-2">
          <div className="h-4 w-32 bg-zinc-200 rounded" />
          <div className="h-4 w-20 bg-zinc-100 rounded" />
        </div>
        <div className="h-3 w-64 bg-zinc-100 rounded" />
      </div>

      {/* Right side skeleton */}
      <div className="flex items-center gap-3">
        <div className="h-3 w-14 bg-zinc-100 rounded" />
        <div className="w-4 h-4 bg-zinc-100 rounded" />
      </div>
    </div>
  );
}
