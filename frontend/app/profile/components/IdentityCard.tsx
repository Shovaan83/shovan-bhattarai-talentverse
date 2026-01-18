"use client";

import { motion } from "framer-motion";
import { User, Edit3 } from "lucide-react";

interface IdentityCardProps {
  name: string;
  email: string;
  bio: string;
  avatarUrl?: string;
  onEdit?: () => void;
}

export function IdentityCard({
  name,
  email,
  bio,
  avatarUrl,
  onEdit,
}: IdentityCardProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay: 0.1 }}
      className="bg-white rounded-3xl p-8 shadow-xl shadow-gray-200/50 border border-gray-100 md:row-span-2 md:sticky md:top-8"
    >
      {/* Avatar */}
      <div className="relative mx-auto w-32 h-32 mb-6">
        <div className="absolute inset-0 bg-gradient-to-br from-emerald-400 to-orange-400 rounded-full blur-lg opacity-30"></div>
        <div className="relative w-full h-full rounded-full bg-gradient-to-br from-emerald-500 to-orange-500 p-1">
          <div className="w-full h-full rounded-full bg-white flex items-center justify-center overflow-hidden">
            {avatarUrl ? (
              <img
                src={avatarUrl}
                alt={name}
                className="w-full h-full object-cover"
              />
            ) : (
              <User className="w-16 h-16 text-gray-300" />
            )}
          </div>
        </div>
      </div>

      {/* Name & Email */}
      <div className="text-center mb-6">
        <h2 className="font-heading text-2xl font-bold text-gray-900 mb-1">
          {name}
        </h2>
        <p className="text-gray-500 text-sm">{email}</p>
      </div>

      {/* Bio */}
      <div className="mb-8">
        <h3 className="font-heading text-sm font-semibold text-gray-400 uppercase tracking-wider mb-3">
          About Me
        </h3>
        <p className="text-gray-600 leading-relaxed text-sm">
          {bio || "No bio added yet. Tell the community about yourself!"}
        </p>
      </div>

      {/* Edit Button */}
      <button
        onClick={onEdit}
        className="w-full py-3 px-4 bg-gray-900 hover:bg-gray-800 text-white rounded-xl font-medium flex items-center justify-center gap-2 transition-all duration-200 hover:shadow-lg hover:shadow-gray-900/20"
      >
        <Edit3 className="w-4 h-4" />
        Edit Profile
      </button>
    </motion.div>
  );
}
