"use client";

import { useState } from "react";
import {
  Search,
  Trash2,
  XCircle,
  Loader2,
  ChevronLeft,
  ChevronRight,
  Flag,
  Star,
  AlertTriangle,
  X,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { fadeUp, scaleIn } from "@/app/components/motion/variants";
import {
  useFlaggedContent,
  useAdminSkills,
  useAdminReviews,
  useRemoveSkill,
  useRemoveReview,
  useDismissReport,
} from "@/lib/hooks/useAdmin";
import type { AdminSkillDto, AdminReviewDto } from "@/lib/types/admin";
import { toast } from "react-hot-toast";
import { AdminStatusBadge } from "@/app/components/ui/AdminStatusBadge";

type Tab = "reports" | "skills" | "reviews";

export default function AdminModerationPage() {
  const [tab, setTab] = useState<Tab>("reports");
  const [skillSearch, setSkillSearch] = useState("");
  const [debouncedSkillSearch, setDebouncedSkillSearch] = useState("");
  const [reviewSearch, setReviewSearch] = useState("");
  const [debouncedReviewSearch, setDebouncedReviewSearch] = useState("");
  const [reportPage, setReportPage] = useState(1);
  const [skillPage, setSkillPage] = useState(1);
  const [reviewPage, setReviewPage] = useState(1);

  // Remove modal
  const [removeModal, setRemoveModal] = useState<{
    type: "skill" | "review";
    id: number;
    label: string;
  } | null>(null);
  const [removeReason, setRemoveReason] = useState("");

  // Hooks
  const flaggedContent = useFlaggedContent(reportPage, 20);
  const adminSkills = useAdminSkills(debouncedSkillSearch || undefined, skillPage, 20);
  const adminReviews = useAdminReviews(debouncedReviewSearch || undefined, reviewPage, 20);
  const removeSkill = useRemoveSkill();
  const removeReview = useRemoveReview();
  const dismissReport = useDismissReport();

  // Debounce helpers
  const handleSkillSearch = (val: string) => {
    setSkillSearch(val);
    setSkillPage(1);
    setTimeout(() => setDebouncedSkillSearch(val), 400);
  };

  const handleReviewSearch = (val: string) => {
    setReviewSearch(val);
    setReviewPage(1);
    setTimeout(() => setDebouncedReviewSearch(val), 400);
  };

  const handleRemove = async () => {
    if (!removeModal || !removeReason.trim()) {
      toast.error("Reason is required");
      return;
    }
    try {
      if (removeModal.type === "skill") {
        await removeSkill.mutateAsync({ userSkillId: removeModal.id, dto: { reason: removeReason.trim() } });
      } else {
        await removeReview.mutateAsync({ reviewId: removeModal.id, dto: { reason: removeReason.trim() } });
      }
      toast.success(`${removeModal.type === "skill" ? "Skill" : "Review"} removed successfully`);
      setRemoveModal(null);
      setRemoveReason("");
    } catch {
      toast.error("Failed to remove content");
    }
  };

  const handleDismiss = async (reportId: number) => {
    try {
      await dismissReport.mutateAsync(reportId);
      toast.success("Report dismissed");
    } catch {
      toast.error("Failed to dismiss report");
    }
  };

  const tabs: { key: Tab; label: string; count?: number }[] = [
    { key: "reports", label: "Reports Queue", count: flaggedContent.data?.totalCount },
    { key: "skills", label: "Skills", count: adminSkills.data?.totalCount },
    { key: "reviews", label: "Reviews", count: adminReviews.data?.totalCount },
  ];

  return (
    <>
      <motion.div
        className="space-y-6"
        initial={fadeUp.initial}
        animate={fadeUp.animate}
        transition={{ duration: 0.3 }}
      >
        {/* Tabs */}
        <div className="bg-zinc-50 rounded-xl border border-zinc-200 shadow-sm">
          <div className="flex border-b border-zinc-200">
            {tabs.map((t) => (
              <button
                key={t.key}
                onClick={() => setTab(t.key)}
                className={`flex-1 px-4 py-3 text-sm font-medium transition-colors relative ${
                  tab === t.key
                    ? "text-zinc-900 border-b-2 border-[#1D9E75] bg-white"
                    : "text-zinc-500 hover:text-zinc-900 hover:bg-white/70"
                }`}
              >
                {t.label}
                {t.count !== undefined && t.count > 0 && (
                  <span className={`ml-2 px-1.5 py-0.5 text-xs rounded-full ${
                    tab === t.key ? "bg-zinc-100 text-zinc-700" : "bg-zinc-100 text-zinc-600"
                  }`}>
                    {t.count}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>

        {/* ── Reports Tab ── */}
        {tab === "reports" && (
          <div className="bg-white rounded-xl border border-zinc-200 shadow-sm overflow-hidden">
            {flaggedContent.isLoading && (
              <div className="p-5 space-y-3">
                {Array.from({ length: 5 }).map((_, idx) => (
                  <div key={idx} className="h-14 rounded-xl bg-zinc-100 animate-pulse" />
                ))}
              </div>
            )}
            {flaggedContent.data && flaggedContent.data.reports.length === 0 && (
              <div className="text-center py-12">
                <div className="w-12 h-12 rounded-full bg-[#E1F5EE] flex items-center justify-center mx-auto mb-3">
                  <Flag className="w-6 h-6 text-[#1D9E75]" />
                </div>
                <p className="text-sm font-semibold text-zinc-900">No pending reports</p>
              </div>
            )}
            {flaggedContent.data && flaggedContent.data.reports.length > 0 && (
              <>
                <div className="divide-y divide-zinc-100">
                  {flaggedContent.data.reports.map((r) => (
                    <div key={r.reportId} className="p-4 hover:bg-zinc-50 transition-colors">
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <AdminStatusBadge status={r.contentType === "Skill" ? "Skill" : "Review"} />
                            <span className="text-xs text-zinc-500">
                              Reported by <span className="font-medium">{r.reporterName}</span>
                            </span>
                            <span className="text-xs text-zinc-400">
                              {new Date(r.createdAt).toLocaleDateString()}
                            </span>
                          </div>
                          {r.contentPreview && (
                            <p className="text-sm font-medium text-zinc-900 mb-1">
                              {r.contentPreview}
                              {r.rating && (
                                <span className="ml-2 text-xs text-amber-600">
                                  <Star className="w-3 h-3 inline -mt-0.5" /> {r.rating}/5
                                </span>
                              )}
                            </p>
                          )}
                          {r.contentOwnerName && (
                            <p className="text-xs text-zinc-500 mb-1">
                              By: {r.contentOwnerName}
                            </p>
                          )}
                          <p className="text-sm text-zinc-600 bg-zinc-50 rounded-lg px-3 py-2">
                            <span className="font-medium text-zinc-700">Reason:</span> {r.reason}
                          </p>
                        </div>
                        <div className="flex items-center gap-1.5 shrink-0">
                          <button
                            onClick={() =>
                              setRemoveModal({
                                type: r.contentType === "Skill" ? "skill" : "review",
                                id: r.contentId,
                                label: r.contentPreview || `#${r.contentId}`,
                              })
                            }
                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="Remove content"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleDismiss(r.reportId)}
                            disabled={dismissReport.isPending}
                            className="p-2 text-gray-500 hover:bg-gray-100 rounded-lg transition-colors"
                            title="Dismiss report"
                          >
                            <XCircle className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
                <Pagination
                  page={reportPage}
                  totalPages={flaggedContent.data.totalPages}
                  setPage={setReportPage}
                />
              </>
            )}
          </div>
        )}

        {/* ── Skills Tab ── */}
        {tab === "skills" && (
          <div className="space-y-4">
            <SearchBox value={skillSearch} onChange={handleSkillSearch} placeholder="Search by skill name or username..." />
            <div className="bg-white rounded-xl border border-zinc-200 shadow-sm overflow-hidden">
              {adminSkills.isLoading && (
                <div className="p-5 space-y-3">
                  {Array.from({ length: 6 }).map((_, idx) => (
                    <div key={idx} className="h-11 rounded-xl bg-zinc-100 animate-pulse" />
                  ))}
                </div>
              )}
              {adminSkills.data && (
                <>
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-zinc-200 bg-zinc-50">
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Skill</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">User</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Type</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Category</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Added</th>
                          <th className="text-right px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Action</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-zinc-100">
                        {adminSkills.data.skills.map((s: AdminSkillDto, index) => (
                          <tr key={s.userSkillId} className={`${index % 2 === 0 ? "bg-zinc-50/50" : "bg-white"} hover:bg-zinc-50 transition-colors`}>
                            <td className="px-4 py-3">
                              <p className="text-sm font-medium text-zinc-900">{s.skillName}</p>
                              {s.description && (
                                <p className="text-xs text-zinc-500 truncate max-w-50">{s.description}</p>
                              )}
                            </td>
                            <td className="px-4 py-3 text-sm text-zinc-700">{s.userName}</td>
                            <td className="px-4 py-3">
                              <AdminStatusBadge status="Skill" className="capitalize" />
                            </td>
                            <td className="px-4 py-3 text-sm text-zinc-600">{s.category || "—"}</td>
                            <td className="px-4 py-3 text-sm text-zinc-600">{new Date(s.createdAt).toLocaleDateString()}</td>
                            <td className="px-4 py-3 text-right">
                              <button
                                onClick={() =>
                                  setRemoveModal({ type: "skill", id: s.userSkillId, label: s.skillName })
                                }
                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                title="Remove skill"
                              >
                                <Trash2 className="w-4 h-4" />
                              </button>
                            </td>
                          </tr>
                        ))}
                        {adminSkills.data.skills.length === 0 && (
                          <tr>
                            <td colSpan={6} className="py-12">
                              <div className="flex flex-col items-center gap-3 text-center">
                                <div className="w-12 h-12 rounded-full bg-[#E1F5EE] flex items-center justify-center">
                                  <Search className="w-6 h-6 text-[#1D9E75]" />
                                </div>
                                <p className="text-sm font-semibold text-zinc-900">No skills found</p>
                              </div>
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination page={skillPage} totalPages={adminSkills.data.totalPages} setPage={setSkillPage} />
                </>
              )}
            </div>
          </div>
        )}

        {/* ── Reviews Tab ── */}
        {tab === "reviews" && (
          <div className="space-y-4">
            <SearchBox value={reviewSearch} onChange={handleReviewSearch} placeholder="Search by username or comment..." />
            <div className="bg-white rounded-xl border border-zinc-200 shadow-sm overflow-hidden">
              {adminReviews.isLoading && (
                <div className="p-5 space-y-3">
                  {Array.from({ length: 6 }).map((_, idx) => (
                    <div key={idx} className="h-11 rounded-xl bg-zinc-100 animate-pulse" />
                  ))}
                </div>
              )}
              {adminReviews.data && (
                <>
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-zinc-200 bg-zinc-50">
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Reviewer</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Reviewee</th>
                          <th className="text-center px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Rating</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Comment</th>
                          <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Date</th>
                          <th className="text-right px-4 py-3 text-xs font-semibold text-zinc-500 uppercase">Action</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-zinc-100">
                        {adminReviews.data.reviews.map((r: AdminReviewDto, index) => (
                          <tr key={r.reviewId} className={`${index % 2 === 0 ? "bg-zinc-50/50" : "bg-white"} hover:bg-zinc-50 transition-colors`}>
                            <td className="px-4 py-3 text-sm font-medium text-zinc-900">{r.reviewerName}</td>
                            <td className="px-4 py-3 text-sm text-zinc-700">{r.revieweeName}</td>
                            <td className="px-4 py-3 text-center">
                              <div className="flex items-center justify-center gap-1">
                                <Star className="w-3.5 h-3.5 text-amber-500 fill-amber-500" />
                                <span className="text-sm font-medium text-zinc-900">{r.rating}</span>
                              </div>
                            </td>
                            <td className="px-4 py-3 text-sm text-zinc-600 max-w-62.5 truncate">
                              {r.comment || <span className="text-zinc-400 italic">No comment</span>}
                            </td>
                            <td className="px-4 py-3 text-sm text-zinc-600">{new Date(r.createdAt).toLocaleDateString()}</td>
                            <td className="px-4 py-3 text-right">
                              <button
                                onClick={() =>
                                  setRemoveModal({ type: "review", id: r.reviewId, label: `Review by ${r.reviewerName}` })
                                }
                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                title="Remove review"
                              >
                                <Trash2 className="w-4 h-4" />
                              </button>
                            </td>
                          </tr>
                        ))}
                        {adminReviews.data.reviews.length === 0 && (
                          <tr>
                            <td colSpan={6} className="py-12">
                              <div className="flex flex-col items-center gap-3 text-center">
                                <div className="w-12 h-12 rounded-full bg-[#E1F5EE] flex items-center justify-center">
                                  <Star className="w-6 h-6 text-[#1D9E75]" />
                                </div>
                                <p className="text-sm font-semibold text-zinc-900">No reviews found</p>
                              </div>
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination page={reviewPage} totalPages={adminReviews.data.totalPages} setPage={setReviewPage} />
                </>
              )}
            </div>
          </div>
        )}
      </motion.div>

      {/* ── Remove Modal ── */}
      <AnimatePresence>
        {removeModal && (
          <motion.div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            <motion.div
              className="bg-white rounded-xl shadow-2xl max-w-md w-full mx-4 p-6"
              initial={scaleIn.initial}
              animate={scaleIn.animate}
              exit={scaleIn.initial}
              transition={scaleIn.transition}
            >
            <div className="flex items-start justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-red-100 rounded-lg">
                  <AlertTriangle className="w-5 h-5 text-red-600" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-zinc-900">
                    Remove {removeModal.type === "skill" ? "Skill" : "Review"}
                  </h3>
                  <p className="text-sm text-zinc-500">{removeModal.label}</p>
                </div>
              </div>
              <button
                onClick={() => { setRemoveModal(null); setRemoveReason(""); }}
                className="text-zinc-400 hover:text-zinc-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
              <p className="text-sm text-red-700">
                <strong>Warning:</strong> This action permanently deletes this {removeModal.type} from the platform.
              </p>
            </div>

            <div className="mb-4">
              <label className="block text-sm font-medium text-zinc-700 mb-1.5">Reason *</label>
              <textarea
                value={removeReason}
                onChange={(e) => setRemoveReason(e.target.value)}
                rows={3}
                placeholder="Why is this content being removed?"
                className="w-full border border-zinc-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-[#1D9E75] focus:border-transparent resize-none"
              />
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => { setRemoveModal(null); setRemoveReason(""); }}
                className="px-4 py-2 text-sm font-medium text-zinc-900 border border-zinc-200 rounded-lg hover:bg-zinc-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleRemove}
                disabled={removeSkill.isPending || removeReview.isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50"
              >
                {(removeSkill.isPending || removeReview.isPending) ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  "Confirm Remove"
                )}
              </button>
            </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}

// ── Shared Components ──

function SearchBox({ value, onChange, placeholder }: { value: string; onChange: (v: string) => void; placeholder: string }) {
  return (
    <div className="bg-white rounded-xl border border-zinc-200 p-4 shadow-sm">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400" />
        <input
          type="text"
          placeholder={placeholder}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          className="w-full pl-10 pr-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#1D9E75] focus:border-transparent"
        />
      </div>
    </div>
  );
}

function Pagination({ page, totalPages, setPage }: { page: number; totalPages: number; setPage: (p: number) => void }) {
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-between px-6 py-4 border-t border-zinc-200">
      <button
        onClick={() => setPage(Math.max(1, page - 1))}
        disabled={page === 1}
        className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-zinc-900 border border-zinc-200 rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        <ChevronLeft className="w-4 h-4" /> Previous
      </button>
      <span className="text-sm text-zinc-600">Page {page} of {totalPages}</span>
      <button
        onClick={() => setPage(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
        className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-zinc-900 border border-zinc-200 rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        Next <ChevronRight className="w-4 h-4" />
      </button>
    </div>
  );
}
