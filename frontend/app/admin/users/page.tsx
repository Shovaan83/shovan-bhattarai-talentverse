"use client";

import { useState } from "react";
import {
  Search,
  User,
  ShieldBan,
  ShieldCheck,
  ShieldOff,
  Loader2,
  ChevronLeft,
  ChevronRight,
  X,
  AlertTriangle,
} from "lucide-react";
import { useAdminUsers, useUpdateUserStatus } from "@/lib/hooks/useAdmin";
import type { AdminUserDto } from "@/lib/types/admin";
import { toast } from "react-hot-toast";
import { AdminStatusBadge } from "@/app/components/ui/AdminStatusBadge";
import { motion, AnimatePresence } from "framer-motion";
import { fadeUp, scaleIn } from "@/app/components/motion/variants";

export default function AdminUsersPage() {
  const [searchQuery, setSearchQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [page, setPage] = useState(1);
  const [actionModal, setActionModal] = useState<{
    user: AdminUserDto;
    action: "Suspend" | "Unsuspend" | "Ban";
  } | null>(null);
  const [reason, setReason] = useState("");

  const { data, isLoading, error } = useAdminUsers(debouncedQuery || undefined, page, 20);
  const updateStatus = useUpdateUserStatus();

  // Debounce search
  const handleSearch = (value: string) => {
    setSearchQuery(value);
    setPage(1);
    const timeoutId = setTimeout(() => setDebouncedQuery(value), 400);
    return () => clearTimeout(timeoutId);
  };

  const handleAction = async () => {
    if (!actionModal) return;
    if (actionModal.action === "Ban" && !reason.trim()) {
      toast.error("Ban reason is required");
      return;
    }

    try {
      await updateStatus.mutateAsync({
        userId: actionModal.user.id,
        dto: {
          action: actionModal.action,
          reason: reason.trim() || undefined,
        },
      });
      toast.success(
        `User ${actionModal.action === "Unsuspend" ? "unsuspended" : actionModal.action.toLowerCase() + "ned"} successfully`
      );
      setActionModal(null);
      setReason("");
    } catch {
      toast.error("Failed to update user status");
    }
  };

  const getStatusBadge = (user: AdminUserDto) => {
    if (user.isBanned)
      return <AdminStatusBadge status="Banned" />;
    if (user.isSuspended)
      return <AdminStatusBadge status="Suspended" />;
    return <AdminStatusBadge status="Active" />;
  };

  return (
    <>
      <motion.div
        className="space-y-6"
        initial={fadeUp.initial}
        animate={fadeUp.animate}
        transition={{ duration: 0.3 }}
      >
        {/* Search */}
        <div className="bg-white rounded-xl border border-zinc-200 p-4 shadow-sm">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-zinc-400" />
            <input
              type="text"
              placeholder="Search by username or email..."
              value={searchQuery}
              onChange={(e) => handleSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 border border-zinc-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-zinc-400 focus:border-transparent"
            />
          </div>
        </div>

        {/* Loading */}
        {isLoading && (
          <div className="bg-white rounded-xl border border-zinc-200 p-5 shadow-sm">
            <div className="space-y-3">
              {Array.from({ length: 6 }).map((_, idx) => (
                <div key={idx} className="h-12 rounded-xl bg-zinc-100 animate-pulse" />
              ))}
            </div>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="text-center py-12">
            <p className="text-red-600">Failed to load users</p>
          </div>
        )}

        {/* User Table */}
        {data && (
          <div className="bg-white rounded-xl border border-zinc-200 shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-zinc-200">
              <p className="text-sm text-zinc-600">
                {data.totalCount} user{data.totalCount !== 1 ? "s" : ""} found
              </p>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-zinc-200 bg-zinc-50">
                    <th className="text-left px-6 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      User
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Joined
                    </th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="text-center px-4 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Skills
                    </th>
                    <th className="text-center px-4 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Swaps
                    </th>
                    <th className="text-center px-4 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Credits
                    </th>
                    <th className="text-right px-6 py-3 text-xs font-semibold text-zinc-600 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-100">
                  {data.users.map((user, index) => (
                    <tr
                      key={user.id}
                      className="hover:bg-zinc-50 transition-colors"
                    >
                      {/* User info */}
                      <td className="px-6 py-3">
                        <div className="flex items-center gap-3">
                          {user.profilePictureUrl ? (
                            <img
                              src={user.profilePictureUrl}
                              alt={user.userName}
                              className="w-9 h-9 rounded-full object-cover"
                            />
                          ) : (
                            <div className="w-9 h-9 rounded-full bg-zinc-100 flex items-center justify-center">
                              <User className="w-4 h-4 text-zinc-500" />
                            </div>
                          )}
                          <div>
                            <div className="flex items-center gap-1.5">
                              <p className="text-sm font-semibold text-zinc-900">
                                {user.userName}
                              </p>
                              {user.isVerified && (
                                <ShieldCheck className="w-3.5 h-3.5 text-blue-500" />
                              )}
                            </div>
                            <p className="text-xs text-zinc-500">{user.email}</p>
                          </div>
                        </div>
                      </td>
                      {/* Joined */}
                      <td className="px-4 py-3 text-sm text-zinc-600">
                        {new Date(user.createdAt).toLocaleDateString()}
                      </td>
                      {/* Status */}
                      <td className="px-4 py-3">{getStatusBadge(user)}</td>
                      {/* Skills */}
                      <td className="px-4 py-3 text-center text-sm text-zinc-700 font-medium">
                        {user.skillCount}
                      </td>
                      {/* Swaps */}
                      <td className="px-4 py-3 text-center text-sm text-zinc-700 font-medium">
                        {user.completedSwaps}
                      </td>
                      {/* Credits */}
                      <td className="px-4 py-3 text-center text-sm text-zinc-700 font-medium">
                        {Math.floor(user.creditBalance)}
                      </td>
                      {/* Actions */}
                      <td className="px-6 py-3 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          {!user.isBanned && !user.isSuspended && (
                            <button
                              onClick={() =>
                                setActionModal({
                                  user,
                                  action: "Suspend",
                                })
                              }
                              className="p-1.5 text-amber-600 hover:bg-amber-50 rounded-lg transition-colors"
                              title="Suspend user"
                            >
                              <ShieldOff className="w-4 h-4" />
                            </button>
                          )}
                          {user.isSuspended && !user.isBanned && (
                            <button
                              onClick={() =>
                                setActionModal({
                                  user,
                                  action: "Unsuspend",
                                })
                              }
                              className="p-1.5 text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors"
                              title="Unsuspend user"
                            >
                              <ShieldCheck className="w-4 h-4" />
                            </button>
                          )}
                          {!user.isBanned && (
                            <button
                              onClick={() =>
                                setActionModal({
                                  user,
                                  action: "Ban",
                                })
                              }
                              className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                              title="Ban user"
                            >
                              <ShieldBan className="w-4 h-4" />
                            </button>
                          )}
                          {user.isBanned && (
                            <span className="text-xs text-gray-400 italic">
                              Banned
                            </span>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                  {data.users.length === 0 && (
                    <tr>
                      <td colSpan={7} className="py-12">
                        <div className="flex flex-col items-center gap-3 text-center">
                          <div className="w-12 h-12 rounded-full bg-emerald-50 flex items-center justify-center">
                            <User className="w-6 h-6 text-[#1D9E75]" />
                          </div>
                          <p className="text-sm font-semibold text-zinc-900">No users found</p>
                          <button
                            onClick={() => {
                              setSearchQuery("");
                              setDebouncedQuery("");
                              setPage(1);
                            }}
                            className="text-xs font-medium text-[#1D9E75] hover:text-[#15785A]"
                          >
                            Clear search filters
                          </button>
                        </div>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="flex items-center justify-between px-6 py-4 border-t border-zinc-200">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-zinc-700 border border-zinc-200 rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  <ChevronLeft className="w-4 h-4" />
                  Previous
                </button>
                <span className="text-sm text-zinc-600">
                  Page {page} of {data.totalPages}
                </span>
                <button
                  onClick={() =>
                    setPage((p) => Math.min(data.totalPages, p + 1))
                  }
                  disabled={page === data.totalPages}
                  className="flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-zinc-700 border border-zinc-200 rounded-lg hover:bg-zinc-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Next
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>
            )}
          </div>
        )}
      </motion.div>

      {/* Action Modal */}
      <AnimatePresence>
        {actionModal && (
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
                <div
                  className={`p-2 rounded-lg ${
                    actionModal.action === "Ban"
                      ? "bg-red-100"
                      : actionModal.action === "Suspend"
                      ? "bg-amber-100"
                      : "bg-emerald-50"
                  }`}
                >
                  {actionModal.action === "Ban" ? (
                    <AlertTriangle className="w-5 h-5 text-red-600" />
                  ) : actionModal.action === "Suspend" ? (
                    <ShieldOff className="w-5 h-5 text-amber-600" />
                  ) : (
                    <ShieldCheck className="w-5 h-5 text-emerald-600" />
                  )}
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-zinc-900">
                    {actionModal.action} User
                  </h3>
                  <p className="text-sm text-zinc-500">
                    {actionModal.user.userName}
                  </p>
                </div>
              </div>
              <button
                onClick={() => {
                  setActionModal(null);
                  setReason("");
                }}
                className="text-zinc-400 hover:text-zinc-600"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {actionModal.action === "Ban" && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
                <p className="text-sm text-red-700">
                  <strong>Warning:</strong> Banning a user is permanent. They
                  will be soft-deleted and locked out of the platform.
                </p>
              </div>
            )}

            {(actionModal.action === "Suspend" ||
              actionModal.action === "Ban") && (
              <div className="mb-4">
                <label className="block text-sm font-medium text-zinc-700 mb-1.5">
                  Reason{actionModal.action === "Ban" ? " *" : " (optional)"}
                </label>
                <textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  rows={3}
                  placeholder={`Why is this user being ${actionModal.action.toLowerCase()}ned?`}
                  className="w-full border border-zinc-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-zinc-400 focus:border-transparent resize-none"
                />
              </div>
            )}

            <div className="flex justify-end gap-3">
              <button
                onClick={() => {
                  setActionModal(null);
                  setReason("");
                }}
                className="px-4 py-2 text-sm font-medium text-zinc-900 border border-zinc-200 rounded-lg hover:bg-zinc-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleAction}
                disabled={updateStatus.isPending}
                className={`px-4 py-2 text-sm font-medium text-white rounded-lg transition-colors disabled:opacity-50 ${
                  actionModal.action === "Ban"
                    ? "bg-red-600 hover:bg-red-700"
                    : actionModal.action === "Suspend"
                    ? "bg-amber-600 hover:bg-amber-700"
                    : "bg-[#1D9E75] hover:bg-[#15785A]"
                }`}
              >
                {updateStatus.isPending ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  `Confirm ${actionModal.action}`
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
