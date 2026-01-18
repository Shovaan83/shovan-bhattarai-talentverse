"use client";

import { useEffect, useMemo, useState } from "react";
import { X } from "lucide-react";
import type { CurrentUser, UpdateProfilePayload } from "@/lib/types/account";

interface EditProfileModalProps {
  isOpen: boolean;
  onClose: () => void;
  user: CurrentUser;
  onSubmit: (payload: UpdateProfilePayload) => void;
  isSubmitting?: boolean;
  errorMessage?: string | null;
}

export default function EditProfileModal({
  isOpen,
  onClose,
  user,
  onSubmit,
  isSubmitting = false,
  errorMessage,
}: EditProfileModalProps) {
  const initial = useMemo(
    () => ({
      username: user.username ?? "",
      bio: user.bio ?? "",
      profilePictureUrl: user.profilePictureUrl ?? "",
    }),
    [user]
  );

  const [username, setUsername] = useState(initial.username);
  const [bio, setBio] = useState(initial.bio);
  const [profilePictureUrl, setProfilePictureUrl] = useState(
    initial.profilePictureUrl
  );

  useEffect(() => {
    if (!isOpen) return;
    setUsername(initial.username);
    setBio(initial.bio);
    setProfilePictureUrl(initial.profilePictureUrl);
  }, [isOpen, initial]);

  if (!isOpen) return null;

  const handleSubmit = () => {
    const payload: UpdateProfilePayload = {
      username: username.trim() || undefined,
      bio,
      // Send empty string to allow backend to clear the picture URL
      profilePictureUrl: profilePictureUrl.trim(),
    };
    onSubmit(payload);
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
    >
      <button
        type="button"
        className="absolute inset-0 bg-black/60"
        onClick={onClose}
        aria-label="Close"
      />

      <div className="relative w-full max-w-xl rounded-2xl bg-white shadow-2xl border border-gray-100 overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <div>
            <h2 className="text-lg font-heading font-bold text-gray-900">
              Edit Profile
            </h2>
            <p className="text-sm text-gray-500">Update your public details</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-2 rounded-full hover:bg-gray-100 text-gray-600 transition-colors"
            aria-label="Close"
          >
            <X size={18} />
          </button>
        </div>

        <div className="px-6 py-5 space-y-4">
          {errorMessage ? (
            <div className="p-3 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm">
              {errorMessage}
            </div>
          ) : null}

          <div className="space-y-1">
            <label className="text-sm font-semibold text-gray-700">Username</label>
            <input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full rounded-xl border border-gray-200 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-emerald-300"
              placeholder="Your username"
              autoComplete="username"
            />
          </div>

          <div className="space-y-1">
            <label className="text-sm font-semibold text-gray-700">Bio</label>
            <textarea
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              className="w-full min-h-[110px] rounded-xl border border-gray-200 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-emerald-300"
              placeholder="Tell people about yourself"
            />
            <p className="text-xs text-gray-400">Max ~500 characters recommended.</p>
          </div>

          <div className="space-y-1">
            <label className="text-sm font-semibold text-gray-700">
              Profile picture URL
            </label>
            <input
              value={profilePictureUrl}
              onChange={(e) => setProfilePictureUrl(e.target.value)}
              className="w-full rounded-xl border border-gray-200 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-emerald-300"
              placeholder="https://..."
              autoComplete="url"
            />
          </div>
        </div>

        <div className="px-6 py-4 border-t border-gray-100 flex items-center justify-end gap-3 bg-gray-50">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-xl bg-white border border-gray-200 text-gray-700 hover:bg-gray-100 transition-colors"
            disabled={isSubmitting}
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            className="px-5 py-2 rounded-xl bg-orange-600 hover:bg-orange-700 text-white font-semibold transition-colors disabled:opacity-60"
            disabled={isSubmitting}
          >
            {isSubmitting ? "Saving..." : "Save changes"}
          </button>
        </div>
      </div>
    </div>
  );
}
