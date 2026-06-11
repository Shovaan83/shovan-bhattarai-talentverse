"use client";

import { useEffect, useState, type ElementType, type FormEvent } from "react";
import { useParams, useRouter } from "next/navigation";
import { motion } from "framer-motion";
import {
  ArrowLeft,
  ArrowRightLeft,
  Calendar,
  Check,
  CheckCircle2,
  Clock,
  Coins,
  Loader2,
  MessageSquare,
  RefreshCw,
  Send,
  X,
  XCircle,
} from "lucide-react";
import {
  useAcceptProposal,
  useCancelProposal,
  useConfirmCompletion,
  useCounterofferProposal,
  useDeclineProposal,
  useProposal,
} from "@/lib/hooks/useProposals";
import { useAuth } from "@/lib/hooks/useAuth";
import { useGoogleCalendarStatus } from "@/lib/hooks/useAppointments";
import { Avatar } from "@/app/components/ui/Avatar";
import ChatPanel from "@/app/proposals/[id]/components/ChatPanel";
import ConnectGoogleCalendar from "@/app/proposals/[id]/components/ConnectGoogleCalendar";
import ScheduleMeetingModal from "@/app/proposals/[id]/components/ScheduleMeetingModal";
import AppointmentsList from "@/app/proposals/[id]/components/AppointmentsList";
import type { ProposalStatus } from "@/lib/types/proposals";

type StatusConfig = {
  label: string;
  badge: string;
  icon: ElementType;
};

const statusConfig: Record<ProposalStatus, StatusConfig> = {
  Pending: {
    label: "Pending",
    badge: "bg-amber-100 text-amber-700",
    icon: Clock,
  },
  Accepted: {
    label: "Accepted",
    badge: "bg-emerald-100 text-emerald-700",
    icon: CheckCircle2,
  },
  Rejected: {
    label: "Declined",
    badge: "bg-red-100 text-red-700",
    icon: XCircle,
  },
  Completed: {
    label: "Completed",
    badge: "bg-violet-100 text-violet-700",
    icon: CheckCircle2,
  },
  Cancelled: {
    label: "Cancelled",
    badge: "bg-zinc-100 text-zinc-600",
    icon: XCircle,
  },
};

const formatDate = (value: string) =>
  new Date(value).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString("en-US", {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });

function getCreditTerms(terms: {
  creditAmount?: number;
  proposerCreditAmount?: number;
  recipientCreditAmount?: number;
  netCreditAmount?: number;
}) {
  const proposerCreditAmount = terms.proposerCreditAmount ?? 0;
  const recipientCreditAmount = terms.recipientCreditAmount ?? terms.creditAmount ?? 0;
  const netCreditAmount =
    terms.netCreditAmount ?? Math.abs(recipientCreditAmount - proposerCreditAmount);

  return { proposerCreditAmount, recipientCreditAmount, netCreditAmount };
}

function SkillRow({
  avatar,
  username,
  role,
  skillName,
  category,
  description,
  tone,
}: {
  avatar?: string;
  username: string;
  role: string;
  skillName: string;
  category: string;
  description?: string;
  tone: "offer" | "request";
}) {
  const toneClass =
    tone === "offer"
      ? "bg-emerald-50 text-[#1D9E75]"
      : "bg-violet-50 text-[#3C2A8A]";

  return (
    <div className="flex items-center gap-4 px-5 py-4">
      <Avatar src={avatar} name={username} size={44} />

      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          <span className="font-semibold text-zinc-900 truncate">{username}</span>
          <span className="text-sm text-zinc-400">· {role}</span>
        </div>
        <div className="mt-1 flex flex-wrap items-center gap-2 text-sm">
          <span className={`rounded-full px-2.5 py-1 font-medium ${toneClass}`}>
            {skillName}
          </span>
          <span className="text-zinc-500">{category}</span>
        </div>
        {description && (
          <p className="mt-2 max-w-2xl text-sm text-zinc-500 line-clamp-2">
            {description}
          </p>
        )}
      </div>
    </div>
  );
}

