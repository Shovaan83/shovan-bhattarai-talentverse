'use client';

import { motion } from 'framer-motion';
import { Star, ArrowRightLeft, User } from 'lucide-react';
import Link from 'next/link';
import type { PublicUserDto } from '@/lib/types/marketplace';

interface UserCardProps {
  user: PublicUserDto;
  index: number;
}

export function UserCard({ user, index }: UserCardProps) {
  const topOfferedSkills = user.offeredSkills.slice(0, 3);
  const topWantedSkills = user.wantedSkills.slice(0, 3);

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: index * 0.05 }}
    >
      <Link href={`/users/${user.id}`}>
        <div className="bg-gradient-to-br from-emerald-900/40 to-emerald-900/20 rounded-3xl p-6 border border-emerald-800/50 hover:border-emerald-600/50 transition-all hover:shadow-lg hover:shadow-emerald-900/20 group cursor-pointer">
          {/* User Header */}
          <div className="flex items-start gap-4 mb-4">
            <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-emerald-600 to-emerald-800 flex items-center justify-center flex-shrink-0 group-hover:scale-105 transition-transform">
              {user.profilePictureUrl ? (
                <img
                  src={user.profilePictureUrl}
                  alt={user.displayName}
                  className="w-full h-full rounded-2xl object-cover"
                />
              ) : (
                <User className="w-7 h-7 text-emerald-200" />
              )}
            </div>
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-lg truncate group-hover:text-emerald-300 transition-colors">
                {user.displayName}
              </h3>
              <p className="text-emerald-400 text-sm truncate">@{user.userName}</p>
            </div>
            {user.averageRating && (
              <div className="flex items-center gap-1 px-2 py-1 rounded-lg bg-yellow-900/30 border border-yellow-800/50">
                <Star className="w-4 h-4 text-yellow-400 fill-yellow-400" />
                <span className="text-yellow-300 text-sm font-medium">
                  {user.averageRating.toFixed(1)}
                </span>
              </div>
            )}
          </div>

          {/* Bio */}
          {user.bio && (
            <p className="text-emerald-300/70 text-sm mb-4 line-clamp-2">{user.bio}</p>
          )}

          {/* Skills Section */}
          <div className="space-y-3">
            {/* Offered Skills */}
            {topOfferedSkills.length > 0 && (
              <div>
                <p className="text-xs font-medium text-emerald-500 mb-1.5">OFFERS</p>
                <div className="flex flex-wrap gap-1.5">
                  {topOfferedSkills.map((skill) => (
                    <span
                      key={skill.id}
                      className="px-2 py-1 rounded-lg bg-emerald-800/50 text-emerald-200 text-xs border border-emerald-700/50"
                    >
                      {skill.skillName}
                    </span>
                  ))}
                  {user.offeredSkills.length > 3 && (
                    <span className="px-2 py-1 text-emerald-500 text-xs">
                      +{user.offeredSkills.length - 3} more
                    </span>
                  )}
                </div>
              </div>
            )}

            {/* Wanted Skills */}
            {topWantedSkills.length > 0 && (
              <div>
                <p className="text-xs font-medium text-orange-500 mb-1.5">SEEKS</p>
                <div className="flex flex-wrap gap-1.5">
                  {topWantedSkills.map((skill) => (
                    <span
                      key={skill.id}
                      className="px-2 py-1 rounded-lg bg-orange-900/50 text-orange-200 text-xs border border-orange-700/50"
                    >
                      {skill.skillName}
                    </span>
                  ))}
                  {user.wantedSkills.length > 3 && (
                    <span className="px-2 py-1 text-orange-500 text-xs">
                      +{user.wantedSkills.length - 3} more
                    </span>
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Footer Stats */}
          <div className="mt-4 pt-4 border-t border-emerald-800/50 flex items-center justify-between">
            <div className="flex items-center gap-1 text-emerald-400 text-sm">
              <ArrowRightLeft className="w-4 h-4" />
              <span>{user.completedSwaps} swaps completed</span>
            </div>
            <span className="text-xs text-emerald-600 group-hover:text-emerald-400 transition-colors">
              View Profile →
            </span>
          </div>
        </div>
      </Link>
    </motion.div>
  );
}
