"use client";

import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Send, MessageSquare, Loader2, X } from "lucide-react";
import { useMessages, useSendMessage } from "@/lib/hooks/useMessages";
import { useSignalR } from "@/lib/hooks/useSignalR";
import type { Message } from "@/lib/types/messages";

const messageSchema = z.object({
  content: z
    .string()
    .min(1, "Message cannot be empty.")
    .max(2000, "Message cannot exceed 2000 characters."),
});
type MessageForm = z.infer<typeof messageSchema>;

interface ChatPanelProps {
  proposalId: number;
  currentUserId: string;
  onClose: () => void;
}

function MessageBubble({ message }: { message: Message }) {
  const isOwn = message.isOwnMessage;
  return (
    <div className={`flex ${isOwn ? "justify-end" : "justify-start"} mb-3`}>
      {!isOwn && (
        <div className="flex-shrink-0 w-8 h-8 rounded-full bg-gray-200 overflow-hidden mr-2 self-end">
          {message.senderProfilePicture ? (
            <img
              src={message.senderProfilePicture}
              alt={message.senderUsername}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-gray-500 text-xs font-bold">
              {message.senderUsername.charAt(0).toUpperCase()}
            </div>
          )}
        </div>
      )}
      <div className="max-w-[75%]">
        {!isOwn && (
          <p className="text-xs text-gray-500 mb-1 ml-1">{message.senderUsername}</p>
        )}
        <div
          className={`px-4 py-2 rounded-2xl text-sm leading-relaxed break-words ${
            isOwn
              ? "bg-emerald-500 text-white rounded-br-sm"
              : "bg-white text-gray-800 border border-gray-100 rounded-bl-sm shadow-sm"
          }`}
        >
          {message.messageContent}
        </div>
        <p
          className={`text-xs text-gray-400 mt-1 ${isOwn ? "text-right" : "text-left ml-1"}`}
        >
          {new Date(message.sentAt).toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit",
          })}
        </p>
      </div>
    </div>
  );
}

function groupByDate(messages: Message[]) {
  const groups: { label: string; messages: Message[] }[] = [];
  const map = new Map<string, Message[]>();

  for (const msg of messages) {
    const date = new Date(msg.sentAt).toLocaleDateString([], {
      weekday: "long",
      month: "short",
      day: "numeric",
    });
    if (!map.has(date)) {
      map.set(date, []);
      groups.push({ label: date, messages: map.get(date)! });
    }
    map.get(date)!.push(msg);
  }

  return groups;
}

export default function ChatPanel({
  proposalId,
  currentUserId,
  onClose,
}: ChatPanelProps) {
  const bottomRef = useRef<HTMLDivElement>(null);
  const { data, isLoading } = useMessages(proposalId, 1, 50);
  const sendMutation = useSendMessage(proposalId);
  const { isConnected } = useSignalR(proposalId, currentUserId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<MessageForm>({ resolver: zodResolver(messageSchema) });

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [data?.messages.length]);

  const onSubmit = async (values: MessageForm) => {
    if (sendMutation.isPending) return;
    try {
      await sendMutation.mutateAsync({
        proposalId,
        messageContent: values.content,
      });
      reset();
    } catch {
      // error handled by mutation state
    }
  };

  const groups = groupByDate(data?.messages ?? []);

  return (
    <div className="flex flex-col h-[500px] bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-5 py-4 bg-gradient-to-r from-emerald-500 to-emerald-600">
        <div className="flex items-center gap-2">
          <MessageSquare size={18} className="text-white" />
          <span className="font-semibold text-white text-sm">Proposal Chat</span>
          {isConnected && (
            <span className="w-2 h-2 rounded-full bg-emerald-200 animate-pulse" />
          )}
        </div>
        <button
          onClick={onClose}
          className="text-white/80 hover:text-white transition-colors"
        >
          <X size={18} />
        </button>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto px-4 py-4 bg-gray-50">
        {isLoading ? (
          <div className="flex items-center justify-center h-full">
            <Loader2 size={24} className="animate-spin text-emerald-500" />
          </div>
        ) : groups.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-center">
            <MessageSquare size={32} className="text-gray-300 mb-2" />
            <p className="text-gray-400 text-sm">No messages yet.</p>
            <p className="text-gray-300 text-xs mt-1">Start the conversation!</p>
          </div>
        ) : (
          <>
            {groups.map((group) => (
              <div key={group.label}>
                <div className="flex items-center gap-2 my-4">
                  <div className="flex-1 h-px bg-gray-200" />
                  <span className="text-xs text-gray-400 px-2">{group.label}</span>
                  <div className="flex-1 h-px bg-gray-200" />
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
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="px-4 py-3 bg-white border-t border-gray-100"
      >
        {errors.content && (
          <p className="text-red-500 text-xs mb-1">{errors.content.message}</p>
        )}
        <div className="flex gap-2">
          <input
            {...register("content")}
            placeholder="Type a message..."
            autoComplete="off"
            className="flex-1 px-4 py-2 text-sm text-gray-900 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-300 focus:border-transparent placeholder:text-gray-400"
          />
          <button
            type="submit"
            disabled={sendMutation.isPending}
            className="flex items-center justify-center w-10 h-10 bg-emerald-500 hover:bg-emerald-600 text-white rounded-xl transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {sendMutation.isPending ? (
              <Loader2 size={16} className="animate-spin" />
            ) : (
              <Send size={16} />
            )}
          </button>
        </div>
      </form>
    </div>
  );
}
