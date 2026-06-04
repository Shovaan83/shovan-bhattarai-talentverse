"use client";

import { useParams, useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
  ArrowLeft,
  ArrowRightLeft,
  Check,
  X,
  Clock,
  CheckCircle2,
  XCircle,
  User,
  Calendar,
  MessageSquare,
  Loader2,
  Send,
} from "lucide-react";
import { useEffect, useState } from "react";
import {
  useProposal,
  useAcceptProposal,
  useDeclineProposal,
  useCancelProposal,
  useConfirmCompletion,
  useCounterofferProposal,
} from "@/lib/hooks/useProposals";
import { useAuth } from "@/lib/hooks/useAuth";
import ChatPanel from "@/app/proposals/[id]/components/ChatPanel";
import ConnectGoogleCalendar from "@/app/proposals/[id]/components/ConnectGoogleCalendar";
import ScheduleMeetingModal from "@/app/proposals/[id]/components/ScheduleMeetingModal";
import AppointmentsList from "@/app/proposals/[id]/components/AppointmentsList";
import { useGoogleCalendarStatus } from "@/lib/hooks/useAppointments";
import type { ProposalStatus } from "@/lib/types/proposals";

const statusConfig: Record<
  ProposalStatus,
  { label: string; color: string; bgColor: string; borderColor: string; icon: React.ElementType }
> = {
  Pending: {
    label: "Pending Review",
    color: "text-amber-700",
    bgColor: "bg-amber-50",
    borderColor: "border-amber-200",
    icon: Clock,
  },
  Accepted: {
    label: "In Progress",
    color: "text-blue-700",
    bgColor: "bg-blue-50",
    borderColor: "border-blue-200",
    icon: CheckCircle2,
  },
  Rejected: {
    label: "Declined",
    color: "text-red-700",
    bgColor: "bg-red-50",
    borderColor: "border-red-200",
    icon: XCircle,
  },
  Completed: {
    label: "Completed",
    color: "text-emerald-700",
    bgColor: "bg-emerald-50",
    borderColor: "border-emerald-200",
    icon: CheckCircle2,
  },
  Cancelled: {
    label: "Cancelled",
    color: "text-gray-600",
    bgColor: "bg-gray-50",
    borderColor: "border-gray-200",
    icon: XCircle,
  },
};

