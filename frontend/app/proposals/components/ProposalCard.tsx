"use client";

import type { ProposalListItem, ProposalStatus } from "@/lib/types/proposals";
import { Avatar } from "@/app/components/ui/Avatar";
import { Check, X, CheckCircle2, Loader2, Star } from "lucide-react";

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

const statusStyles: Record<ProposalStatus, string> = {
  Pending: "bg-amber-100 text-amber-700",
  Accepted: "bg-emerald-100 text-emerald-700",
  Completed: "bg-violet-100 text-violet-700",
  Rejected: "bg-red-100 text-red-700",
  Cancelled: "bg-zinc-100 text-zinc-600",
};

function formatRelativeTime(date: string): string {
  const now = new Date();
  const then = new Date(date);
  const diffMs = now.getTime() - then.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return "Just now";
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;

  return then.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}

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

  // Determine skills from user's perspective
  const yourSkill = proposal.isProposer
    ? proposal.offeringSkillName
    : proposal.receivingSkillName;
  const theirSkill = proposal.isProposer
    ? proposal.receivingSkillName
    : proposal.offeringSkillName;

  const hasActions = canAccept || canDecline || canCancel || canConfirm || canLeaveReview;

  return (
    <div
      className="flex items-center gap-4 px-4 py-3 hover:bg-zinc-50 transition-colors cursor-pointer"
      onClick={() => onClick?.(proposal.proposalId)}
    >
      {/* Avatar */}
      <Avatar
        src={proposal.otherProfilePicture}
        name={proposal.otherUsername}
        size={40}
        className="flex-shrink-0"
      />

      {/* User + Skills exchange */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-medium text-zinc-900 truncate">
            {proposal.otherUsername}
          </span>
          <span className="text-zinc-400 text-sm">
            {proposal.isProposer ? "· You sent" : "· Received"}
          </span>
        </div>
        <div className="flex items-center gap-1.5 text-sm mt-0.5">
          <span className="text-zinc-500">Your</span>
          <span className="font-medium text-[#1D9E75] truncate max-w-[120px]">
            {yourSkill}
          </span>
          <span className="text-zinc-400">↔</span>
          <span className="text-zinc-500">Their</span>
          <span className="font-medium text-[#3C2A8A] truncate max-w-[120px]">
            {theirSkill}
          </span>
        </div>
        <div className="mt-1.5 text-xs text-zinc-500">
          Credits: <span className="font-semibold text-zinc-700">{proposal.creditAmount}</span>
        </div>

        {/* Completion indicators for Accepted proposals */}
        {proposal.status === "Accepted" && (
          <div className="flex items-center gap-3 mt-1.5">
            <div className="flex items-center gap-1">
              <div
                className={`w-4 h-4 rounded-full flex items-center justify-center ${
                  userConfirmed
                    ? "bg-[#1D9E75] text-white"
                    : "bg-zinc-200 text-zinc-400"
                }`}
              >
                <Check size={10} />
              </div>
              <span className="text-xs text-zinc-500">You</span>
            </div>
            <div className="flex items-center gap-1">
              <div
                className={`w-4 h-4 rounded-full flex items-center justify-center ${
                  otherConfirmed
                    ? "bg-[#1D9E75] text-white"
                    : "bg-zinc-200 text-zinc-400"
                }`}
              >
                <Check size={10} />
              </div>
              <span className="text-xs text-zinc-500 truncate max-w-[80px]">
                {proposal.otherUsername}
              </span>
            </div>
          </div>
        )}
      </div>

      {/* Status badge */}
      <span
        className={`flex-shrink-0 px-2.5 py-1 text-xs font-medium rounded-full ${
          statusStyles[proposal.status as ProposalStatus]
        }`}
      >
        {proposal.status}
      </span>

      {/* Time */}
      <span className="flex-shrink-0 text-xs text-zinc-500 w-16 text-right">
        {formatRelativeTime(proposal.createdAt)}
      </span>

      {/* Actions */}
      {hasActions && (
        <div className="flex items-center gap-1.5 flex-shrink-0">
          {canAccept && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onAccept?.(proposal.proposalId);
              }}
              disabled={isLoading}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1D9E75] hover:bg-[#0F6E56] text-white text-xs font-medium rounded-full transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={12} className="animate-spin" />
              ) : (
                <Check size={12} />
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
              className="flex items-center gap-1.5 px-3 py-1.5 bg-zinc-100 hover:bg-red-50 text-zinc-700 hover:text-red-600 text-xs font-medium rounded-full transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={12} className="animate-spin" />
              ) : (
                <X size={12} />
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
              className="flex items-center gap-1.5 px-3 py-1.5 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 text-xs font-medium rounded-full transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={12} className="animate-spin" />
              ) : (
                <X size={12} />
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
              className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1D9E75] hover:bg-[#0F6E56] text-white text-xs font-medium rounded-full transition-colors disabled:opacity-50"
            >
              {isLoading ? (
                <Loader2 size={12} className="animate-spin" />
              ) : (
                <CheckCircle2 size={12} />
              )}
              Confirm
            </button>
          )}
          {canLeaveReview && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onLeaveReview?.(proposal.proposalId, proposal.otherUsername);
              }}
              disabled={isLoading}
              className="flex items-center gap-1.5 px-3 py-1.5 bg-[#1D9E75] hover:bg-[#0F6E56] text-white text-xs font-medium rounded-full transition-colors disabled:opacity-50"
            >
              <Star size={12} />
              Review
            </button>
          )}
        </div>
      )}
    </div>
  );
}
