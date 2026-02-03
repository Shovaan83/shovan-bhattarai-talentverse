"use client";

import { motion, type Variants } from "framer-motion";
import type { ProposalListItem, ProposalStatus } from "@/lib/types/proposals";
import {
  Check,
  X,
  Clock,
  CheckCircle2,
  XCircle,
  ArrowRightLeft,
  Loader2,
  Star,
} from "lucide-react";

interface ProposalCardProps {
  proposal: ProposalListItem;
  onAccept?: (id: number) => void;
  onDecline?: (id: number) => void;
  onCancel?: (id: number) => void;
  onConfirmCompletion?: (id: number) => void;
  onLeaveReview?: (proposalId: number, otherUsername: string) => void;
  isLoading?: boolean;
  onClick?: (id: number) => void;
}

const statusConfig: Record<
  ProposalStatus,
  { label: string; color: string; bgColor: string; icon: React.ElementType }
> = {
  Pending: {
    label: "Pending",
    color: "text-amber-700",
    bgColor: "bg-amber-100",
    icon: Clock,
  },
  Accepted: {
    label: "Accepted",
    color: "text-blue-700",
    bgColor: "bg-blue-100",
    icon: CheckCircle2,
  },
  Rejected: {
    label: "Declined",
    color: "text-red-700",
    bgColor: "bg-red-100",
    icon: XCircle,
  },
  Completed: {
    label: "Completed",
    color: "text-emerald-700",
    bgColor: "bg-emerald-100",
    icon: CheckCircle2,
  },
  Cancelled: {
    label: "Cancelled",
    color: "text-gray-600",
    bgColor: "bg-gray-100",
    icon: XCircle,
  },
};

const cardVariants: Variants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.4, ease: [0.25, 0.1, 0.25, 1] },
  },
};

