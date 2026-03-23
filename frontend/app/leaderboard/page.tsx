"use client";

import { motion } from "framer-motion";
import {
  Trophy,
  Medal,
  Crown,
  Coins,
  ArrowRightLeft,
  Loader2,
} from "lucide-react";
import { useLeaderboard } from "@/lib/hooks/useCredits";
import { useQuery } from "@tanstack/react-query";
import { accountApi } from "@/lib/api/account";

const PODIUM_STYLES = [
  {
    icon: Crown,
    iconColor: "text-yellow-400",
    bg: "from-yellow-400/20 to-amber-500/20",
    border: "border-yellow-400/30",
    order: "order-2",
    heightClass: "h-28",
  },
  {
    icon: Medal,
    iconColor: "text-gray-300",
    bg: "from-gray-400/20 to-gray-500/20",
    border: "border-gray-400/30",
    order: "order-1",
    heightClass: "h-20",
  },
  {
    icon: Medal,
    iconColor: "text-amber-600",
    bg: "from-amber-700/20 to-orange-800/20",
    border: "border-amber-700/30",
    order: "order-3",
    heightClass: "h-16",
  },
];

export default function LeaderboardPage() {
  const { data: leaderboard, isLoading } = useLeaderboard();
  const { data: currentUser } = useQuery({
    queryKey: ["me"],
    queryFn: accountApi.getMe,
  });

  const top3 = leaderboard?.entries.slice(0, 3) ?? [];
  const rest = leaderboard?.entries.slice(3) ?? [];

  return (
    <div className="relative min-h-screen p-4 md:p-8 bg-emerald-950 text-white overflow-hidden">
      {/* Background */}
      <div className="absolute top-0 left-0 w-full h-64 bg-gradient-to-b from-amber-900/20 to-transparent pointer-events-none" />

      <div className="max-w-4xl mx-auto relative z-10">
        {/* Header */}
        <header className="mb-10 text-center">
          <div className="flex items-center justify-center gap-3 mb-2">
            <Trophy className="w-8 h-8 text-amber-400" />
            <h1 className="text-4xl font-heading font-bold text-white">
              Leaderboard
            </h1>
          </div>
          <p className="text-white/50">
            Top earners in the TalentVerse community
          </p>
        </header>

        {isLoading ? (
          <div className="flex justify-center py-20">
            <Loader2 className="w-10 h-10 animate-spin text-amber-400" />
          </div>
        ) : (
          <>
            {/* Podium — top 3 */}
            {top3.length >= 1 && (
              <motion.div
                initial={{ opacity: 0, y: 30 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
                className="flex items-end justify-center gap-4 mb-10"
              >
                {top3.map((entry, idx) => {
                  const style = PODIUM_STYLES[idx];
                  const Icon = style.icon;
                  const isCurrentUser = entry.userId === currentUser?.id;

                  return (
                    <div
                      key={entry.userId}
                      className={`flex flex-col items-center ${style.order} flex-1 max-w-[180px]`}
                    >
                      {/* Avatar */}
                      <div className="relative mb-3">
                        <div
                          className={`w-16 h-16 rounded-2xl border-2 ${style.border} bg-gradient-to-br ${style.bg} flex items-center justify-center text-2xl font-heading font-bold text-white overflow-hidden`}
                        >
                          {entry.profilePictureUrl ? (
                            <img
                              src={entry.profilePictureUrl}
                              alt={entry.username}
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            entry.username.slice(0, 1).toUpperCase()
                          )}
                        </div>
                        <div
                          className={`absolute -top-2 -right-2 w-7 h-7 rounded-full bg-emerald-900 border-2 ${style.border} flex items-center justify-center`}
                        >
                          <Icon className={`w-3.5 h-3.5 ${style.iconColor}`} />
                        </div>
                      </div>
                      {/* Username */}
                      <p
                        className={`font-semibold text-sm text-center truncate max-w-full ${
                          isCurrentUser ? "text-amber-300" : "text-white"
                        }`}
                      >
                        {isCurrentUser ? "You" : entry.username}
                      </p>
                      {/* Pedestal */}
                      <div
                        className={`mt-3 w-full ${style.heightClass} bg-gradient-to-b ${style.bg} border ${style.border} rounded-t-xl flex flex-col items-center justify-center gap-1`}
                      >
                        <span className="text-lg font-heading font-bold text-white">
                          #{entry.rank}
                        </span>
                         <span className="text-xs text-amber-300 font-mono">
                           {entry.creditBalance.toLocaleString()} cr
                         </span>
                      </div>
                    </div>
                  );
                })}
              </motion.div>
            )}

            {/* Ranked Table */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
              className="bg-white/10 backdrop-blur-sm rounded-2xl border border-white/10 overflow-hidden"
            >
              {/* Current user's rank banner (if not in top 3) */}
                  {leaderboard &&
                leaderboard.currentUserRank != null &&
                leaderboard.currentUserRank > 3 && (
                  <div className="p-4 bg-amber-500/10 border-b border-amber-400/20 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="text-amber-400 font-bold font-mono text-lg">
                        #{leaderboard.currentUserRank}
                      </span>
                      <span className="text-white/70 text-sm">Your rank</span>
                    </div>
                    <div className="flex items-center gap-1 text-amber-300 font-mono text-sm font-bold">
                      <Coins className="w-4 h-4" />
                      {(leaderboard.currentUserBalance ?? 0).toLocaleString()}
                    </div>
                  </div>
                )}

              {/* Header row */}
              <div className="grid grid-cols-12 gap-2 px-5 py-3 text-xs text-white/40 uppercase tracking-wider border-b border-white/10">
                <span className="col-span-1">Rank</span>
                <span className="col-span-6">User</span>
                <span className="col-span-3 text-right">Credits</span>
                <span className="col-span-2 text-right">Swaps</span>
              </div>

              {/* Top 3 condensed + rest */}
              {[...top3, ...rest].map((entry) => {
                const isCurrentUser = entry.userId === currentUser?.id;
                return (
                  <div
                    key={entry.userId}
                    className={`grid grid-cols-12 gap-2 px-5 py-3 items-center hover:bg-white/5 transition-colors border-b border-white/5 last:border-0 ${
                      isCurrentUser ? "bg-amber-500/5" : ""
                    }`}
                  >
                    <span
                      className={`col-span-1 font-mono font-bold text-sm ${
                        entry.rank === 1
                          ? "text-yellow-400"
                          : entry.rank === 2
                          ? "text-gray-300"
                          : entry.rank === 3
                          ? "text-amber-600"
                          : "text-white/50"
                      }`}
                    >
                      {entry.rank}
                    </span>
                    <div className="col-span-6 flex items-center gap-3">
                      <div className="w-8 h-8 rounded-lg bg-white/10 flex items-center justify-center text-sm font-bold text-white overflow-hidden shrink-0">
                        {entry.profilePictureUrl ? (
                          <img
                            src={entry.profilePictureUrl}
                            alt={entry.username}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          entry.username.slice(0, 1).toUpperCase()
                        )}
                      </div>
                      <span
                        className={`font-medium text-sm truncate ${
                          isCurrentUser ? "text-amber-300" : "text-white"
                        }`}
                      >
                        {isCurrentUser ? `${entry.username} (You)` : entry.username}
                      </span>
                    </div>
                    <div className="col-span-3 flex items-center justify-end gap-1 text-amber-300 font-mono text-sm font-bold">
                      <Coins className="w-3.5 h-3.5" />
                      {entry.creditBalance.toLocaleString()}
                    </div>
                    <div className="col-span-2 flex items-center justify-end gap-1 text-white/50 text-sm">
                      <ArrowRightLeft className="w-3 h-3" />
                      {entry.completedSwaps}
                    </div>
                  </div>
                );
              })}

              {leaderboard?.entries.length === 0 && (
                <div className="py-16 text-center text-white/40">
                  <Trophy className="w-10 h-10 mx-auto mb-3 opacity-30" />
                  <p>No data yet. Complete swaps to earn credits and appear here!</p>
                </div>
              )}
            </motion.div>
          </>
        )}
      </div>
    </div>
  );
}
