"use client";

import { useAdminDashboard } from "@/lib/hooks/useAdmin";
import {
  Users,
  ArrowRightLeft,
  Coins,
  Star,
  ShieldCheck,
  TrendingUp,
  Loader2,
} from "lucide-react";
import dynamic from "next/dynamic";

// Lazy-load D3 charts (they use browser APIs)
const LineChart = dynamic(() => import("./components/charts/LineChart"), { ssr: false });
const BarChart = dynamic(() => import("./components/charts/BarChart"), { ssr: false });
const DonutChart = dynamic(() => import("./components/charts/DonutChart"), { ssr: false });

export default function AdminDashboardPage() {
  const { data, isLoading, error } = useAdminDashboard();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-indigo-500" />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="text-center py-20">
        <p className="text-red-600">Failed to load dashboard data.</p>
      </div>
    );
  }

  const kpis = [
    {
      label: "Total Users",
      value: data.totalUsers,
      icon: Users,
      color: "bg-indigo-50 text-indigo-600",
      accent: "border-indigo-200",
    },
    {
      label: "Active (30d)",
      value: data.activeUsersLast30Days,
      icon: TrendingUp,
      color: "bg-emerald-50 text-emerald-600",
      accent: "border-emerald-200",
    },
    {
      label: "Completed Swaps",
      value: data.totalSwaps,
      icon: ArrowRightLeft,
      color: "bg-violet-50 text-violet-600",
      accent: "border-violet-200",
    },
    {
      label: "Credits Circulated",
      value: Math.floor(data.totalCreditsCirculated),
      icon: Coins,
      color: "bg-amber-50 text-amber-600",
      accent: "border-amber-200",
    },
    {
      label: "Pending Verifications",
      value: data.pendingVerifications,
      icon: ShieldCheck,
      color: "bg-blue-50 text-blue-600",
      accent: "border-blue-200",
    },
    {
      label: "Total Reviews",
      value: data.totalReviews,
      icon: Star,
      color: "bg-pink-50 text-pink-600",
      accent: "border-pink-200",
    },
  ];

  return (
    <div className="space-y-8">
      {/* KPI Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
        {kpis.map((kpi) => (
          <div
            key={kpi.label}
            className={`bg-white rounded-xl border ${kpi.accent} p-4 shadow-sm hover:shadow-md transition-shadow`}
          >
            <div className="flex items-center gap-2 mb-2">
              <div className={`p-1.5 rounded-lg ${kpi.color}`}>
                <kpi.icon className="w-4 h-4" />
              </div>
            </div>
            <p className="text-2xl font-bold text-gray-900">
              {kpi.value.toLocaleString()}
            </p>
            <p className="text-xs text-gray-500 mt-0.5">{kpi.label}</p>
          </div>
        ))}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* User Growth Line Chart */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            User Growth (Last 12 Months)
          </h3>
          {data.userGrowth.length > 0 ? (
            <LineChart data={data.userGrowth} />
          ) : (
            <p className="text-gray-400 text-sm">No data available</p>
          )}
        </div>

        {/* Proposal Distribution Donut */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            Proposal Status Distribution
          </h3>
          <DonutChart data={data.proposalStats} />
        </div>
      </div>

      {/* Top Skills Bar Chart */}
      <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Top 10 Skills by User Count
        </h3>
        {data.topSkills.length > 0 ? (
          <BarChart data={data.topSkills} />
        ) : (
          <p className="text-gray-400 text-sm">No data available</p>
        )}
      </div>
    </div>
  );
}
