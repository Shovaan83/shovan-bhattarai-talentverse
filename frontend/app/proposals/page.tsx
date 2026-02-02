"use client";

import { useState, useMemo } from "react";
import { motion, type Variants } from "framer-motion";
import { useRouter } from "next/navigation";
import { ArrowLeft, RefreshCw } from "lucide-react";
import {
  useProposals,
  useAcceptProposal,
  useDeclineProposal,
  useCancelProposal,
  useConfirmCompletion,
} from "@/lib/hooks/useProposals";
import type { ProposalStatus } from "@/lib/types/proposals";
import {
  ProposalCard,
  ProposalFilters,
  ProposalStats,
  EmptyProposals,
} from "./components";

const containerVariants: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.08 },
  },
};

export default function ProposalsPage() {
  const router = useRouter();
  const [direction, setDirection] = useState<"all" | "sent" | "received">(
    "all"
  );
  const [statusFilter, setStatusFilter] = useState<ProposalStatus | "all">(
    "all"
  );
  const [searchQuery, setSearchQuery] = useState("");
  const [sortBy, setSortBy] = useState<"UpdatedAt" | "CreatedAt" | "Status">("UpdatedAt");
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">("desc");
  const [actionLoadingId, setActionLoadingId] = useState<number | null>(null);

  // Fetch proposals
  const { data, isLoading, isError, refetch, isFetching } = useProposals({
    direction: direction === "all" ? undefined : direction,
    status: statusFilter === "all" ? undefined : statusFilter,
    searchQuery: searchQuery || undefined,
    sortBy,
    sortOrder,
    page: 1,
    pageSize: 50,
  });

  // Mutations
  const acceptMutation = useAcceptProposal();
  const declineMutation = useDeclineProposal();
  const cancelMutation = useCancelProposal();
  const confirmMutation = useConfirmCompletion();

  // Calculate stats from all proposals
  const stats = useMemo(() => {
    if (!data?.proposals) {
      return { pending: 0, accepted: 0, completed: 0, total: 0 };
    }
    const proposals = data.proposals;
    return {
      pending: proposals.filter((p) => p.status === "Pending").length,
      accepted: proposals.filter((p) => p.status === "Accepted").length,
      completed: proposals.filter((p) => p.status === "Completed").length,
      total: proposals.length,
    };
  }, [data?.proposals]);

  // Action handlers
  const handleAccept = async (id: number) => {
    setActionLoadingId(id);
    try {
      await acceptMutation.mutateAsync(id);
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleDecline = async (id: number) => {
    setActionLoadingId(id);
    try {
      await declineMutation.mutateAsync(id);
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleCancel = async (id: number) => {
    setActionLoadingId(id);
    try {
      await cancelMutation.mutateAsync(id);
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleConfirmCompletion = async (id: number) => {
    setActionLoadingId(id);
    try {
      await confirmMutation.mutateAsync(id);
    } finally {
      setActionLoadingId(null);
    }
  };

  const handleCardClick = (id: number) => {
    // Could navigate to detailed view
    router.push(`/proposals/${id}`);
  };

  return (
    <div className="relative min-h-screen p-4 md:p-8 bg-emerald-950 text-white overflow-hidden selection:bg-emerald-200 selection:text-emerald-950">
      {/* Background decorations */}
      <div className="absolute top-0 right-0 w-1/2 h-full bg-gradient-to-l from-emerald-900/50 to-transparent pointer-events-none" />
      <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-emerald-800/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute top-1/3 right-1/4 w-64 h-64 bg-orange-500/10 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        {/* Header */}
        <header className="mb-8">
          <div className="flex items-center gap-4 mb-2">
            <button
              onClick={() => router.push("/profile")}
              className="p-2 rounded-xl bg-white/10 hover:bg-white/20 transition-colors"
            >
              <ArrowLeft size={20} />
            </button>
            <div className="flex-1">
              <h1 className="text-3xl font-heading font-bold text-white">
                My Proposals
              </h1>
              <p className="text-emerald-200/80 font-sans">
                Manage your skill swap proposals
              </p>
            </div>
            <button
              onClick={() => refetch()}
              disabled={isFetching}
              className="p-2 rounded-xl bg-white/10 hover:bg-white/20 transition-colors disabled:opacity-50"
            >
              <RefreshCw
                size={20}
                className={isFetching ? "animate-spin" : ""}
              />
            </button>
          </div>
        </header>

        {/* Stats */}
        <ProposalStats
          pending={stats.pending}
          accepted={stats.accepted}
          completed={stats.completed}
          total={stats.total}
        />

        {/* Filters */}
        <ProposalFilters
          direction={direction}
          status={statusFilter}
          searchQuery={searchQuery}
          sortBy={sortBy}
          sortOrder={sortOrder}
          onDirectionChange={setDirection}
          onStatusChange={setStatusFilter}
          onSearchChange={setSearchQuery}
          onSortChange={(newSortBy, newSortOrder) => {
            setSortBy(newSortBy);
            setSortOrder(newSortOrder);
          }}
        />

        {/* Content */}
        {isLoading ? (
          <div className="flex items-center justify-center py-20">
            <div className="w-12 h-12 border-4 border-emerald-200 border-t-emerald-500 rounded-full animate-spin" />
          </div>
        ) : isError ? (
          <div className="bg-red-50 border border-red-200 rounded-2xl p-6 text-center">
            <p className="text-red-700 font-medium">
              Failed to load proposals. Please try again.
            </p>
            <button
              onClick={() => refetch()}
              className="mt-4 px-4 py-2 bg-red-100 hover:bg-red-200 text-red-700 rounded-xl font-medium transition-colors"
            >
              Retry
            </button>
          </div>
        ) : !data?.proposals || data.proposals.length === 0 ? (
          <EmptyProposals
            direction={direction}
            onCreateProposal={() => router.push("/marketplace")}
          />
        ) : (
          <motion.div
            variants={containerVariants}
            initial="hidden"
            animate="visible"
            className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6"
          >
            {data.proposals.map((proposal) => (
              <ProposalCard
                key={proposal.proposalId}
                proposal={proposal}
                onAccept={handleAccept}
                onDecline={handleDecline}
                onCancel={handleCancel}
                onConfirmCompletion={handleConfirmCompletion}
                isLoading={actionLoadingId === proposal.proposalId}
                onClick={handleCardClick}
              />
            ))}
          </motion.div>
        )}

        {/* Pagination info */}
        {data && data.totalPages > 1 && (
          <div className="mt-8 text-center text-emerald-200/60 text-sm">
            Showing {data.proposals.length} of {data.totalCount} proposals
          </div>
        )}
      </div>
    </div>
  );
}
