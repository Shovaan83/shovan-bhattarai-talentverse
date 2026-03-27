'use client';

import { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { User, Settings, LogOut } from 'lucide-react';
import Link from 'next/link';
import { useAuth } from '@/lib/hooks/useAuth';
import { Avatar } from '@/app/components/ui/Avatar';

interface ProfileDropdownProps {
  username: string;
  profilePictureUrl?: string | null;
}

export function ProfileDropdown({ username, profilePictureUrl }: ProfileDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { logout } = useAuth();

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const handleLogout = async () => {
    setIsOpen(false);
    await logout();
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* Trigger Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 px-2 py-1.5 rounded-lg hover:bg-white/10 transition-colors"
      >
        <Avatar
          src={profilePictureUrl}
          name={username}
          size={32}
          className="border-2 border-zinc-200/30"
        />
        <span className="hidden md:block text-sm font-medium text-white/70">
          {username}
        </span>
      </button>

      {/* Dropdown Menu */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.15 }}
            className="absolute right-0 mt-2 w-56 bg-white rounded-xl shadow-xl border border-zinc-200 py-2 z-50"
          >
            {/* User Info */}
            <div className="px-4 py-3 border-b border-zinc-200">
              <p className="text-sm font-display font-semibold text-zinc-900">{username}</p>
              <p className="text-xs text-gray-500 mt-0.5">View and edit profile</p>
            </div>

            {/* Menu Items */}
            <div className="py-1">
              <Link href="/profile">
                <button
                  onClick={() => setIsOpen(false)}
                  className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-zinc-900 hover:bg-zinc-50 transition-colors"
                >
                  <User className="w-4 h-4 text-zinc-600" />
                  View Profile
                </button>
              </Link>

              <Link href="/settings">
                <button
                  onClick={() => setIsOpen(false)}
                  className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-zinc-900 hover:bg-zinc-50 transition-colors"
                >
                  <Settings className="w-4 h-4 text-zinc-600" />
                  Settings
                </button>
              </Link>
            </div>

            {/* Logout */}
            <div className="border-t border-zinc-200 pt-1">
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 transition-colors"
              >
                <LogOut className="w-4 h-4" />
                Log Out
              </button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
