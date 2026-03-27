'use client';

import { useEffect, useRef } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Send, Loader2, ExternalLink, Wifi, WifiOff } from 'lucide-react';
import Link from 'next/link';
import { useMessages, useSendMessage } from '@/lib/hooks/useMessages';
import { useSignalR } from '@/lib/hooks/useSignalR';
import type { Message, Conversation } from '@/lib/types/messages';

const messageSchema = z.object({
  content: z
    .string()
    .min(1, 'Message cannot be empty.')
    .max(2000, 'Message cannot exceed 2000 characters.'),
});
type MessageForm = z.infer<typeof messageSchema>;

interface ChatThreadProps {
  proposalId: number;
  currentUserId: string;
  conversation: Conversation;
}

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function groupByDate(messages: Message[]): { label: string; messages: Message[] }[] {
  const groups: { label: string; messages: Message[] }[] = [];
  const map = new Map<string, Message[]>();

  for (const msg of messages) {
    const date = new Date(msg.sentAt);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);

    let label: string;
    if (date.toDateString() === today.toDateString()) {
      label = 'Today';
    } else if (date.toDateString() === yesterday.toDateString()) {
      label = 'Yesterday';
    } else {
      label = date.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
    }

    if (!map.has(label)) {
      map.set(label, []);
      groups.push({ label, messages: map.get(label)! });
    }
    map.get(label)!.push(msg);
  }

  return groups;
}

function MessageBubble({ message }: { message: Message }) {
  const isOwn = message.isOwnMessage;
  const time = new Date(message.sentAt).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });

  return (
    <div className={`flex ${isOwn ? 'justify-end' : 'justify-start'} mb-3`}>
      {!isOwn && (
        <div className="flex-shrink-0 w-8 h-8 rounded-full bg-zinc-200 overflow-hidden mr-2 self-end">
          {message.senderProfilePicture ? (
            <img
              src={message.senderProfilePicture}
              alt={message.senderUsername}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-zinc-500 text-xs font-bold">
              {message.senderUsername.charAt(0).toUpperCase()}
            </div>
          )}
        </div>
      )}

      <div className="max-w-[70%]">
        {!isOwn && (
          <p className="text-xs text-zinc-500 mb-1 ml-1">{message.senderUsername}</p>
        )}
        <div
          className={`px-4 py-2.5 rounded-2xl text-sm leading-relaxed break-words ${
            isOwn
              ? 'bg-zinc-900 text-white rounded-br-sm'
              : 'bg-zinc-100 text-zinc-900 rounded-bl-sm'
          }`}
        >
          {message.messageContent}
        </div>
        <p className={`text-xs text-zinc-400 mt-1 ${isOwn ? 'text-right mr-1' : 'ml-1'}`}>
          {time}
        </p>
      </div>
    </div>
  );
}

export function ChatThread({ proposalId, currentUserId, conversation }: ChatThreadProps) {
  const bottomRef = useRef<HTMLDivElement>(null);
  const { data: messageData, isLoading } = useMessages(proposalId, 1, 50);
  const sendMessage = useSendMessage(proposalId);
  const { isConnected } = useSignalR(proposalId, currentUserId);

  const messages = messageData?.messages ?? [];
  const groups = groupByDate(messages);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<MessageForm>({ resolver: zodResolver(messageSchema) });

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages.length]);

  const onSubmit = (data: MessageForm) => {
    sendMessage.mutate(
      { proposalId, messageContent: data.content },
      { onSuccess: () => reset() }
    );
  };

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="p-4 border-b border-zinc-200 bg-white flex-shrink-0">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full overflow-hidden bg-zinc-100 flex items-center justify-center flex-shrink-0">
              {conversation.otherUserProfilePicture ? (
                <img
                  src={conversation.otherUserProfilePicture}
                  alt={conversation.otherUsername}
                  className="w-full h-full object-cover"
                />
              ) : (
                <span className="text-zinc-600 font-semibold text-sm">
                  {getInitials(conversation.otherUsername)}
                </span>
              )}
            </div>
            <div>
              <h3 className="font-semibold text-zinc-900 text-sm">{conversation.otherUsername}</h3>
              <p className="text-xs text-zinc-500">
                {conversation.offeringSkillName} ↔ {conversation.receivingSkillName}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {/* Connection indicator */}
            <div className="flex items-center gap-1.5">
              {isConnected ? (
                <Wifi className="w-3.5 h-3.5 text-[#1D9E75]" />
              ) : (
                <WifiOff className="w-3.5 h-3.5 text-zinc-400" />
              )}
              <span className={`text-xs ${isConnected ? 'text-[#1D9E75]' : 'text-zinc-400'}`}>
                {isConnected ? 'Live' : 'Offline'}
              </span>
            </div>

            {/* Link to proposal */}
            <Link
              href={`/proposals/${proposalId}`}
              className="flex items-center gap-1 text-xs text-zinc-600 hover:text-zinc-900 transition-colors"
              title="View proposal"
            >
              <ExternalLink className="w-3.5 h-3.5" />
            </Link>
          </div>
        </div>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-4 bg-zinc-50">
        {isLoading ? (
          <div className="flex items-center justify-center h-full">
            <Loader2 className="w-6 h-6 animate-spin text-[#1D9E75]" />
          </div>
        ) : messages.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-center">
            <p className="text-zinc-500 text-sm">No messages yet.</p>
            <p className="text-zinc-400 text-xs mt-1">Say hi to get the conversation started!</p>
          </div>
        ) : (
          <>
            {groups.map((group) => (
              <div key={group.label}>
                {/* Date separator */}
                <div className="flex items-center gap-3 my-4">
                  <div className="flex-1 h-px bg-zinc-200" />
                  <span className="text-xs text-zinc-400 font-medium px-2">{group.label}</span>
                  <div className="flex-1 h-px bg-zinc-200" />
                </div>
                {group.messages.map((msg) => (
                  <MessageBubble key={msg.messageId} message={msg} />
                ))}
              </div>
            ))}
            <div ref={bottomRef} />
          </>
        )}
      </div>

      {/* Input */}
      <div className="p-3 bg-white border-t border-zinc-200 flex-shrink-0">
        <form onSubmit={handleSubmit(onSubmit)} className="flex items-end gap-2">
          <div className="flex-1">
            <textarea
              {...register('content')}
              placeholder="Type a message…"
              rows={1}
              className="w-full px-4 py-2.5 border border-zinc-200 rounded-xl text-sm resize-none focus:outline-none focus:ring-2 focus:ring-[#1D9E75] focus:border-[#1D9E75] bg-white"
              onKeyDown={(e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                  e.preventDefault();
                  handleSubmit(onSubmit)();
                }
              }}
            />
            {errors.content && (
              <p className="text-red-500 text-xs mt-1">{errors.content.message}</p>
            )}
          </div>
          <button
            type="submit"
            disabled={sendMessage.isPending}
            className="p-2.5 bg-[#1D9E75] hover:bg-[#0F6E56] disabled:opacity-50 text-white rounded-xl transition-colors flex-shrink-0"
          >
            {sendMessage.isPending ? (
              <Loader2 className="w-4 h-4 animate-spin" />
            ) : (
              <Send className="w-4 h-4" />
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
