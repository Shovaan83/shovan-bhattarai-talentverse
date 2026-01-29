"use client";

import { motion, type Variants } from "framer-motion";
import {
  Clock,
  CheckCircle2,
  ArrowRightLeft,
  XCircle,
  TrendingUp,
} from "lucide-react";

interface ProposalStatsProps {
  pending: number;
  accepted: number;
  completed: number;
  total: number;
}

const containerVariants: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const itemVariants: Variants = {
  hidden: { opacity: 0, scale: 0.9 },
  visible: {
    opacity: 1,
    scale: 1,
    transition: { duration: 0.4 },
  },
};

export function ProposalStats({
  pending,
  accepted,
  completed,
  total,
}: ProposalStatsProps) {
  const stats = [
    {
      label: "Pending",
      value: pending,
      icon: Clock,
      gradient: "from-amber-400 to-orange-500",
      bgColor: "bg-amber-50",
      textColor: "text-amber-700",
    },
    {
      label: "In Progress",
      value: accepted,
      icon: ArrowRightLeft,
      gradient: "from-blue-400 to-indigo-500",
      bgColor: "bg-blue-50",
      textColor: "text-blue-700",
    },
    {
      label: "Completed",
      value: completed,
      icon: CheckCircle2,
      gradient: "from-emerald-400 to-teal-500",
      bgColor: "bg-emerald-50",
      textColor: "text-emerald-700",
    },
    {
      label: "Total Swaps",
      value: total,
      icon: TrendingUp,
      gradient: "from-violet-400 to-purple-500",
      bgColor: "bg-violet-50",
      textColor: "text-violet-700",
    },
  ];

  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8"
    >
      {stats.map((stat) => (
        <motion.div
          key={stat.label}
          variants={itemVariants}
          className="bg-white rounded-2xl p-5 shadow-lg shadow-black/5 border border-gray-100 relative overflow-hidden group hover:shadow-xl transition-shadow"
        >
          {/* Background gradient */}
          <div
            className={`absolute top-0 right-0 w-24 h-24 bg-gradient-to-br ${stat.gradient} opacity-10 rounded-full blur-2xl -translate-y-1/2 translate-x-1/2 group-hover:opacity-20 transition-opacity`}
          />

          <div className="relative">
            <div
              className={`w-10 h-10 rounded-xl bg-gradient-to-br ${stat.gradient} flex items-center justify-center mb-3 shadow-lg`}
            >
              <stat.icon className="w-5 h-5 text-white" />
            </div>
            <p className="text-3xl font-heading font-bold text-gray-900">
              {stat.value}
            </p>
            <p className="text-sm text-gray-500 font-medium">{stat.label}</p>
          </div>
        </motion.div>
      ))}
    </motion.div>
  );
}
