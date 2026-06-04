'use client';

import { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Bell, BellRing, ArrowRightLeft, CheckCircle2, Clock, XCircle } from 'lucide-react';
import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import { proposalsApi } from '@/lib/api/proposals';

interface NotificationDropdownProps {
  count: number;
}

export function NotificationDropdown({ count }: NotificationDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const { data: proposalsData } = useQuery({
    queryKey: ['proposals', 'notifications', 'recent'],
    queryFn: () => proposalsApi.getProposals({
      direction: 'received',
      status: 'Pending',
      page: 1,
      pageSize: 5,
    }),
    enabled: isOpen,
  });

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

  const formatTimeAgo = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (diffInSeconds < 60) return 'Just now';
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
    return `${Math.floor(diffInSeconds / 86400)}d ago`;
  };

  const getNotificationIcon = (status: string) => {
    switch (status) {
      case 'Pending':
        return <BellRing className="w-5 h-5 text-[#1D9E75]" />;
      case 'Accepted':
      case 'Completed':
        return <CheckCircle2 className="w-5 h-5 text-emerald-600" />;
      case 'Declined':
      case 'Cancelled':
        return <XCircle className="w-5 h-5 text-rose-500" />;
      default:
        return <ArrowRightLeft className="w-5 h-5 text-[#1D9E75]" />;
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* Bell Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        aria-label="Open notifications"
        className="relative p-2 rounded-lg text-zinc-600 hover:text-zinc-900 hover:bg-zinc-100 transition-colors"
      >
        <Bell className="w-5 h-5" />
        {count > 0 && (
          <span className="absolute top-0 right-0 w-5 h-5 bg-[#1D9E75] text-white text-xs font-bold rounded-full flex items-center justify-center">
            {count > 9 ? '9+' : count}
          </span>
        )}
      </button>

      {/* Dropdown */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.15 }}
            className="absolute right-0 mt-2 w-80 bg-white rounded-xl shadow-xl border border-zinc-200 z-50"
          >
            {/* Header */}
            <div className="px-4 py-3 border-b border-zinc-200">
              <div className="flex items-center gap-2">
                <Bell className="w-4 h-4 text-[#1D9E75]" />
                <h3 className="text-sm font-display font-semibold text-zinc-900">Notifications</h3>
              </div>
              <p className="text-xs text-gray-500 mt-0.5">
                {count === 0 ? 'No new notifications' : `${count} pending proposal${count === 1 ? '' : 's'}`}
              </p>
            </div>

            {/* Notification List */}
            <div className="max-h-96 overflow-y-auto">
              {proposalsData?.proposals && proposalsData.proposals.length > 0 ? (
                proposalsData.proposals.map((proposal) => (
                  <Link
                    key={proposal.proposalId}
                    href={`/proposals/${proposal.proposalId}`}
                    onClick={() => setIsOpen(false)}
                  >
                    <div className="px-4 py-3 hover:bg-zinc-50 transition-colors border-b border-zinc-100 cursor-pointer">
                      <div className="flex items-start gap-3">
                        {/* Icon */}
                        <div className="w-10 h-10 rounded-full bg-[#E1F5EE] flex items-center justify-center shrink-0 ring-1 ring-[#1D9E75]/10">
                          {getNotificationIcon(proposal.status)}
                        </div>

                        {/* Content */}
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-zinc-900 truncate">
                            New proposal from {proposal.otherUsername}
                          </p>
                          <p className="text-xs text-gray-500 mt-1 line-clamp-2">
                            Wants to swap {proposal.receivingSkillName} for {proposal.offeringSkillName}
                          </p>
                          <div className="flex items-center gap-2 mt-2 text-xs text-gray-400">
                            <Clock className="w-3 h-3" />
                            {formatTimeAgo(proposal.createdAt)}
                          </div>
                        </div>
                      </div>
                    </div>
                  </Link>
                ))
              ) : (
                <div className="px-4 py-8 text-center">
                  <div className="w-12 h-12 rounded-full bg-[#E1F5EE] flex items-center justify-center mx-auto mb-3">
                      <BellRing className="w-6 h-6 text-[#1D9E75]" />
                  </div>
                  <p className="text-sm font-medium text-zinc-900">No new notifications</p>
                  <p className="text-xs text-gray-500 mt-1">
                    You&apos;re all caught up!
                  </p>
                </div>
              )}
            </div>

            {/* Footer */}
            {count > 0 && (
              <div className="px-4 py-3 border-t border-zinc-200">
                <Link href="/proposals">
                  <button
                    onClick={() => setIsOpen(false)}
                    className="w-full text-center text-sm font-medium text-[#1D9E75] hover:text-[#0F6E56] transition-colors"
                  >
                    View all proposals
                  </button>
                </Link>
              </div>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
