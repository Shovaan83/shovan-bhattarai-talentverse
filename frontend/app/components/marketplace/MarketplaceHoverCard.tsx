'use client';

import Link from 'next/link';
import { ArrowRightLeft, ChevronsRight, Star } from 'lucide-react';
import type { PublicSkillDto, PublicUserDto } from '@/lib/types/marketplace';
import VerifiedBadge from '../VerifiedBadge';
import { Avatar } from '@/app/components/ui/Avatar';

interface MarketplaceHoverCardProps {
  user: PublicUserDto;
}

function SkillPills({
  skills,
  tone,
  limit = 3,
}: {
  skills: PublicSkillDto[];
  tone: 'offer' | 'want';
  limit?: number;
}) {
  const visibleSkills = skills.slice(0, limit);
  const remainingCount = skills.length - visibleSkills.length;
  const className =
    tone === 'offer'
      ? 'bg-emerald-100 text-emerald-800 border-emerald-200'
      : 'bg-orange-100 text-orange-800 border-orange-200';

  if (visibleSkills.length === 0) {
    return <span className="text-xs text-white/70">No skills listed yet</span>;
  }

  return (
    <div className="flex flex-wrap gap-1.5">
      {visibleSkills.map((skill) => (
        <span
          key={skill.id}
          className={`max-w-full truncate rounded-full border px-2 py-1 text-xs font-medium ${className}`}
          title={skill.skillName}
        >
          {skill.skillName}
        </span>
      ))}
      {remainingCount > 0 && (
        <span className="rounded-full border border-white/25 bg-white/15 px-2 py-1 text-xs font-medium text-white">
          +{remainingCount}
        </span>
      )}
    </div>
  );
}

export function MarketplaceHoverCard({ user }: MarketplaceHoverCardProps) {
  const coverImage =
    user.coverPhotoUrl ||
    user.profilePictureUrl ||
    '/brand/brand-pattern.png';
  const topMobileSkills = user.offeredSkills.slice(0, 2);
  const hasRating = typeof user.averageRating === 'number';

  return (
    <Link
      href={`/users/${user.id}`}
      className="group relative block h-[360px] overflow-hidden rounded-xl border border-zinc-200 bg-zinc-900 shadow-sm outline-none transition-all duration-300 hover:-translate-y-1 hover:border-emerald-300 hover:shadow-xl hover:shadow-emerald-950/10 focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-2"
      aria-label={`View ${user.displayName}'s profile`}
    >
      <img
        src={coverImage}
        alt=""
        className="absolute inset-0 h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
      />
      <div className="absolute inset-0 bg-gradient-to-t from-zinc-950 via-zinc-950/35 to-zinc-950/5" />

      <div className="absolute left-4 right-4 top-4 flex items-start justify-between gap-3">
        <Avatar
          src={user.profilePictureUrl}
          name={user.displayName}
          size={56}
          className="border-2 border-white/80 bg-zinc-900 shadow-lg"
          imageClassName="bg-zinc-900"
        />
        {user.isVerified && (
          <span className="rounded-full bg-white/95 px-2 py-1 shadow-sm">
            <VerifiedBadge size="sm" />
          </span>
        )}
      </div>

      <div className="absolute inset-x-0 bottom-0 z-10 p-5 text-white transition-opacity duration-300 md:group-hover:opacity-0 md:group-focus-visible:opacity-0">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="truncate text-xl font-semibold">{user.displayName}</h3>
            {user.isVerified && <VerifiedBadge size="sm" className="md:hidden" />}
          </div>
          <p className="truncate text-sm text-white/75">@{user.userName}</p>
        </div>

        <div className="mt-3 flex flex-wrap items-center gap-2 text-xs font-medium text-white/85">
          <span className="inline-flex items-center gap-1 rounded-full bg-white/15 px-2.5 py-1 backdrop-blur">
            <ArrowRightLeft className="h-3.5 w-3.5" />
            {user.completedSwaps} swaps
          </span>
          {hasRating && (
            <span className="inline-flex items-center gap-1 rounded-full bg-white/15 px-2.5 py-1 backdrop-blur">
              <Star className="h-3.5 w-3.5 fill-yellow-400 text-yellow-400" />
              {user.averageRating!.toFixed(1)}
            </span>
          )}
        </div>

        {topMobileSkills.length > 0 && (
          <div className="mt-3 md:hidden">
            <SkillPills skills={topMobileSkills} tone="offer" limit={2} />
          </div>
        )}
      </div>

      <div className="absolute inset-0 z-20 hidden flex-col justify-end bg-emerald-950/92 p-5 text-white opacity-0 backdrop-blur-sm transition-opacity duration-300 md:flex md:group-hover:opacity-100 md:group-focus-visible:opacity-100">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="truncate text-2xl font-semibold">{user.displayName}</h3>
            {user.isVerified && <VerifiedBadge size="sm" />}
          </div>
          <p className="truncate text-sm text-emerald-100/80">@{user.userName}</p>
        </div>

        <p className="mt-4 line-clamp-3 text-sm leading-6 text-emerald-50/85">
          {user.bio?.trim() || 'No bio added yet.'}
        </p>

        <div className="mt-5 space-y-4">
          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-emerald-200">
              Offers
            </p>
            <SkillPills skills={user.offeredSkills} tone="offer" />
          </div>
          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-orange-200">
              Wants
            </p>
            <SkillPills skills={user.wantedSkills} tone="want" />
          </div>
        </div>

        <div className="mt-6 flex items-center justify-between border-t border-white/15 pt-4">
          <div className="flex items-center gap-3 text-sm text-emerald-50/85">
            <span>{user.completedSwaps} completed swaps</span>
            {hasRating && (
              <span className="inline-flex items-center gap-1">
                <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                {user.averageRating!.toFixed(1)}
              </span>
            )}
          </div>
          <span className="inline-flex items-center gap-1 text-sm font-semibold text-white">
            View Profile
            <ChevronsRight className="h-4 w-4" />
          </span>
        </div>
      </div>
    </Link>
  );
}

export function MarketplaceHoverCardSkeleton() {
  return <div className="h-[360px] rounded-xl bg-zinc-200 animate-pulse" />;
}
