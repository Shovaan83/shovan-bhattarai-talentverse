import type { ProposalStatus } from "@/lib/types/proposals";

type StatusBadgeStatus = ProposalStatus | "Declined";
type ExtendedStatusBadgeStatus = StatusBadgeStatus | "Rejected";

type BadgeConfig = {
  bg: string;
  text: string;
  dot: string;
  label: string;
};

const statusConfig: Record<"Pending" | "Accepted" | "Declined" | "Cancelled" | "Completed", BadgeConfig> = {
  Pending: {
    bg: "bg-brand-gold-50",
    text: "text-brand-gold-800",
    dot: "bg-brand-gold-600",
    label: "Pending",
  },
  Accepted: {
    bg: "bg-brand-teal-50",
    text: "text-brand-teal-700",
    dot: "bg-brand-teal-500",
    label: "Accepted",
  },
  Declined: {
    bg: "bg-red-50",
    text: "text-red-800",
    dot: "bg-red-500",
    label: "Declined",
  },
  Cancelled: {
    bg: "bg-gray-100",
    text: "text-gray-600",
    dot: "bg-gray-400",
    label: "Cancelled",
  },
  Completed: {
    bg: "bg-zinc-100",
    text: "text-zinc-700",
    dot: "bg-zinc-500",
    label: "Completed",
  },
};

function normalizeStatus(status: ExtendedStatusBadgeStatus): keyof typeof statusConfig {
  if (status === "Rejected") {
    return "Declined";
  }
  return status;
}

interface StatusBadgeProps {
  status: ExtendedStatusBadgeStatus;
  className?: string;
}

export function StatusBadge({ status, className = "" }: StatusBadgeProps) {
  const config = statusConfig[normalizeStatus(status)];

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${config.bg} ${config.text} ${className}`.trim()}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${config.dot}`} />
      {config.label}
    </span>
  );
}
