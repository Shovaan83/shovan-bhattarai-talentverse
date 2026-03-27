"use client";

import { useState } from "react";
import { X, CheckCircle, XCircle, Loader2, FileText, User, Mail, Calendar } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { useReviewVerification } from "@/lib/hooks/useVerification";
import type { VerificationRequestDto } from "@/lib/types/verification";

interface ReviewModalProps {
  request: VerificationRequestDto | null;
  isOpen: boolean;
  onClose: () => void;
}

export default function ReviewModal({ request, isOpen, onClose }: ReviewModalProps) {
  const [isApproved, setIsApproved] = useState<boolean | null>(null);
  const [rejectionReason, setRejectionReason] = useState("");
  const [adminNotes, setAdminNotes] = useState("");

  const reviewMutation = useReviewVerification();

  const handleClose = () => {
    setIsApproved(null);
    setRejectionReason("");
    setAdminNotes("");
    onClose();
  };

  const handleReview = async (approved: boolean) => {
    if (!request) return;

    if (!approved && !rejectionReason.trim()) {
      alert("Please provide a rejection reason");
      return;
    }

    try {
      await reviewMutation.mutateAsync({
        id: request.id,
        dto: {
          isApproved: approved,
          adminNotes: adminNotes.trim() || undefined,
          rejectionReason: !approved ? rejectionReason.trim() : undefined,
        },
      });

      handleClose();
    } catch (error) {
      console.error("Review failed:", error);
    }
  };

  if (!request) return null;

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleClose}
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
          />

          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto"
            >
              <div className="flex items-center justify-between p-6 border-b border-gray-100">
                <div>
                  <h2 className="text-2xl font-display font-bold text-zinc-900">
                    Review Verification Request
                  </h2>
                  <p className="text-gray-600 mt-1">Request ID: {request.id}</p>
                </div>
                <button
                  onClick={handleClose}
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5 text-gray-500" />
                </button>
              </div>

              <div className="p-6 space-y-6">
                <div className="grid grid-cols-2 gap-4">
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-blue-50 rounded-lg">
                      <User className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Username</p>
                      <p className="text-sm font-medium text-gray-900">{request.userName}</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-purple-50 rounded-lg">
                      <Mail className="w-5 h-5 text-purple-600" />
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Email</p>
                      <p className="text-sm font-medium text-gray-900">{request.userEmail}</p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3 col-span-2">
                    <div className="p-2 bg-green-50 rounded-lg">
                      <Calendar className="w-5 h-5 text-green-600" />
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Submitted</p>
                      <p className="text-sm font-medium text-gray-900">
                        {new Date(request.submittedAt).toLocaleString()}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <FileText className="w-5 h-5 text-gray-600" />
                    <h3 className="text-sm font-semibold text-gray-900">Verification Document</h3>
                  </div>

                  <div className="border rounded-lg overflow-hidden">
                    {request.documentUrl.endsWith('.pdf') ? (
                      <iframe
                        src={request.documentUrl}
                        className="w-full h-96"
                        title="Verification Document"
                      />
                    ) : (
                      <img
                        src={request.documentUrl}
                        alt="Verification Document"
                        className="w-full h-auto"
                      />
                    )}
                  </div>

                  <a
                    href={request.documentUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-blue-600 hover:text-blue-700 hover:underline inline-block"
                  >
                    Open in new tab →
                  </a>
                </div>

                {isApproved === null ? (
                  <div className="flex gap-3">
                    <button
                      onClick={() => setIsApproved(false)}
                      className="flex-1 px-6 py-3 border-2 border-red-500 text-red-600 font-medium rounded-lg hover:bg-red-50 transition-colors flex items-center justify-center gap-2"
                    >
                      <XCircle className="w-5 h-5" />
                      Reject
                    </button>
                    <button
                      onClick={() => setIsApproved(true)}
                      className="flex-1 px-6 py-3 bg-[#1D9E75] text-white font-medium rounded-lg hover:bg-[#15785A] transition-colors flex items-center justify-center gap-2"
                    >
                      <CheckCircle className="w-5 h-5" />
                      Approve
                    </button>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {!isApproved && (
                      <div>
                        <label className="block text-sm font-semibold text-gray-900 mb-2">
                          Rejection Reason <span className="text-red-500">*</span>
                        </label>
                        <textarea
                          value={rejectionReason}
                          onChange={(e) => setRejectionReason(e.target.value)}
                          rows={3}
                          placeholder="Explain why this request is being rejected..."
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-red-500 resize-none"
                          maxLength={500}
                        />
                        <p className="text-xs text-gray-500 mt-1">
                          {rejectionReason.length} / 500
                        </p>
                      </div>
                    )}

                    <div>
                      <label className="block text-sm font-semibold text-gray-900 mb-2">
                        Admin Notes (Optional)
                      </label>
                      <textarea
                        value={adminNotes}
                        onChange={(e) => setAdminNotes(e.target.value)}
                        rows={2}
                        placeholder="Internal notes (not visible to user)..."
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                        maxLength={1000}
                      />
                    </div>

                    <div className="flex gap-3">
                      <button
                        onClick={() => setIsApproved(null)}
                        className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
                      >
                        Back
                      </button>
                      <button
                        onClick={() => handleReview(isApproved)}
                        disabled={reviewMutation.isPending}
                        className={`flex-1 px-6 py-3 font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2 ${
                          isApproved
                              ? 'bg-[#1D9E75] text-white hover:bg-[#15785A]'
                            : 'bg-red-600 text-white hover:bg-red-700'
                        }`}
                      >
                        {reviewMutation.isPending ? (
                          <>
                            <Loader2 className="w-5 h-5 animate-spin" />
                            Processing...
                          </>
                        ) : (
                          <>
                            {isApproved ? <CheckCircle className="w-5 h-5" /> : <XCircle className="w-5 h-5" />}
                            Confirm {isApproved ? 'Approval' : 'Rejection'}
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </motion.div>
          </div>
        </>
      )}
    </AnimatePresence>
  );
}