export default function ProposalDetailPage() {
  const params = useParams();
  const router = useRouter();
  const proposalId = Number(params.id);
  const [isActioning, setIsActioning] = useState(false);
  const [isChatOpen, setIsChatOpen] = useState(false);
  const [isScheduleOpen, setIsScheduleOpen] = useState(false);
  const [showCounterofferForm, setShowCounterofferForm] = useState(false);
  const [counterofferProposerAmount, setCounterofferProposerAmount] = useState("");
  const [counterofferRecipientAmount, setCounterofferRecipientAmount] = useState("");
  const [counterofferMessage, setCounterofferMessage] = useState("");

  const { user: currentUser } = useAuth();
  const { data: proposal, isLoading, isError, refetch, isFetching } = useProposal(proposalId);
  const { data: calendarStatus } = useGoogleCalendarStatus();

  const acceptMutation = useAcceptProposal();
  const declineMutation = useDeclineProposal();
  const cancelMutation = useCancelProposal();
  const confirmMutation = useConfirmCompletion();
  const counterofferMutation = useCounterofferProposal();

  useEffect(() => {
    if (proposal) {
      const creditTerms = getCreditTerms(proposal);
      setCounterofferProposerAmount(creditTerms.proposerCreditAmount.toString());
      setCounterofferRecipientAmount(creditTerms.recipientCreditAmount.toString());
    }
  }, [proposal]);

  const handleAction = async (
    action: "accept" | "decline" | "cancel" | "confirm"
  ) => {
    setIsActioning(true);
    try {
      if (action === "accept") await acceptMutation.mutateAsync(proposalId);
      if (action === "decline") await declineMutation.mutateAsync(proposalId);
      if (action === "cancel") await cancelMutation.mutateAsync(proposalId);
      if (action === "confirm") await confirmMutation.mutateAsync(proposalId);
    } finally {
      setIsActioning(false);
    }
  };

  const handleCounteroffer = async (e: FormEvent) => {
    e.preventDefault();

    const parsedProposerAmount = Number(counterofferProposerAmount);
    const parsedRecipientAmount = Number(counterofferRecipientAmount);
    if (
      !Number.isFinite(parsedProposerAmount) ||
      !Number.isFinite(parsedRecipientAmount) ||
      parsedProposerAmount < 0 ||
      parsedRecipientAmount < 0 ||
      (parsedProposerAmount === 0 && parsedRecipientAmount === 0)
    ) return;

    const netCreditAmount = Math.abs(parsedRecipientAmount - parsedProposerAmount);

    setIsActioning(true);
    try {
      await counterofferMutation.mutateAsync({
        proposalId,
        payload: {
          creditAmount: netCreditAmount,
          proposerCreditAmount: parsedProposerAmount,
          recipientCreditAmount: parsedRecipientAmount,
          message: counterofferMessage || undefined,
        },
      });
      setCounterofferMessage("");
      setShowCounterofferForm(false);
    } finally {
      setIsActioning(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] text-zinc-900">
        <div className="border-b border-zinc-200 bg-white/95">
          <div className="mx-auto max-w-7xl px-6 py-4">
            <div className="h-10 w-72 animate-pulse rounded-lg bg-zinc-100" />
          </div>
        </div>
        <div className="flex items-center justify-center py-24">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-zinc-200 border-t-[#1D9E75]" />
        </div>
      </div>
    );
  }

  if (isError || !proposal) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] text-zinc-900">
        <div className="mx-auto max-w-2xl px-6 py-16">
          <div className="rounded-xl border border-zinc-200 bg-white p-8 text-center">
            <XCircle className="mx-auto mb-4 h-12 w-12 text-red-500" />
            <h2 className="mb-2 text-xl font-semibold text-zinc-900">
              Proposal not found
            </h2>
            <p className="mb-5 text-sm text-zinc-500">
              This proposal does not exist or you do not have access to it.
            </p>
            <button
              onClick={() => router.push("/proposals")}
              className="rounded-xl bg-zinc-900 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-zinc-800"
              type="button"
            >
              Back to Proposals
            </button>
          </div>
        </div>
      </div>
    );
  }

  const status = statusConfig[proposal.status];
  const StatusIcon = status.icon;
  const canChat = proposal.status === "Accepted" || proposal.status === "Completed";
  const canSchedule = proposal.status === "Accepted";
  const creditTerms = getCreditTerms(proposal);
  const netCreditReceiverUserId =
    proposal.netCreditReceiverUserId ||
    (creditTerms.recipientCreditAmount > creditTerms.proposerCreditAmount
      ? proposal.recipientId
      : creditTerms.proposerCreditAmount > creditTerms.recipientCreditAmount
        ? proposal.proposerId
        : "");
  const netCreditReceiver =
    netCreditReceiverUserId === proposal.proposerId
      ? proposal.proposerUsername
      : netCreditReceiverUserId === proposal.recipientId
        ? proposal.recipientUsername
        : "No one";
  const completedSteps =
    (proposal.proposerConfirmed ? 1 : 0) + (proposal.recipientConfirmed ? 1 : 0);
  return (
    <div className="min-h-screen bg-[#FAFAFA] text-zinc-900">
      <div className="sticky top-16 z-10 border-b border-zinc-200 bg-white/95 backdrop-blur-sm">
        <div className="mx-auto max-w-7xl px-6 py-4">
          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              <button
                onClick={() => router.push("/proposals")}
                className="rounded-xl bg-zinc-100 p-2 text-zinc-600 transition-colors hover:bg-zinc-200"
                type="button"
              >
                <ArrowLeft className="h-5 w-5" />
              </button>
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-2xl font-display font-bold text-zinc-900">
                    Proposal #{proposal.proposalId}
                  </h1>
                  <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${status.badge}`}>
                    {status.label}
                  </span>
                </div>
                <p className="text-sm text-zinc-500">
                  Created {formatDate(proposal.createdAt)}
                </p>
              </div>
            </div>

            <button
              onClick={() => refetch()}
              disabled={isFetching}
              className="rounded-xl bg-zinc-100 p-2 text-zinc-600 transition-colors hover:bg-zinc-200 disabled:opacity-50"
              type="button"
            >
              <RefreshCw className={`h-5 w-5 ${isFetching ? "animate-spin" : ""}`} />
            </button>
          </div>
        </div>
      </div>

      <div className="mx-auto max-w-7xl px-6 py-8">
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8 rounded-xl border border-zinc-200 bg-white px-5 py-4"
        >
          <div className="grid gap-5 md:grid-cols-4">
            <div>
              <p className="text-xs uppercase tracking-wide text-zinc-500">Status</p>
              <div className="mt-1 flex items-center gap-2">
                <StatusIcon className="h-5 w-5 text-zinc-500" />
                <span className="font-semibold text-zinc-900">{status.label}</span>
              </div>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wide text-zinc-500">Proposer</p>
              <p className="mt-1 truncate font-semibold text-zinc-900">
                {proposal.proposerUsername}
              </p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wide text-zinc-500">Recipient</p>
              <p className="mt-1 truncate font-semibold text-zinc-900">
                {proposal.recipientUsername}
              </p>
            </div>
            <div>
              <p className="text-xs uppercase tracking-wide text-zinc-500">Net Credits</p>
              <p className="mt-1 flex items-center gap-2 font-semibold text-zinc-900">
                <Coins className="h-4 w-4 text-amber-600" />
                {creditTerms.netCreditAmount}
              </p>
              <p className="mt-1 text-xs text-zinc-500">
                Receiver: {netCreditReceiver}
              </p>
            </div>
          </div>
        </motion.div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
          <main className="space-y-6 lg:col-span-8">
            <motion.section
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.05 }}
              className="overflow-hidden rounded-xl border border-zinc-200 bg-white"
            >
              <div className="flex items-center justify-between gap-3 border-b border-zinc-100 px-5 py-4">
                <div>
                  <h2 className="font-semibold text-zinc-900">Skill Exchange</h2>
                  <p className="text-sm text-zinc-500">
                    What each member is bringing to the swap
                  </p>
                </div>
                <div className="rounded-full bg-zinc-100 p-2 text-zinc-500">
                  <ArrowRightLeft className="h-4 w-4" />
                </div>
              </div>

              <div className="divide-y divide-zinc-100">
                <SkillRow
                  avatar={proposal.proposerProfilePicture}
                  username={proposal.proposerUsername}
                  role="Proposer"
                  skillName={proposal.proposerSkillName}
                  category={proposal.proposerSkillCategory}
                  description={proposal.proposerSkillDescription}
                  tone="offer"
                />
                <SkillRow
                  avatar={proposal.recipientProfilePicture}
                  username={proposal.recipientUsername}
                  role="Recipient"
                  skillName={proposal.recipientSkillName}
                  category={proposal.recipientSkillCategory}
                  description={proposal.recipientSkillDescription}
                  tone="request"
                />
              </div>
            </motion.section>

            <motion.section
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.08 }}
              className="rounded-xl border border-zinc-200 bg-white px-5 py-4"
            >
              <div className="mb-4 flex items-center justify-between gap-3">
                <div>
                  <h2 className="font-semibold text-zinc-900">Credit Terms</h2>
                  <p className="text-sm text-zinc-500">
                    Gross values for each skill and the net settlement after completion
                  </p>
                </div>
                <Coins className="h-5 w-5 text-amber-600" />
              </div>

              <div className="grid gap-3 md:grid-cols-3">
                <div className="rounded-xl bg-emerald-50 px-4 py-3">
                  <p className="text-xs font-medium uppercase tracking-wide text-emerald-700">
                    {proposal.proposerUsername} earns
                  </p>
                  <p className="mt-1 text-xl font-semibold text-zinc-900">
                    {creditTerms.proposerCreditAmount} credits
                  </p>
                </div>
                <div className="rounded-xl bg-orange-50 px-4 py-3">
                  <p className="text-xs font-medium uppercase tracking-wide text-orange-700">
                    {proposal.recipientUsername} earns
                  </p>
                  <p className="mt-1 text-xl font-semibold text-zinc-900">
                    {creditTerms.recipientCreditAmount} credits
                  </p>
                </div>
                <div className="rounded-xl bg-blue-50 px-4 py-3">
                  <p className="text-xs font-medium uppercase tracking-wide text-blue-700">
                    Net settlement
                  </p>
                  <p className="mt-1 text-xl font-semibold text-zinc-900">
                    {creditTerms.netCreditAmount} credits
                  </p>
                  <p className="mt-1 text-xs text-zinc-500">
                    Paid to {netCreditReceiver}
                  </p>
                </div>
              </div>
            </motion.section>

            {(proposal.status === "Accepted" || proposal.status === "Completed") && (
              <motion.section
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 }}
                className="rounded-xl border border-zinc-200 bg-white px-5 py-4"
              >
                <div className="mb-4 flex items-center justify-between gap-3">
                  <div>
                    <h2 className="font-semibold text-zinc-900">Completion</h2>
                    <p className="text-sm text-zinc-500">
                      Both members confirm after the swap is done
                    </p>
                  </div>
                  <span className="text-sm font-medium text-zinc-500">
                    {completedSteps}/2 confirmed
                  </span>
                </div>

                <div className="flex items-center gap-4">
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className="truncate text-sm font-medium text-zinc-900">
                        {proposal.proposerUsername}
                      </span>
                      {proposal.proposerConfirmed && (
                        <Check className="h-4 w-4 text-[#1D9E75]" />
                      )}
                    </div>
                    <p className="text-xs text-zinc-500">
                      {proposal.proposerConfirmed ? "Confirmed" : "Waiting"}
                    </p>
                  </div>
                  <div className="h-2 flex-[2] rounded-full bg-zinc-100">
                    <div
                      className="h-full rounded-full bg-[#1D9E75] transition-all"
                      style={{ width: `${completedSteps * 50}%` }}
                    />
                  </div>
                  <div className="min-w-0 flex-1 text-right">
                    <div className="flex items-center justify-end gap-2">
                      {proposal.recipientConfirmed && (
                        <Check className="h-4 w-4 text-[#1D9E75]" />
                      )}
                      <span className="truncate text-sm font-medium text-zinc-900">
                        {proposal.recipientUsername}
                      </span>
                    </div>
                    <p className="text-xs text-zinc-500">
                      {proposal.recipientConfirmed ? "Confirmed" : "Waiting"}
                    </p>
                  </div>
                </div>
              </motion.section>
            )}

            <motion.section
              id="negotiation-history"
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.15 }}
              className="scroll-mt-28 overflow-hidden rounded-xl border border-amber-200 bg-white shadow-sm"
            >
              <div className="border-b border-amber-100 bg-amber-50/60 px-5 py-4">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <h2 className="font-semibold text-zinc-900">Negotiation History</h2>
                    <p className="text-sm text-zinc-500">
                      Original and counteroffer activity
                    </p>
                  </div>
                  <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-800">
                    Credits
                  </span>
                </div>
                {!proposal.canCounteroffer && proposal.status !== "Pending" && (
                  <p className="mt-3 rounded-lg bg-white px-3 py-2 text-xs text-amber-800">
                    Negotiation is locked after a proposal is accepted.
                  </p>
                )}
              </div>

              <div className="divide-y divide-zinc-100">
                {proposal.counteroffers.length === 0 ? (
                  <div className="px-5 py-6 text-sm text-zinc-500">
                    No counteroffers yet. The current amount is the original proposal amount.
                  </div>
                ) : (
                  proposal.counteroffers.map((counteroffer) => {
                    const counterofferTerms = getCreditTerms(counteroffer);

                    return (
                      <div
                        key={counteroffer.proposalCounterofferId}
                        className="flex items-start justify-between gap-4 px-5 py-4"
                      >
                        <div className="min-w-0">
                          <p className="font-medium text-zinc-900">
                            {counteroffer.offeredByUsername}
                          </p>
                          <p className="text-xs text-zinc-500">
                            {formatDateTime(counteroffer.createdAt)}
                          </p>
                          {counteroffer.message && (
                            <p className="mt-2 whitespace-pre-wrap text-sm text-zinc-600">
                              {counteroffer.message}
                            </p>
                          )}
                        </div>
                        <div className="shrink-0 text-right text-sm">
                          <p className="font-semibold text-zinc-900">
                            Net {counterofferTerms.netCreditAmount} credits
                          </p>
                          <p className="text-xs text-zinc-500">
                            {proposal.proposerUsername}: {counterofferTerms.proposerCreditAmount}
                          </p>
                          <p className="text-xs text-zinc-500">
                            {proposal.recipientUsername}: {counterofferTerms.recipientCreditAmount}
                          </p>
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
            </motion.section>

            {(proposal.status === "Accepted" || proposal.status === "Completed") && (
              <motion.section
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="overflow-hidden rounded-xl border border-zinc-200 bg-white"
              >
                <div className="border-b border-zinc-100 px-5 py-4">
                  <h2 className="font-semibold text-zinc-900">Meetings</h2>
                  <p className="text-sm text-zinc-500">
                    Scheduled sessions for this proposal
                  </p>
                </div>
                <div className="px-5 py-4">
                  <AppointmentsList proposalId={proposalId} />
                </div>
              </motion.section>
            )}
          </main>

          <aside className="space-y-6 lg:col-span-4">
            <motion.section
              initial={{ opacity: 0, x: 12 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.1 }}
              className="rounded-xl border border-zinc-200 bg-white p-5"
            >
              <h2 className="mb-4 font-semibold text-zinc-900">Actions</h2>

              <div className="space-y-2">
                {proposal.canAccept && (
                  <button
                    onClick={() => handleAction("accept")}
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#1D9E75] px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-[#0F6E56] disabled:opacity-50"
                    type="button"
                  >
                    {isActioning ? <Loader2 className="h-4 w-4 animate-spin" /> : <Check className="h-4 w-4" />}
                    Accept
                  </button>
                )}
                {proposal.canDecline && (
                  <button
                    onClick={() => handleAction("decline")}
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-red-50 px-4 py-2.5 text-sm font-medium text-red-700 transition-colors hover:bg-red-100 disabled:opacity-50"
                    type="button"
                  >
                    {isActioning ? <Loader2 className="h-4 w-4 animate-spin" /> : <X className="h-4 w-4" />}
                    Decline
                  </button>
                )}
                {proposal.canCancel && (
                  <button
                    onClick={() => handleAction("cancel")}
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-zinc-100 px-4 py-2.5 text-sm font-medium text-zinc-700 transition-colors hover:bg-zinc-200 disabled:opacity-50"
                    type="button"
                  >
                    {isActioning ? <Loader2 className="h-4 w-4 animate-spin" /> : <X className="h-4 w-4" />}
                    Cancel
                  </button>
                )}
                {proposal.canConfirmCompletion && (
                  <button
                    onClick={() => handleAction("confirm")}
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#1D9E75] px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-[#0F6E56] disabled:opacity-50"
                    type="button"
                  >
                    {isActioning ? <Loader2 className="h-4 w-4 animate-spin" /> : <CheckCircle2 className="h-4 w-4" />}
                    Confirm Completion
                  </button>
                )}
                {proposal.canCounteroffer && (
                  <button
                    onClick={() => setShowCounterofferForm((prev) => !prev)}
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-amber-500 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-amber-600 disabled:opacity-50"
                    type="button"
                  >
                    <Coins className="h-4 w-4" />
                    {showCounterofferForm ? "Hide Negotiation" : "Negotiate Credits"}
                  </button>
                )}
                {!proposal.canCounteroffer && (
                  <a
                    href="#negotiation-history"
                    className="flex w-full items-center justify-center gap-2 rounded-xl border border-amber-300 bg-amber-50 px-4 py-2.5 text-sm font-semibold text-amber-800 shadow-sm transition-colors hover:bg-amber-100"
                  >
                    <Coins className="h-4 w-4" />
                    View Negotiations
                  </a>
                )}
              </div>
            </motion.section>

            {proposal.canCounteroffer && showCounterofferForm && (
              <motion.section
                initial={{ opacity: 0, x: 12 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.15 }}
                className="rounded-xl border border-zinc-200 bg-white p-5"
              >
                <h2 className="mb-4 font-semibold text-zinc-900">Counteroffer</h2>
                <form className="space-y-4" onSubmit={handleCounteroffer}>
                  <div>
                    <label className="mb-1.5 block text-sm font-medium text-zinc-700">
                      Credits for {proposal.proposerUsername}
                    </label>
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={counterofferProposerAmount}
                      onChange={(e) => setCounterofferProposerAmount(e.target.value)}
                      className="w-full rounded-xl border border-zinc-200 bg-white px-3 py-2.5 text-sm text-zinc-900 outline-none transition-colors focus:border-[#1D9E75] focus:ring-2 focus:ring-emerald-100"
                    />
                  </div>
                  <div>
                    <label className="mb-1.5 block text-sm font-medium text-zinc-700">
                      Credits for {proposal.recipientUsername}
                    </label>
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={counterofferRecipientAmount}
                      onChange={(e) => setCounterofferRecipientAmount(e.target.value)}
                      className="w-full rounded-xl border border-zinc-200 bg-white px-3 py-2.5 text-sm text-zinc-900 outline-none transition-colors focus:border-[#1D9E75] focus:ring-2 focus:ring-emerald-100"
                    />
                  </div>
                  <div>
                    <label className="mb-1.5 block text-sm font-medium text-zinc-700">
                      Message
                    </label>
                    <textarea
                      rows={3}
                      value={counterofferMessage}
                      onChange={(e) => setCounterofferMessage(e.target.value)}
                      className="w-full resize-none rounded-xl border border-zinc-200 bg-white px-3 py-2.5 text-sm text-zinc-900 outline-none transition-colors focus:border-[#1D9E75] focus:ring-2 focus:ring-emerald-100"
                      placeholder="Optional note"
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={isActioning}
                    className="flex w-full items-center justify-center gap-2 rounded-xl bg-zinc-900 px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-zinc-800 disabled:opacity-50"
                  >
                    {isActioning ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
                    Send Counteroffer
                  </button>
                </form>
              </motion.section>
            )}

            <motion.section
              initial={{ opacity: 0, x: 12 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2 }}
              className="rounded-xl border border-zinc-200 bg-white p-5"
            >
              <h2 className="mb-4 font-semibold text-zinc-900">Chat & Schedule</h2>
              <div className="space-y-2">
                <button
                  onClick={() => setIsChatOpen((prev) => !prev)}
                  disabled={!canChat}
                  className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#1D9E75] px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-[#0F6E56] disabled:cursor-not-allowed disabled:bg-zinc-100 disabled:text-zinc-400"
                  type="button"
                >
                  <MessageSquare className="h-4 w-4" />
                  {canChat ? (isChatOpen ? "Close Chat" : "Open Chat") : "Chat after acceptance"}
                </button>
                <button
                  onClick={() => setIsScheduleOpen(true)}
                  disabled={!canSchedule || !calendarStatus?.isConnected || calendarStatus?.isRevoked}
                  className="flex w-full items-center justify-center gap-2 rounded-xl bg-zinc-900 px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-zinc-800 disabled:cursor-not-allowed disabled:bg-zinc-100 disabled:text-zinc-400"
                  type="button"
                >
                  <Calendar className="h-4 w-4" />
                  Schedule Meeting
                </button>
              </div>
              {proposal.status === "Accepted" && (
                <div className="mt-4 border-t border-zinc-100 pt-4">
                  <ConnectGoogleCalendar />
                </div>
              )}
            </motion.section>

            <motion.section
              initial={{ opacity: 0, x: 12 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.25 }}
              className="rounded-xl border border-zinc-200 bg-white p-5"
            >
              <h2 className="mb-4 font-semibold text-zinc-900">Timeline</h2>
              <div className="space-y-4">
                <div className="flex gap-3">
                  <div className="mt-0.5 rounded-full bg-emerald-50 p-2 text-[#1D9E75]">
                    <Calendar className="h-4 w-4" />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-zinc-900">Created</p>
                    <p className="text-sm text-zinc-500">
                      {formatDateTime(proposal.createdAt)}
                    </p>
                  </div>
                </div>
                {proposal.updatedAt !== proposal.createdAt && (
                  <div className="flex gap-3">
                    <div className="mt-0.5 rounded-full bg-blue-50 p-2 text-blue-600">
                      <Clock className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-zinc-900">Updated</p>
                      <p className="text-sm text-zinc-500">
                        {formatDateTime(proposal.updatedAt)}
                      </p>
                    </div>
                  </div>
                )}
              </div>
            </motion.section>
          </aside>
        </div>

        {isChatOpen && currentUser && (
          <div className="mt-6 rounded-xl border border-zinc-200 bg-white">
            <ChatPanel
              proposalId={proposalId}
              currentUserId={currentUser.id}
              onClose={() => setIsChatOpen(false)}
            />
          </div>
        )}
      </div>

      {isScheduleOpen && (
        <ScheduleMeetingModal
          proposalId={proposalId}
          onClose={() => setIsScheduleOpen(false)}
        />
      )}
    </div>
  );
}
