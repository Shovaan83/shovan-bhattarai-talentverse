'use client';

import { MessageSquare } from 'lucide-react';
import { Conversation } from '@/lib/types/messages';

interface ConversationListProps {
  conversations: Conversation[];
  selectedId: number | null;
  onSelect: (proposalId: number) => void;
}

function timeAgo(dateStr: string | null | undefined): string {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  const diffHours = Math.floor(diffMins / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString();
}

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

const statusColors: Record<string, string> = {
  Accepted: 'bg-emerald-100 text-emerald-700',
  Completed: 'bg-blue-100 text-blue-700',
  Pending: 'bg-yellow-100 text-yellow-700',
  Declined: 'bg-red-100 text-red-700',
};

export function ConversationList({ conversations, selectedId, onSelect }: ConversationListProps) {
  if (conversations.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full p-8 text-center">
        <MessageSquare className="w-12 h-12 text-gray-300 mb-3" />
        <p className="text-gray-500 font-medium">No conversations yet</p>
        <p className="text-gray-400 text-sm mt-1">
          Accept a proposal to start chatting with your exchange partner.
        </p>
      </div>
    );
  }

  return (
    <div className="flex flex-col divide-y divide-gray-100 overflow-y-auto">
      {conversations.map((conv) => {
        const isSelected = selectedId === conv.proposalId;
        const statusColor = statusColors[conv.proposalStatus] ?? 'bg-gray-100 text-gray-600';

        return (
          <button
            key={conv.proposalId}
            onClick={() => onSelect(conv.proposalId)}
            className={`w-full text-left px-4 py-4 hover:bg-gray-50 transition-colors flex items-start gap-3 relative ${
              isSelected ? 'bg-emerald-50 border-l-4 border-emerald-500' : 'border-l-4 border-transparent'
            }`}
          >
            {/* Avatar */}
            <div className="flex-shrink-0 w-11 h-11 rounded-full overflow-hidden bg-emerald-100 flex items-center justify-center">
              {conv.otherUserProfilePicture ? (
                <img
                  src={conv.otherUserProfilePicture}
                  alt={conv.otherUsername}
                  className="w-full h-full object-cover"
                />
              ) : (
                <span className="text-emerald-700 font-semibold text-sm">
                  {getInitials(conv.otherUsername)}
                </span>
              )}
            </div>

            {/* Content */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between mb-0.5">
                <span className={`font-semibold text-sm truncate ${isSelected ? 'text-emerald-900' : 'text-gray-900'}`}>
                  {conv.otherUsername}
                </span>
                <span className="text-xs text-gray-400 flex-shrink-0 ml-2">
                  {timeAgo(conv.lastMessageAt)}
                </span>
              </div>

              {/* Skill exchange */}
              <p className="text-xs text-gray-500 truncate mb-1">
                {conv.offeringSkillName} ↔ {conv.receivingSkillName}
              </p>

              <div className="flex items-center justify-between gap-2">
                {/* Last message preview */}
                <p className="text-xs text-gray-500 truncate">
                  {conv.lastMessage
                    ? conv.lastMessage.length > 55
                      ? conv.lastMessage.slice(0, 55) + '…'
                      : conv.lastMessage
                    : 'No messages yet'}
                </p>

                {/* Unread badge */}
                {conv.unreadCount > 0 && (
                  <span className="flex-shrink-0 w-5 h-5 bg-blue-500 text-white text-xs font-bold rounded-full flex items-center justify-center">
                    {conv.unreadCount > 9 ? '9+' : conv.unreadCount}
                  </span>
                )}
              </div>
            </div>
          </button>
        );
      })}
    </div>
  );
}
