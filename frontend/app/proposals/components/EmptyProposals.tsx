"use client";

import { motion } from "framer-motion";
import { ArrowRightLeft, Plus } from "lucide-react";

interface EmptyProposalsProps {
  direction: "all" | "sent" | "received";
  onCreateProposal?: () => void;
}

export function EmptyProposals({
  direction,
  onCreateProposal,
}: EmptyProposalsProps) {
  const messages = {
    all: {
      title: "No proposals yet",
      description:
        "Start by browsing skills and sending a swap proposal to someone!",
    },
    sent: {
      title: "No sent proposals",
      description: "You haven't sent any proposals yet. Find skills to swap!",
    },
    received: {
      title: "No received proposals",
      description:
        "You haven't received any proposals yet. Make sure you have skills to offer!",
    },
  };

  const { title, description } = messages[direction];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-3xl p-12 shadow-lg shadow-black/5 border border-gray-100 text-center"
    >
      <div className="w-20 h-20 mx-auto mb-6 rounded-full bg-gradient-to-br from-emerald-100 to-emerald-200 flex items-center justify-center">
        <ArrowRightLeft className="w-10 h-10 text-emerald-600" />
      </div>
      <h3 className="text-xl font-heading font-bold text-gray-900 mb-2">
        {title}
      </h3>
      <p className="text-gray-500 mb-6 max-w-md mx-auto">{description}</p>
      {onCreateProposal && (
        <button
          onClick={onCreateProposal}
          className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-500 hover:bg-emerald-600 text-white font-semibold rounded-xl transition-colors shadow-lg shadow-emerald-500/30"
        >
          <Plus size={20} />
          Browse Skills
        </button>
      )}
    </motion.div>
  );
}
