"use client";

import { motion } from "framer-motion";
import type { ProposalStatus } from "@/lib/types/proposals";
import { Inbox, Send, Filter, Search, ArrowUpDown } from "lucide-react";
import { useState, useEffect } from "react";

interface ProposalFiltersProps {
  direction: "all" | "sent" | "received";
  status: ProposalStatus | "all";
  searchQuery: string;
  sortBy: "UpdatedAt" | "CreatedAt" | "Status";
  sortOrder: "asc" | "desc";
  onDirectionChange: (direction: "all" | "sent" | "received") => void;
  onStatusChange: (status: ProposalStatus | "all") => void;
  onSearchChange: (searchQuery: string) => void;
  onSortChange: (sortBy: "UpdatedAt" | "CreatedAt" | "Status", sortOrder: "asc" | "desc") => void;
}

const statusOptions: { value: ProposalStatus | "all"; label: string }[] = [
  { value: "all", label: "All Status" },
  { value: "Pending", label: "Pending" },
  { value: "Accepted", label: "Accepted" },
  { value: "Completed", label: "Completed" },
  { value: "Rejected", label: "Declined" },
  { value: "Cancelled", label: "Cancelled" },
];

const sortOptions: { value: "UpdatedAt" | "CreatedAt" | "Status"; label: string }[] = [
  { value: "UpdatedAt", label: "Recently Updated" },
  { value: "CreatedAt", label: "Recently Created" },
  { value: "Status", label: "Status" },
];

export function ProposalFilters({
  direction,
  status,
  searchQuery,
  sortBy,
  sortOrder,
  onDirectionChange,
  onStatusChange,
  onSearchChange,
  onSortChange,
}: ProposalFiltersProps) {
  const [localSearch, setLocalSearch] = useState(searchQuery);

  // Debounce search input
  useEffect(() => {
    const timer = setTimeout(() => {
      onSearchChange(localSearch);
    }, 300);

    return () => clearTimeout(timer);
  }, [localSearch, onSearchChange]);

  const toggleSortOrder = () => {
    onSortChange(sortBy, sortOrder === "asc" ? "desc" : "asc");
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex flex-col gap-4 mb-6"
    >
      {/* Direction tabs */}
      <div className="flex bg-white/10 backdrop-blur-sm rounded-xl p-1 border border-white/20">
        <button
          onClick={() => onDirectionChange("all")}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
            direction === "all"
              ? "bg-white text-emerald-900 shadow-md"
              : "text-emerald-100 hover:text-white hover:bg-white/10"
          }`}
        >
          <Filter size={16} />
          All
        </button>
        <button
          onClick={() => onDirectionChange("received")}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
            direction === "received"
              ? "bg-white text-emerald-900 shadow-md"
              : "text-emerald-100 hover:text-white hover:bg-white/10"
          }`}
        >
          <Inbox size={16} />
          Received
        </button>
        <button
          onClick={() => onDirectionChange("sent")}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
            direction === "sent"
              ? "bg-white text-emerald-900 shadow-md"
              : "text-emerald-100 hover:text-white hover:bg-white/10"
          }`}
        >
          <Send size={16} />
          Sent
        </button>
      </div>

      {/* Search and filters row */}
      <div className="flex flex-col sm:flex-row gap-4">
        {/* Search input */}
        <div className="relative flex-1">
          <Search
            size={18}
            className="absolute left-3 top-1/2 -translate-y-1/2 text-white/50"
          />
          <input
            type="text"
            value={localSearch}
            onChange={(e) => setLocalSearch(e.target.value)}
            placeholder="Search by username or skill..."
            className="w-full bg-white/10 backdrop-blur-sm text-white placeholder-white/50 border border-white/20 rounded-xl pl-10 pr-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-400 focus:border-transparent"
          />
        </div>

        {/* Status dropdown */}
        <div className="relative">
          <select
            value={status}
            onChange={(e) =>
              onStatusChange(e.target.value as ProposalStatus | "all")
            }
            className="appearance-none bg-white/10 backdrop-blur-sm text-white border border-white/20 rounded-xl px-4 py-2.5 pr-10 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-emerald-400 focus:border-transparent cursor-pointer hover:bg-white/20 transition-colors"
          >
            {statusOptions.map((option) => (
              <option
                key={option.value}
                value={option.value}
                className="bg-emerald-900 text-white"
              >
                {option.label}
              </option>
            ))}
          </select>
          <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
            <svg
              className="w-4 h-4 text-white/70"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </div>
        </div>

        {/* Sort dropdown */}
        <div className="relative">
          <select
            value={sortBy}
            onChange={(e) =>
              onSortChange(
                e.target.value as "UpdatedAt" | "CreatedAt" | "Status",
                sortOrder
              )
            }
            className="appearance-none bg-white/10 backdrop-blur-sm text-white border border-white/20 rounded-xl px-4 py-2.5 pr-10 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-emerald-400 focus:border-transparent cursor-pointer hover:bg-white/20 transition-colors"
          >
            {sortOptions.map((option) => (
              <option
                key={option.value}
                value={option.value}
                className="bg-emerald-900 text-white"
              >
                {option.label}
              </option>
            ))}
          </select>
          <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
            <svg
              className="w-4 h-4 text-white/70"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </div>
        </div>

        {/* Sort order toggle */}
        <button
          onClick={toggleSortOrder}
          className="flex items-center justify-center gap-2 bg-white/10 backdrop-blur-sm text-white border border-white/20 rounded-xl px-4 py-2.5 text-sm font-medium hover:bg-white/20 transition-colors"
          title={sortOrder === "asc" ? "Ascending" : "Descending"}
        >
          <ArrowUpDown size={16} />
          <span className="hidden sm:inline">
            {sortOrder === "asc" ? "↑" : "↓"}
          </span>
        </button>
      </div>
    </motion.div>
  );
}
