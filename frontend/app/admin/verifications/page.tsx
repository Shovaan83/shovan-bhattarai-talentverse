"use client";

import { useState } from "react";
import { FileText, User, Clock, ChevronLeft, ChevronRight } from "lucide-react";
import { usePendingVerifications } from "@/lib/hooks/useVerification";
import ReviewModal from "./components/ReviewModal";
import type { VerificationRequestDto } from "@/lib/types/verification";
import { motion } from "framer-motion";
import { fadeUp } from "@/app/components/motion/variants";

export default function AdminVerificationsPage() {
  const [page, setPage] = useState(1);
  const [selectedRequest, setSelectedRequest] = useState<VerificationRequestDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { data, isLoading, error } = usePendingVerifications(page, 20);

  const handleReview = (request: VerificationRequestDto) => {
    setSelectedRequest(request);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedRequest(null);
  };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl border border-zinc-200 p-6 shadow-sm">
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, idx) => (
            <div key={idx} className="h-24 rounded-xl bg-zinc-100 animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-600">Failed to load verification requests</p>
      </div>
    );
  }

  if (!data || data.requests.length === 0) {
    return (
      <div className="text-center py-12 bg-white rounded-xl border border-zinc-200">
        <div className="w-14 h-14 rounded-full bg-emerald-50 flex items-center justify-center mx-auto mb-4">
          <FileText className="w-7 h-7 text-[#1D9E75]" />
        </div>
        <h3 className="text-lg font-semibold text-zinc-900 mb-2">
          No Pending Requests
        </h3>
        <p className="text-zinc-600">
          All verification requests have been reviewed.
        </p>
      </div>
    );
  }

  return (
    <>
      <motion.div
        className="space-y-6"
        initial={fadeUp.initial}
        animate={fadeUp.animate}
        transition={{ duration: 0.3 }}
      >
        <div className="bg-white rounded-lg shadow-sm border border-zinc-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-xl font-display font-bold text-zinc-900">
                Pending Verification Requests
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                {data.totalCount} total request{data.totalCount !== 1 ? 's' : ''}
              </p>
            </div>
          </div>

          <div className="space-y-4">
            {data.requests.map((request) => (
              <div
                key={request.id}
                className="border border-zinc-200 rounded-lg p-4 hover:border-zinc-300 hover:shadow-sm transition-all"
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4 flex-1">
                    {request.userProfilePictureUrl ? (
                      <img
                        src={request.userProfilePictureUrl}
                        alt={request.userName}
                        className="w-12 h-12 rounded-full object-cover"
                      />
                    ) : (
                      <div className="w-12 h-12 rounded-full bg-zinc-100 flex items-center justify-center">
                        <User className="w-6 h-6 text-zinc-500" />
                      </div>
                    )}

                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-zinc-900">
                          {request.userName}
                        </h3>
                        <span className="px-2 py-0.5 bg-brand-gold-50 text-brand-gold-800 text-xs font-medium rounded-full">
                          {request.status}
                        </span>
                      </div>
                      <p className="text-sm text-gray-600 mt-1">{request.userEmail}</p>

                      <div className="flex items-center gap-2 mt-2 text-xs text-gray-500">
                        <Clock className="w-4 h-4" />
                        <span>
                          Submitted {new Date(request.submittedAt).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                  </div>

                  <button
                    onClick={() => handleReview(request)}
                    className="px-4 py-2 bg-[#1D9E75] text-white font-medium rounded-lg hover:bg-[#15785A] transition-colors flex items-center gap-2"
                  >
                    <FileText className="w-4 h-4" />
                    Review
                  </button>
                </div>
              </div>
            ))}
          </div>

          {data.totalPages > 1 && (
            <div className="flex items-center justify-between mt-6 pt-6 border-t border-zinc-200">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border border-zinc-200 text-zinc-900 font-medium rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
              >
                <ChevronLeft className="w-4 h-4" />
                Previous
              </button>

              <span className="text-sm text-zinc-600">
                Page {page} of {data.totalPages}
              </span>

              <button
                onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="px-4 py-2 border border-zinc-200 text-zinc-900 font-medium rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
              >
                Next
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          )}
        </div>
      </motion.div>

      <ReviewModal
        request={selectedRequest}
        isOpen={isModalOpen}
        onClose={handleCloseModal}
      />
    </>
  );
}
