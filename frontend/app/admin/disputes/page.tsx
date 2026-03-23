"use client";

import { useState } from "react";
import {
  Search,
  Loader2,
  ChevronLeft,
  ChevronRight,
  CheckCircle,
  XCircle,
  AlertTriangle,
  X,
  ArrowRightLeft,
} from "lucide-react";
import {
  useAdminProposals,
  useResolveDispute,
} from "@/lib/hooks/useAdmin";
import type { AdminProposalDto, ResolveDisputeDto } from "@/lib/types/admin";
import { toast } from "react-hot-toast";

const STATUS_TABS = [
  { label: "All", value: undefined },
  { label: "Pending", value: 0 },
  { label: "Accepted", value: 1 },
  { label: "Rejected", value: 2 },
  { label: "Completed", value: 3 },
  { label: "Cancelled", value: 4 },
] as const;

const STATUS_BADGE: Record<string, string> = {
  Pending: "bg-amber-100 text-amber-700",
  Accepted: "bg-sky-100 text-sky-700",
  Rejected: "bg-red-100 text-red-700",
  Completed: "bg-emerald-100 text-emerald-700",
  Cancelled: "bg-gray-100 text-gray-600",
};

export default function AdminDisputesPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);
  const [page, setPage] = useState(1);

  // Resolve modal
  const [resolveModal, setResolveModal] = useState<AdminProposalDto | null>(null);
  const [resolveAction, setResolveAction] = useState<"ForceComplete" | "ForceCancel">("ForceCancel");
  const [adminNote, setAdminNote] = useState("");

  const proposals = useAdminProposals(debouncedSearch || undefined, statusFilter, page, 20);
  const resolveDispute = useResolveDispute();

  const handleSearch = (val: string) => {
    setSearch(val);
    setPage(1);
    setTimeout(() => setDebouncedSearch(val), 400);
  };

  const handleResolve = async () => {
    if (!resolveModal || !adminNote.trim() || adminNote.trim().length < 5) {
      toast.error("Admin note must be at least 5 characters");
      return;
    }
    try {
      const dto: ResolveDisputeDto = { action: resolveAction, adminNote: adminNote.trim() };
      await resolveDispute.mutateAsync({ proposalId: resolveModal.proposalId, dto });
      toast.success(resolveAction === "ForceComplete" ? "Proposal force-completed" : "Proposal force-cancelled");
      setResolveModal(null);
      setAdminNote("");
      setResolveAction("ForceCancel");
    } catch {
      toast.error("Failed to resolve dispute");
    }
  };

  const canResolve = (p: AdminProposalDto) => p.status !== "Completed" && p.status !== "Cancelled";

  return (
    <>
      <div className="space-y-6">
        {/* Search */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search by proposer or recipient username..."
              value={search}
              onChange={(e) => handleSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
            />
          </div>
        </div>

        {/* Status Tabs */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div className="flex border-b border-gray-200 overflow-x-auto">
            {STATUS_TABS.map((t) => (
              <button
                key={t.label}
                onClick={() => { setStatusFilter(t.value); setPage(1); }}
                className={`px-4 py-3 text-sm font-medium whitespace-nowrap transition-colors relative ${
                  statusFilter === t.value
                    ? "text-indigo-700 border-b-2 border-indigo-600"
                    : "text-gray-600 hover:text-gray-900 hover:bg-gray-50"
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>
        </div>

        {/* Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          {proposals.isLoading && (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="w-8 h-8 animate-spin text-indigo-500" />
            </div>
          )}
          {proposals.data && (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200 bg-gray-50/80">
                      <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Proposer</th>
                      <th className="text-center px-2 py-3 text-xs font-semibold text-gray-500 uppercase"></th>
                      <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Recipient</th>
                      <th className="text-center px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Status</th>
                      <th className="text-center px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Confirmed</th>
                      <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Created</th>
                      <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {proposals.data.proposals.map((p: AdminProposalDto) => (
                      <tr key={p.proposalId} className="hover:bg-gray-50/50 transition-colors">
                        <td className="px-4 py-3">
                          <p className="text-sm font-medium text-gray-900">{p.proposerName}</p>
                          <p className="text-xs text-emerald-600">{p.proposerSkill || "—"}</p>
                        </td>
                        <td className="px-2 py-3 text-center">
                          <ArrowRightLeft className="w-4 h-4 text-gray-400 mx-auto" />
                        </td>
                        <td className="px-4 py-3">
                          <p className="text-sm font-medium text-gray-900">{p.recipientName}</p>
                          <p className="text-xs text-violet-600">{p.recipientSkill || "—"}</p>
                        </td>
                        <td className="px-4 py-3 text-center">
                          <span className={`px-2 py-0.5 text-xs font-medium rounded-full ${STATUS_BADGE[p.status] || "bg-gray-100 text-gray-600"}`}>
                            {p.status}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-center gap-3 text-xs">
                            <span className="flex items-center gap-1">
                              {p.proposerConfirmed ? (
                                <CheckCircle className="w-3.5 h-3.5 text-emerald-500" />
                              ) : (
                                <XCircle className="w-3.5 h-3.5 text-gray-300" />
                              )}
                              P
                            </span>
                            <span className="flex items-center gap-1">
                              {p.recipientConfirmed ? (
                                <CheckCircle className="w-3.5 h-3.5 text-emerald-500" />
                              ) : (
                                <XCircle className="w-3.5 h-3.5 text-gray-300" />
                              )}
                              R
                            </span>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-600">
                          {new Date(p.createdAt).toLocaleDateString()}
                        </td>
                        <td className="px-4 py-3 text-right">
                          {canResolve(p) ? (
                            <button
                              onClick={() => {
                                setResolveModal(p);
                                setResolveAction(p.status === "Accepted" ? "ForceComplete" : "ForceCancel");
                              }}
                              className="px-3 py-1.5 text-xs font-medium text-indigo-700 bg-indigo-50 hover:bg-indigo-100 rounded-lg transition-colors"
                            >
                              Resolve
                            </button>
                          ) : (
                            <span className="text-xs text-gray-400">—</span>
                          )}
                        </td>
                      </tr>
                    ))}
                    {proposals.data.proposals.length === 0 && (
                      <tr>
                        <td colSpan={7} className="text-center py-12 text-gray-400">No proposals found</td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
              {proposals.data.totalPages > 1 && (
                <div className="flex items-center justify-between px-6 py-4 border-t border-gray-200">
                  <button
                    onClick={() => setPage(Math.max(1, page - 1))}
                    disabled={page === 1}
                    className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    <ChevronLeft className="w-4 h-4" /> Previous
                  </button>
                  <span className="text-sm text-gray-600">Page {page} of {proposals.data.totalPages}</span>
                  <button
                    onClick={() => setPage(Math.min(proposals.data!.totalPages, page + 1))}
                    disabled={page === proposals.data.totalPages}
                    className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    Next <ChevronRight className="w-4 h-4" />
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      {/* ── Resolve Modal ── */}
      {resolveModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="bg-white rounded-xl shadow-2xl max-w-lg w-full mx-4 p-6">
            <div className="flex items-start justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-indigo-100 rounded-lg">
                  <AlertTriangle className="w-5 h-5 text-indigo-600" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Resolve Dispute</h3>
                  <p className="text-sm text-gray-500">
                    Proposal #{resolveModal.proposalId} — {resolveModal.proposerName} ↔ {resolveModal.recipientName}
                  </p>
                </div>
              </div>
              <button
                onClick={() => { setResolveModal(null); setAdminNote(""); }}
                className="text-gray-400 hover:text-gray-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Proposal summary */}
            <div className="bg-gray-50 rounded-lg p-3 mb-4 text-sm">
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <span className="text-gray-500">Proposer:</span>{" "}
                  <span className="font-medium">{resolveModal.proposerName}</span>
                  <span className="text-emerald-600 text-xs ml-1">({resolveModal.proposerSkill})</span>
                </div>
                <div>
                  <span className="text-gray-500">Recipient:</span>{" "}
                  <span className="font-medium">{resolveModal.recipientName}</span>
                  <span className="text-violet-600 text-xs ml-1">({resolveModal.recipientSkill})</span>
                </div>
                <div>
                  <span className="text-gray-500">Status:</span>{" "}
                  <span className={`px-1.5 py-0.5 text-xs font-medium rounded-full ${STATUS_BADGE[resolveModal.status]}`}>
                    {resolveModal.status}
                  </span>
                </div>
                <div>
                  <span className="text-gray-500">Confirmed:</span>{" "}
                  <span className="text-xs">
                    P: {resolveModal.proposerConfirmed ? "✓" : "✗"} | R: {resolveModal.recipientConfirmed ? "✓" : "✗"}
                  </span>
                </div>
              </div>
            </div>

            {/* Action selection */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">Action</label>
              <div className="flex gap-3">
                {resolveModal.status === "Accepted" && (
                  <label className={`flex-1 flex items-center gap-2 p-3 border rounded-lg cursor-pointer transition-all ${
                    resolveAction === "ForceComplete"
                      ? "border-emerald-500 bg-emerald-50 ring-2 ring-emerald-200"
                      : "border-gray-300 hover:bg-gray-50"
                  }`}>
                    <input
                      type="radio"
                      name="action"
                      checked={resolveAction === "ForceComplete"}
                      onChange={() => setResolveAction("ForceComplete")}
                      className="sr-only"
                    />
                    <CheckCircle className={`w-5 h-5 ${resolveAction === "ForceComplete" ? "text-emerald-600" : "text-gray-400"}`} />
                    <div>
                      <p className="text-sm font-medium text-gray-900">Force Complete</p>
                      <p className="text-xs text-gray-500">Award credits + badges to both</p>
                    </div>
                  </label>
                )}
                <label className={`flex-1 flex items-center gap-2 p-3 border rounded-lg cursor-pointer transition-all ${
                  resolveAction === "ForceCancel"
                    ? "border-red-500 bg-red-50 ring-2 ring-red-200"
                    : "border-gray-300 hover:bg-gray-50"
                }`}>
                  <input
                    type="radio"
                    name="action"
                    checked={resolveAction === "ForceCancel"}
                    onChange={() => setResolveAction("ForceCancel")}
                    className="sr-only"
                  />
                  <XCircle className={`w-5 h-5 ${resolveAction === "ForceCancel" ? "text-red-600" : "text-gray-400"}`} />
                  <div>
                    <p className="text-sm font-medium text-gray-900">Force Cancel</p>
                    <p className="text-xs text-gray-500">Cancel without credits</p>
                  </div>
                </label>
              </div>
            </div>

            {/* Admin note */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1.5">Admin Note *</label>
              <textarea
                value={adminNote}
                onChange={(e) => setAdminNote(e.target.value)}
                rows={3}
                placeholder="Explain reason for this decision..."
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none"
              />
            </div>

            <div className="flex justify-end gap-3">
              <button
                onClick={() => { setResolveModal(null); setAdminNote(""); }}
                className="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleResolve}
                disabled={resolveDispute.isPending}
                className={`px-4 py-2 text-sm font-medium text-white rounded-lg transition-colors disabled:opacity-50 ${
                  resolveAction === "ForceComplete"
                    ? "bg-emerald-600 hover:bg-emerald-700"
                    : "bg-red-600 hover:bg-red-700"
                }`}
              >
                {resolveDispute.isPending ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : resolveAction === "ForceComplete" ? (
                  "Force Complete"
                ) : (
                  "Force Cancel"
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
