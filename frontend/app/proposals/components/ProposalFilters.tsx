"use client";

import { motion } from "framer-motion";
import type { ProposalStatus } from "@/lib/types/proposals";
import { Inbox, Send, Filter } from "lucide-react";

interface ProposalFiltersProps {
  direction: "all" | "sent" | "received";
  status: ProposalStatus | "all";
  onDirectionChange: (direction: "all" | "sent" | "received") => void;
  onStatusChange: (status: ProposalStatus | "all") => void;
}

const statusOptions: { value: ProposalStatus | "all"; label: string }[] = [
  { value: "all", label: "All Status" },
  { value: "Pending", label: "Pending" },
  { value: "Accepted", label: "Accepted" },
  { value: "Completed", label: "Completed" },
  { value: "Rejected", label: "Declined" },
  { value: "Cancelled", label: "Cancelled" },
];

export function ProposalFilters({
  direction,
  status,
  onDirectionChange,
  onStatusChange,
}: ProposalFiltersProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: -10 }}
      animate={{ opacity: 1, y: 0 }}
      className="flex flex-col sm:flex-row gap-4 mb-6"
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
    </motion.div>
  );
}
