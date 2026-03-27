"use client";

import { Clock, CheckCircle2, ArrowRightLeft, TrendingUp } from "lucide-react";

interface ProposalStatsProps {
  pending: number;
  accepted: number;
  completed: number;
  total: number;
}

export function ProposalStats({
  pending,
  accepted,
  completed,
  total,
}: ProposalStatsProps) {
  const stats = [
    { label: "Pending", value: pending, icon: Clock, color: "text-amber-600" },
    { label: "In Progress", value: accepted, icon: ArrowRightLeft, color: "text-[#3C2A8A]" },
    { label: "Completed", value: completed, icon: CheckCircle2, color: "text-[#1D9E75]" },
    { label: "Total", value: total, icon: TrendingUp, color: "text-zinc-600" },
  ];

  return (
    <div className="flex flex-wrap items-center gap-6 sm:gap-8 py-4 mb-6">
      {stats.map((stat, index) => (
        <div key={stat.label} className="flex items-center gap-6 sm:gap-8">
          <div className="flex items-center gap-2">
            <stat.icon className={`w-4 h-4 ${stat.color}`} />
            <div>
              <p className={`text-2xl font-semibold ${stat.color}`}>
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
