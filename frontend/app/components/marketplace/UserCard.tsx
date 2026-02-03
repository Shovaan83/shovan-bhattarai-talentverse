'use client';

import React, { useState } from "react";
import { ArrowRightLeft, ArrowRight, User } from "lucide-react";
import { motion } from "framer-motion";
import Link from "next/link";
import type { PublicUserDto } from "@/lib/types/marketplace";

interface UserCardProps {
  user: PublicUserDto;
}

export function UserCard({ user }: UserCardProps) {
  const [imageError, setImageError] = useState(false);
  const fallbackCover = !user.coverPhotoUrl || imageError;

  // Get skill names for display
  const offerSkills = user.offeredSkills.slice(0, 5).map(s => s.skillName);
  const wantSkills = user.wantedSkills.slice(0, 5).map(s => s.skillName);

  return (
    <motion.div
      whileHover={{ y: -4, scale: 1.02 }}
      transition={{ duration: 0.2, ease: 'easeOut' }}
      className="group relative w-full max-w-[360px] h-[540px] overflow-hidden rounded-[32px] shadow-xl hover:shadow-2xl transition-all duration-300 cursor-pointer"
    >
      <Link href={`/users/${user.id}`} className="absolute inset-0 z-50" aria-label={`View ${user.displayName}'s profile`}>
        <span className="sr-only">View profile</span>
      </Link>
        
        {/* Background Image / Fallback */}
        <div className="absolute inset-0">
          {!fallbackCover ? (
            <img
              src={user.coverPhotoUrl}
              alt={`${user.displayName}'s cover`}
              className="h-full w-full object-cover transition-transform duration-700 group-hover:scale-105"
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="h-full w-full bg-gradient-to-br from-emerald-600 to-emerald-800" />
          )}
        </div>

        {/* Gradient Overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-[#012215] via-[#012215]/80 to-transparent pt-20" />

        {/* Content Container */}
        <div className="absolute inset-0 flex flex-col justify-between p-6">
          
          {/* Top: Swaps Badge */}
          <div className="flex justify-between items-start">
            <div className="flex items-center gap-1.5 rounded-full bg-emerald-900/50 border border-emerald-500/30 px-3 py-1.5 text-xs font-semibold text-emerald-100 backdrop-blur-md shadow-sm">
              <ArrowRightLeft className="h-3.5 w-3.5" />
              <span>{user.completedSwaps} swaps completed</span>
            </div>
          </div>

          {/* Bottom Content */}
          <div className="flex flex-col gap-5 mt-auto">
            
            {/* User Profile */}
            <div className="flex items-center gap-4">
              <div className="relative h-14 w-14 shrink-0 overflow-hidden rounded-full border-2 border-white/20 shadow-md bg-gray-700">
                {user.profilePictureUrl ? (
                  <img 
                    src={user.profilePictureUrl} 
                    alt={user.displayName} 
                    className="h-full w-full object-cover" 
                  />
                ) : (
                  <div className="h-full w-full flex items-center justify-center">
                    <User className="h-6 w-6 text-gray-400" />
                  </div>
                )}
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="text-2xl font-bold text-white leading-tight truncate">
                  {user.displayName}
                </h3>
                <p className="text-sm font-medium text-emerald-300 truncate">
                  @{user.userName}
                </p>
              </div>
            </div>

            {/* Bio */}
            {user.bio && (
              <p className="text-sm text-gray-200 leading-relaxed line-clamp-2">
                {user.bio}
              </p>
            )}

            {/* Tags */}
            <div className="space-y-3">
              {/* Offers */}
              {offerSkills.length > 0 && (
                <div className="flex flex-col gap-1.5">
                  <span className="text-[10px] font-bold uppercase tracking-wider text-emerald-400">
                    Offers
                  </span>
                  <div className="flex flex-wrap gap-1.5 max-h-16 overflow-y-auto scrollbar-hide">
                    {offerSkills.map((skill, i) => (
                      <span 
                        key={i} 
                        className="px-2 py-1 rounded-md bg-emerald-500/20 border border-emerald-500/30 text-[11px] font-medium text-emerald-100 whitespace-nowrap"
                      >
                        {skill}
                      </span>
                    ))}
                    {user.offeredSkills.length > 5 && (
                      <span className="px-2 py-1 rounded-md bg-emerald-500/20 border border-emerald-500/30 text-[11px] font-medium text-emerald-100">
                        +{user.offeredSkills.length - 5}
                      </span>
                    )}
                  </div>
                </div>
              )}

              {/* Seeks */}
              {wantSkills.length > 0 && (
                <div className="flex flex-col gap-1.5">
                  <span className="text-[10px] font-bold uppercase tracking-wider text-orange-400">
                    Seeks
                  </span>
                  <div className="flex flex-wrap gap-1.5 max-h-16 overflow-y-auto scrollbar-hide">
                    {wantSkills.map((seek, i) => (
                      <span 
                        key={i} 
                        className="px-2 py-1 rounded-md bg-orange-500/20 border border-orange-500/30 text-[11px] font-medium text-orange-100 whitespace-nowrap"
                      >
                        {seek}
                      </span>
                    ))}
                    {user.wantedSkills.length > 5 && (
                      <span className="px-2 py-1 rounded-md bg-orange-500/20 border border-orange-500/30 text-[11px] font-medium text-orange-100">
                        +{user.wantedSkills.length - 5}
                      </span>
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Divider */}
            <div className="h-px w-full bg-white/10" />

            {/* CTA */}
            <div 
              className="flex items-center justify-end gap-2 text-sm font-semibold text-white hover:text-emerald-300 transition-colors"
              aria-label={`View ${user.displayName}'s profile`}
            >
              <span>View Profile</span>
              <ArrowRight className="h-4 w-4 transition-transform group-hover:translate-x-1" />
            </div>
          </div>
        </div>

        {/* Hover Border Glow */}
        <div className="absolute inset-0 rounded-[32px] opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none ring-2 ring-emerald-400/30" />
    </motion.div>
  );
}

// Loading Skeleton
export function UserCardSkeleton() {
  return (
    <div className="relative w-full max-w-[360px] h-[540px] rounded-[32px] bg-emerald-900/30 border border-emerald-800/50 animate-pulse overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-t from-emerald-950 to-transparent" />
    </div>
  );
}
