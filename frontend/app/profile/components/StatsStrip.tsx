"use client";

import { motion } from "framer-motion";
import { Coins, Star, ArrowRightLeft } from "lucide-react";

interface StatsStripProps {
  swapCredits: number;
  reputationScore: number;
  totalSwaps: number;
}

export function StatsStrip({
  swapCredits,
  reputationScore,
  totalSwaps,
}: StatsStripProps) {
  const stats = [
    {
      label: "Swap Credits",
      value: swapCredits,
      icon: Coins,
      color: "from-amber-400 to-orange-500",
      bgGlow: "bg-amber-400/20",
    },
    {
      label: "Reputation",
      value: `${reputationScore}%`,
      icon: Star,
      color: "from-emerald-400 to-teal-500",
      bgGlow: "bg-emerald-400/20",
    },
    {
      label: "Total Swaps",
      value: totalSwaps,
      icon: ArrowRightLeft,
      color: "from-violet-400 to-purple-500",
      bgGlow: "bg-violet-400/20",
    },
  ];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.2 }}
      className="md:col-span-2 bg-white/70 backdrop-blur-xl rounded-3xl p-6 border border-white/50 shadow-lg shadow-gray-200/30 relative overflow-hidden"
    >
      {/* Glassmorphism background effect */}
      <div className="absolute inset-0 bg-gradient-to-r from-emerald-50/50 via-white/50 to-orange-50/50 pointer-events-none"></div>

      {/* Animated gradient border */}
      <div className="absolute inset-0 rounded-3xl bg-gradient-to-r from-emerald-200 via-purple-200 to-orange-200 opacity-30 blur-sm -z-10"></div>

      <div className="relative grid grid-cols-3 gap-4">
        {stats.map((stat, index) => (
          <motion.div
            key={stat.label}
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.4, delay: 0.3 + index * 0.1 }}
            className="flex flex-col items-center p-4 rounded-2xl hover:bg-white/60 transition-colors duration-300"
          >
            <div
              className={`w-12 h-12 rounded-2xl bg-gradient-to-br ${stat.color} flex items-center justify-center mb-3 shadow-lg`}
            >
              <stat.icon className="w-6 h-6 text-white" />
            </div>
            <span className="font-heading text-2xl font-bold text-gray-900">
              {stat.value}
            </span>
            <span className="text-xs text-gray-500 font-medium uppercase tracking-wider">
              {stat.label}
            </span>
          </motion.div>
        ))}
      </div>
    </motion.div>
  );
}