export function ProposalCard({
  proposal,
  onAccept,
  onDecline,
  onCancel,
  onConfirmCompletion,
  onLeaveReview,
  isLoading,
  onClick,
}: ProposalCardProps) {
  const status = statusConfig[proposal.status];
  const StatusIcon = status.icon;

  const canAccept = !proposal.isProposer && proposal.status === "Pending";
  const canDecline = !proposal.isProposer && proposal.status === "Pending";
  const canCancel = proposal.isProposer && proposal.status === "Pending";
  const canConfirm =
    proposal.status === "Accepted" &&
    ((proposal.isProposer && !proposal.proposerConfirmed) ||
      (!proposal.isProposer && !proposal.recipientConfirmed));
  const canLeaveReview = proposal.status === "Completed";

  const userConfirmed = proposal.isProposer
    ? proposal.proposerConfirmed
    : proposal.recipientConfirmed;
  const otherConfirmed = proposal.isProposer
    ? proposal.recipientConfirmed
    : proposal.proposerConfirmed;

  return (
    <motion.div
      variants={cardVariants}
      className="bg-white rounded-2xl border border-gray-100 shadow-lg shadow-black/5 overflow-hidden hover:shadow-xl transition-shadow duration-300 cursor-pointer"
      onClick={() => onClick?.(proposal.proposalId)}
    >
      {/* Header with status */}
      <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
        <div className="flex items-center gap-3">
          {proposal.otherProfilePicture ? (
            <img
              src={proposal.otherProfilePicture}
              alt={proposal.otherUsername}
              className="w-10 h-10 rounded-full object-cover border-2 border-gray-100"
            />
          ) : (
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-emerald-600 flex items-center justify-center text-white font-bold">
              {proposal.otherUsername.charAt(0).toUpperCase()}
            </div>
          )}
          <div>
            <p className="font-semibold text-gray-900">
              {proposal.otherUsername}
            </p>
            <p className="text-xs text-gray-500">
              {proposal.isProposer ? "You sent" : "Received from"}
            </p>
          </div>
        </div>

        <div
          className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full ${status.bgColor} ${status.color}`}
        >
          <StatusIcon size={14} />
          <span className="text-xs font-semibold">{status.label}</span>
        </div>
      </div>

      {/* Swap details */}
      <div className="px-5 py-4">
        <div className="flex items-center gap-3">
          {/* Your skill */}
          <div className="flex-1 p-3 rounded-xl bg-emerald-50 border border-emerald-100">
            <p className="text-[10px] uppercase tracking-wider text-emerald-600 font-semibold mb-1">
              {proposal.isProposer ? "You offer" : "They offer"}
            </p>
            <p className="font-semibold text-emerald-800 truncate">
              {proposal.isProposer
                ? proposal.offeringSkillName
                : proposal.receivingSkillName}
            </p>
          </div>

          {/* Arrow */}
          <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center flex-shrink-0">
            <ArrowRightLeft size={14} className="text-gray-500" />
          </div>

          {/* Their skill */}
          <div className="flex-1 p-3 rounded-xl bg-orange-50 border border-orange-100">
            <p className="text-[10px] uppercase tracking-wider text-orange-600 font-semibold mb-1">
              {proposal.isProposer ? "You get" : "They want"}
            </p>
            <p className="font-semibold text-orange-800 truncate">
              {proposal.isProposer
                ? proposal.receivingSkillName
                : proposal.offeringSkillName}
            </p>
          </div>
        </div>

        {/* Completion progress for Accepted proposals */}
        {proposal.status === "Accepted" && (
          <div className="mt-4 p-3 rounded-xl bg-blue-50 border border-blue-100">
            <p className="text-xs font-semibold text-blue-700 mb-2">
              Completion Status
            </p>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <div
                  className={`w-5 h-5 rounded-full flex items-center justify-center ${
                    userConfirmed
                      ? "bg-emerald-500 text-white"
                      : "bg-gray-200 text-gray-400"
                  }`}
                >
                  <Check size={12} />
                </div>
                <span className="text-xs text-gray-600">You</span>
              </div>
              <div className="flex items-center gap-2">
                <div
                  className={`w-5 h-5 rounded-full flex items-center justify-center ${
                    otherConfirmed
                      ? "bg-emerald-500 text-white"
                      : "bg-gray-200 text-gray-400"
                  }`}
                >
                  <Check size={12} />
                </div>
                <span className="text-xs text-gray-600">
                  {proposal.otherUsername}
                </span>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Action buttons */}
      {(canAccept || canDecline || canCancel || canConfirm || canLeaveReview) && (
        <div className="px-5 py-3 bg-gray-50 border-t border-gray-100 flex gap-2">
          {canAccept && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onAccept?.(proposal.proposalId);
              }}
              disabled={isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-emerald-500 hover:bg-emerald-600 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <Check size={16} />
              )}
              Accept
            </button>
          )}
          {canDecline && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onDecline?.(proposal.proposalId);
              }}
              disabled={isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-red-500 hover:bg-red-600 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <X size={16} />
              )}
              Decline
            </button>
          )}
          {canCancel && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onCancel?.(proposal.proposalId);
              }}
              disabled={isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-gray-500 hover:bg-gray-600 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <X size={16} />
              )}
              Cancel
            </button>
          )}
          {canConfirm && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onConfirmCompletion?.(proposal.proposalId);
              }}
              disabled={isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={16} className="animate-spin" />
              ) : (
                <CheckCircle2 size={16} />
              )}
              Confirm Done
            </button>
          )}
          {canLeaveReview && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onLeaveReview?.(proposal.proposalId, proposal.otherUsername);
              }}
              disabled={isLoading}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              <Star size={16} />
              Leave Review
            </button>
          )}
        </div>
      )}

      {/* Timestamp */}
      <div className="px-5 py-2 text-xs text-gray-400 text-right">
        {new Date(proposal.createdAt).toLocaleDateString("en-US", {
          month: "short",
          day: "numeric",
          year: "numeric",
        })}
      </div>
    </motion.div>
  );
}