export default function ProposalDetailPage() {
  const params = useParams();
  const router = useRouter();
  const proposalId = Number(params.id);
  const [isActioning, setIsActioning] = useState(false);
  const [isChatOpen, setIsChatOpen] = useState(false);
  const [isScheduleOpen, setIsScheduleOpen] = useState(false);
  const [counterofferAmount, setCounterofferAmount] = useState("");
  const [counterofferMessage, setCounterofferMessage] = useState("");

  const { user: currentUser } = useAuth();
  const { data: proposal, isLoading, isError } = useProposal(proposalId);
  const { data: calendarStatus } = useGoogleCalendarStatus();

  const acceptMutation = useAcceptProposal();
  const declineMutation = useDeclineProposal();
  const cancelMutation = useCancelProposal();
  const confirmMutation = useConfirmCompletion();
  const counterofferMutation = useCounterofferProposal();

  useEffect(() => {
    if (proposal) {
      setCounterofferAmount(proposal.creditAmount.toString());
    }
  }, [proposal]);

  const handleAction = async (
    action: "accept" | "decline" | "cancel" | "confirm"
  ) => {
    setIsActioning(true);
    try {
      switch (action) {
        case "accept":
          await acceptMutation.mutateAsync(proposalId);
          break;
        case "decline":
          await declineMutation.mutateAsync(proposalId);
          break;
        case "cancel":
          await cancelMutation.mutateAsync(proposalId);
          break;
        case "confirm":
          await confirmMutation.mutateAsync(proposalId);
          break;
      }
    } finally {
      setIsActioning(false);
    }
  };

  const handleCounteroffer = async (e: React.FormEvent) => {
    e.preventDefault();

    const parsedAmount = Number(counterofferAmount);
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      return;
    }

    setIsActioning(true);
    try {
      await counterofferMutation.mutateAsync({
        proposalId,
        payload: {
          creditAmount: parsedAmount,
          message: counterofferMessage || undefined,
        },
      });
      setCounterofferMessage("");
    } finally {
      setIsActioning(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-zinc-200 border-t-[#1D9E75] rounded-full animate-spin" />
      </div>
    );
  }

  if (isError || !proposal) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] p-8">
        <div className="max-w-2xl mx-auto">
          <div className="bg-white border border-zinc-200 rounded-2xl p-8 text-center shadow-sm">
            <XCircle className="w-12 h-12 text-red-500 mx-auto mb-4" />
            <h2 className="text-xl font-bold text-zinc-900 mb-2">
              Proposal not found
            </h2>
            <p className="text-zinc-600 mb-4">
              This proposal doesn't exist or you don't have access to it.
            </p>
            <button
              onClick={() => router.push("/proposals")}
              className="px-4 py-2 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 rounded-xl font-medium transition-colors"
            >
              Back to Proposals
            </button>
          </div>
        </div>
      </div>
    );
  }

  const status = statusConfig[proposal.status as ProposalStatus];
  const StatusIcon = status.icon;

  // Determine current user role
  const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;
  // Note: In a real app, you'd decode the JWT to get the user ID
  // For now, we'll use the action flags from the API

  return (
    <div className="relative min-h-screen p-4 md:p-8 bg-[#FAFAFA] overflow-hidden">
      <div className="max-w-4xl mx-auto relative z-10">
        {/* Header */}
        <motion.header
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <button
            onClick={() => router.push("/proposals")}
            className="flex items-center gap-2 bg-zinc-100 hover:bg-zinc-200 text-zinc-600 px-3 py-2 rounded-lg transition-colors mb-4"
          >
            <ArrowLeft size={20} />
            Back to Proposals
          </button>
          <h1 className="text-3xl font-heading font-bold text-zinc-900">
            Proposal Details
          </h1>
        </motion.header>

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left Column - Main Info */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="lg:col-span-2 space-y-6"
          >
            {/* Status Banner */}
            <div
              className={`${status.bgColor} ${status.borderColor} border rounded-2xl p-6`}
            >
              <div className="flex items-center gap-3">
                <div
                  className={`w-12 h-12 rounded-xl ${status.bgColor} flex items-center justify-center`}
                >
                  <StatusIcon className={`w-6 h-6 ${status.color}`} />
                </div>
                <div>
                  <p className={`font-bold text-lg ${status.color}`}>
                    {status.label}
                  </p>
                  <p className="text-sm text-gray-600">
                    Created on{" "}
                    {new Date(proposal.createdAt).toLocaleDateString("en-US", {
                      weekday: "long",
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })}
                  </p>
                </div>
              </div>
            </div>

            {/* Swap Details Card */}
            <div className="bg-white border border-zinc-200 rounded-2xl shadow-sm overflow-hidden">
              <div className="p-6 border-b border-zinc-200">
                <h2 className="font-heading font-bold text-xl text-zinc-900">
                  Swap Exchange
                </h2>
                <div className="mt-3 inline-flex items-center gap-2 rounded-full bg-zinc-100 px-3 py-1 text-sm text-zinc-700">
                  <span className="font-medium">Proposed credits</span>
                  <span className="font-semibold text-[#1D9E75]">{proposal.creditAmount}</span>
                </div>
              </div>

              <div className="p-6">
                <div className="flex items-stretch gap-4">
                  {/* Proposer Skill */}
                  <div className="flex-1 p-5 rounded-2xl bg-[#1D9E75]/10 border border-[#1D9E75]/20">
                    <div className="flex items-center gap-3 mb-4">
                      {proposal.proposerProfilePicture ? (
                        <img
                          src={proposal.proposerProfilePicture}
                          alt={proposal.proposerUsername}
                          className="w-10 h-10 rounded-full object-cover"
                        />
                      ) : (
                        <div className="w-10 h-10 rounded-full bg-[#1D9E75] flex items-center justify-center text-white font-bold">
                          {proposal.proposerUsername.charAt(0).toUpperCase()}
                        </div>
                      )}
                      <div>
                        <p className="font-semibold text-zinc-900">
                          {proposal.proposerUsername}
                        </p>
                        <p className="text-xs text-[#1D9E75]">Proposer</p>
                      </div>
                    </div>
                    <p className="text-xs uppercase tracking-wider text-[#1D9E75] font-semibold mb-1">
                      Offering
                    </p>
                    <p className="font-bold text-lg text-[#1D9E75]">
                      {proposal.proposerSkillName}
                    </p>
                    <span className="inline-block mt-2 text-xs bg-[#1D9E75]/10 text-[#1D9E75] px-2 py-1 rounded-full">
                      {proposal.proposerSkillCategory}
                    </span>
                    {proposal.proposerSkillDescription && (
                      <p className="mt-3 text-sm text-zinc-600">
                        {proposal.proposerSkillDescription}
                      </p>
                    )}
                  </div>

                  {/* Arrow */}
                  <div className="flex items-center">
                    <div className="w-12 h-12 rounded-full bg-zinc-100 flex items-center justify-center">
                      <ArrowRightLeft className="w-5 h-5 text-zinc-500" />
                    </div>
                  </div>

                  {/* Recipient Skill */}
                  <div className="flex-1 p-5 rounded-2xl bg-[#3C2A8A]/10 border border-[#3C2A8A]/20">
                    <div className="flex items-center gap-3 mb-4">
                      {proposal.recipientProfilePicture ? (
                        <img
                          src={proposal.recipientProfilePicture}
                          alt={proposal.recipientUsername}
                          className="w-10 h-10 rounded-full object-cover"
                        />
                      ) : (
                        <div className="w-10 h-10 rounded-full bg-[#3C2A8A] flex items-center justify-center text-white font-bold">
                          {proposal.recipientUsername.charAt(0).toUpperCase()}
                        </div>
                      )}
                      <div>
                        <p className="font-semibold text-zinc-900">
                          {proposal.recipientUsername}
                        </p>
                        <p className="text-xs text-[#3C2A8A]">Recipient</p>
                      </div>
                    </div>
                    <p className="text-xs uppercase tracking-wider text-[#3C2A8A] font-semibold mb-1">
                      Offering
                    </p>
                    <p className="font-bold text-lg text-[#3C2A8A]">
                      {proposal.recipientSkillName}
                    </p>
                    <span className="inline-block mt-2 text-xs bg-[#3C2A8A]/10 text-[#3C2A8A] px-2 py-1 rounded-full">
                      {proposal.recipientSkillCategory}
                    </span>
                    {proposal.recipientSkillDescription && (
                      <p className="mt-3 text-sm text-zinc-600">
                        {proposal.recipientSkillDescription}
                      </p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Negotiation History */}
            <div className="bg-white border border-zinc-200 rounded-2xl shadow-sm overflow-hidden">
              <div className="px-6 py-4 border-b border-zinc-200">
                <h3 className="font-heading font-bold text-lg text-zinc-900">
                  Negotiation History
                </h3>
              </div>
              <div className="p-6 space-y-4">
                {proposal.counteroffers.length === 0 ? (
                  <p className="text-sm text-zinc-500">
                    No counteroffers yet. The current amount is the original proposal amount.
                  </p>
                ) : (
                  proposal.counteroffers.map((counteroffer) => (
                    <div
                      key={counteroffer.proposalCounterofferId}
                      className="rounded-2xl border border-zinc-200 bg-zinc-50 p-4"
                    >
                      <div className="flex items-center justify-between gap-3">
                        <div>
                          <p className="font-semibold text-zinc-900">
                            {counteroffer.offeredByUsername}
                          </p>
                          <p className="text-xs text-zinc-500">
                            {new Date(counteroffer.createdAt).toLocaleString()}
                          </p>
                        </div>
                        <div className="rounded-full bg-white px-3 py-1 text-sm font-semibold text-[#1D9E75] border border-[#1D9E75]/20">
                          {counteroffer.creditAmount} credits
                        </div>
                      </div>
                      {counteroffer.message && (
                        <p className="mt-3 text-sm text-zinc-600 whitespace-pre-wrap">
                          {counteroffer.message}
                        </p>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>

            {/* Completion Progress (for Accepted proposals) */}
            {proposal.status === "Accepted" && (
              <div className="bg-white border border-zinc-200 rounded-2xl p-6 shadow-sm">
                <h3 className="font-heading font-bold text-lg text-zinc-900 mb-4">
                  Completion Progress
                </h3>
                <div className="flex items-center gap-8">
                  <div className="flex items-center gap-3">
                    <div
                      className={`w-10 h-10 rounded-full flex items-center justify-center ${
                        proposal.proposerConfirmed
                          ? "bg-[#1D9E75] text-white"
                          : "bg-zinc-200 text-zinc-400"
                      }`}
                    >
                      <Check size={20} />
                    </div>
                    <div>
                      <p className="font-medium text-zinc-900">
                        {proposal.proposerUsername}
                      </p>
                      <p className="text-xs text-zinc-500">
                        {proposal.proposerConfirmed
                          ? "Confirmed"
                          : "Not confirmed"}
                      </p>
                    </div>
                  </div>

                  <div className="flex-1 h-1 bg-zinc-200 rounded-full">
                    <div
                      className="h-full bg-[#1D9E75] rounded-full transition-all"
                      style={{
                        width: `${
                          ((proposal.proposerConfirmed ? 1 : 0) +
                            (proposal.recipientConfirmed ? 1 : 0)) *
                          50
                        }%`,
                      }}
                    />
                  </div>

                  <div className="flex items-center gap-3">
                    <div
                      className={`w-10 h-10 rounded-full flex items-center justify-center ${
                        proposal.recipientConfirmed
                          ? "bg-[#1D9E75] text-white"
                          : "bg-zinc-200 text-zinc-400"
                      }`}
                    >
                      <Check size={20} />
                    </div>
                    <div>
                      <p className="font-medium text-zinc-900">
                        {proposal.recipientUsername}
                      </p>
                      <p className="text-xs text-zinc-500">
                        {proposal.recipientConfirmed
                          ? "Confirmed"
                          : "Not confirmed"}
                      </p>
                    </div>
                  </div>
                </div>
                <p className="mt-4 text-sm text-zinc-500 text-center">
                  Both parties must confirm completion to finalize the swap.
                </p>
              </div>
            )}

            {/* Scheduled Meetings */}
            {(proposal.status === "Accepted" || proposal.status === "Completed") && (
              <div className="bg-white border border-zinc-200 rounded-2xl shadow-sm overflow-hidden">
                <div className="px-6 py-4 border-b border-zinc-200">
                  <h3 className="font-heading font-bold text-lg text-zinc-900">Scheduled Meetings</h3>
                </div>
                <div className="p-6">
                  <AppointmentsList proposalId={proposalId} />
                </div>
              </div>
            )}
          </motion.div>

          {/* Right Column - Actions */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="space-y-6"
          >
            {/* Actions Card */}
            <div className="bg-white border border-zinc-200 rounded-2xl p-6 shadow-sm">
              <h3 className="font-heading font-bold text-lg text-zinc-900 mb-4">
                Actions
              </h3>

              <div className="space-y-3">
                {proposal.canAccept && (
                  <button
                    onClick={() => handleAction("accept")}
                    disabled={isActioning}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-[#1D9E75] hover:bg-[#178a65] text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                  >
                    {isActioning ? (
                      <Loader2 size={20} className="animate-spin" />
                    ) : (
                      <Check size={20} />
                    )}
                    Accept Proposal
                  </button>
                )}

                {proposal.canDecline && (
                  <button
                    onClick={() => handleAction("decline")}
                    disabled={isActioning}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-red-500 hover:bg-red-600 text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                  >
                    {isActioning ? (
                      <Loader2 size={20} className="animate-spin" />
                    ) : (
                      <X size={20} />
                    )}
                    Decline Proposal
                  </button>
                )}

                {proposal.canCancel && (
                  <button
                    onClick={() => handleAction("cancel")}
                    disabled={isActioning}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-zinc-500 hover:bg-zinc-600 text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                  >
                    {isActioning ? (
                      <Loader2 size={20} className="animate-spin" />
                    ) : (
                      <X size={20} />
                    )}
                    Cancel Proposal
                  </button>
                )}

                {proposal.canConfirmCompletion && (
                  <button
                    onClick={() => handleAction("confirm")}
                    disabled={isActioning}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-blue-500 hover:bg-blue-600 text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                  >
                    {isActioning ? (
                      <Loader2 size={20} className="animate-spin" />
                    ) : (
                      <CheckCircle2 size={20} />
                    )}
                    Confirm Completion
                  </button>
                )}

                {!proposal.canAccept &&
                  !proposal.canDecline &&
                  !proposal.canCancel &&
                  !proposal.canConfirmCompletion && (
                    <p className="text-center text-zinc-500 py-4">
                      No actions available for this proposal.
                    </p>
                  )}
              </div>
            </div>

            {proposal.canCounteroffer && (
              <div className="bg-white border border-zinc-200 rounded-2xl p-6 shadow-sm">
                <h3 className="font-heading font-bold text-lg text-zinc-900 mb-4">
                  Counteroffer
                </h3>
                <form className="space-y-4" onSubmit={handleCounteroffer}>
                  <div>
                    <label className="block text-sm font-medium text-zinc-700 mb-2">
                      Updated credit amount
                    </label>
                    <input
                      type="number"
                      min="0.01"
                      step="0.01"
                      value={counterofferAmount}
                      onChange={(e) => setCounterofferAmount(e.target.value)}
                      className="w-full px-4 py-3 rounded-xl bg-white border border-zinc-200 text-zinc-900 placeholder-zinc-400 focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75]"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-zinc-700 mb-2">
                      Message (optional)
                    </label>
                    <textarea
                      rows={3}
                      value={counterofferMessage}
                      onChange={(e) => setCounterofferMessage(e.target.value)}
                      className="w-full px-4 py-3 rounded-xl bg-white border border-zinc-200 text-zinc-900 placeholder-zinc-400 focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75] resize-none"
                      placeholder="Explain why you're changing the amount..."
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={isActioning}
                    className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-[#3C2A8A] hover:bg-[#32226d] text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                  >
                    {isActioning ? (
                      <Loader2 size={20} className="animate-spin" />
                    ) : (
                      <Send size={20} />
                    )}
                    Send Counteroffer
                  </button>
                </form>
              </div>
            )}

            {/* Timeline */}
            <div className="bg-white border border-zinc-200 rounded-2xl p-6 shadow-sm">
              <h3 className="font-heading font-bold text-lg text-zinc-900 mb-4">
                Timeline
              </h3>
              <div className="space-y-4">
                <div className="flex items-start gap-3">
                  <div className="w-8 h-8 rounded-full bg-[#1D9E75]/10 flex items-center justify-center flex-shrink-0">
                    <Calendar size={16} className="text-[#1D9E75]" />
                  </div>
                  <div>
                    <p className="font-medium text-zinc-900">Created</p>
                    <p className="text-sm text-zinc-500">
                      {new Date(proposal.createdAt).toLocaleString()}
                    </p>
                  </div>
                </div>
                {proposal.updatedAt !== proposal.createdAt && (
                  <div className="flex items-start gap-3">
                    <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
                      <Clock size={16} className="text-blue-600" />
                    </div>
                    <div>
                      <p className="font-medium text-zinc-900">Last Updated</p>
                      <p className="text-sm text-zinc-500">
                        {new Date(proposal.updatedAt).toLocaleString()}
                      </p>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Chat Panel */}
            {isChatOpen && currentUser && (
              <ChatPanel
                proposalId={proposalId}
                currentUserId={currentUser.id}
                onClose={() => setIsChatOpen(false)}
              />
            )}

            {/* Google Calendar - only show for accepted proposals */}
            {proposal.status === "Accepted" && (
              <div className="bg-white border border-zinc-200 rounded-2xl p-5 shadow-sm">
                <h3 className="font-heading font-bold text-sm text-zinc-900 mb-3">
                  Google Calendar
                </h3>
                <ConnectGoogleCalendar />
              </div>
            )}

            {/* Quick Actions */}
            <div className="bg-white border border-zinc-200 rounded-2xl p-6 shadow-sm">
              <h3 className="font-heading font-bold text-lg text-zinc-900 mb-4">
                Chat & Schedule
              </h3>
              <div className="space-y-3">
                {(proposal.status === "Accepted" || proposal.status === "Completed") ? (
                  <button
                    onClick={() => setIsChatOpen((prev) => !prev)}
                    className="w-full flex items-center gap-3 px-4 py-3 bg-[#1D9E75] hover:bg-[#178a65] text-white rounded-xl transition-colors font-medium"
                  >
                    <MessageSquare size={20} />
                    {isChatOpen ? "Close Chat" : "Open Chat"}
                  </button>
                ) : (
                  <button
                    disabled
                    className="w-full flex items-center gap-3 px-4 py-3 bg-zinc-100 text-zinc-400 rounded-xl cursor-not-allowed"
                    title="Chat is available once the proposal is accepted"
                  >
                    <MessageSquare size={20} />
                    Chat (Accept first)
                  </button>
                )}
                {proposal.status === "Accepted" ? (
                  <button
                    onClick={() => setIsScheduleOpen(true)}
                    disabled={!calendarStatus?.isConnected || calendarStatus?.isRevoked}
                    title={!calendarStatus?.isConnected ? "Connect Google Calendar first" : undefined}
                    className="w-full flex items-center gap-3 px-4 py-3 bg-zinc-900 hover:bg-zinc-800 text-white rounded-xl transition-colors font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <Calendar size={20} />
                    Schedule Meeting
                  </button>
                ) : (
                  <button
                    disabled
                    className="w-full flex items-center gap-3 px-4 py-3 bg-zinc-100 text-zinc-400 rounded-xl cursor-not-allowed"
                    title="Schedule a meeting once the proposal is accepted"
                  >
                    <Calendar size={20} />
                    Schedule Meeting
                  </button>
                )}
              </div>
            </div>
          </motion.div>
        </div>
      </div>

      {/* Schedule Meeting Modal */}
      {isScheduleOpen && (
        <ScheduleMeetingModal
          proposalId={proposalId}
          onClose={() => setIsScheduleOpen(false)}
        />
      )}
    </div>
  );
}
