"use client";

import { useState } from "react";
import { FileText, User, Clock, ChevronLeft, ChevronRight, Loader2 } from "lucide-react";
import { usePendingVerifications } from "@/lib/hooks/useVerification";
import ReviewModal from "./components/ReviewModal";
import type { VerificationRequestDto } from "@/lib/types/verification";

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
      <div className="flex items-center justify-center py-12">
        <Loader2 className="w-8 h-8 animate-spin text-gray-400" />
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
      <div className="text-center py-12">
        <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
        <h3 className="text-lg font-semibold text-gray-900 mb-2">
          No Pending Requests
        </h3>
        <p className="text-gray-600">
          All verification requests have been reviewed.
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-6">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-xl font-bold text-gray-900">
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
                className="border border-gray-200 rounded-lg p-4 hover:border-blue-300 hover:shadow-sm transition-all"
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
                      <div className="w-12 h-12 rounded-full bg-gray-200 flex items-center justify-center">
                        <User className="w-6 h-6 text-gray-500" />
                      </div>
                    )}

                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold text-gray-900">
                          {request.userName}
                        </h3>
                        <span className="px-2 py-0.5 bg-yellow-100 text-yellow-800 text-xs font-medium rounded">
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
                    className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
                  >
                    <FileText className="w-4 h-4" />
                    Review
                  </button>
                </div>
              </div>
            ))}
          </div>

          {data.totalPages > 1 && (
            <div className="flex items-center justify-between mt-6 pt-6 border-t border-gray-200">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
              >
                <ChevronLeft className="w-4 h-4" />
                Previous
              </button>

              <span className="text-sm text-gray-600">
                Page {page} of {data.totalPages}
              </span>

              <button
                onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))}
                disabled={page === data.totalPages}
                className="px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
              >
                Next
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          )}
        </div>
      </div>

      <ReviewModal
        request={selectedRequest}
        isOpen={isModalOpen}
        onClose={handleCloseModal}
      />
    </>
  );
}
