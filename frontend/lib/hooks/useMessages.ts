import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { messagesApi } from "@/lib/api/messages";
import type { SendMessagePayload } from "@/lib/types/messages";

export const MESSAGE_QUERY_KEYS = {
  messages: (proposalId: number, page?: number) =>
    ["messages", proposalId, page ?? 1] as const,
  conversations: () => ["conversations"] as const,
  unreadCount: () => ["unread-count"] as const,
};

export function useMessages(proposalId: number, page = 1, pageSize = 50) {
  return useQuery({
    queryKey: MESSAGE_QUERY_KEYS.messages(proposalId, page),
    queryFn: () => messagesApi.getMessages(proposalId, page, pageSize),
    enabled: proposalId > 0,
    staleTime: 10_000,
  });
}

export function useConversations() {
  return useQuery({
    queryKey: MESSAGE_QUERY_KEYS.conversations(),
    queryFn: messagesApi.getConversations,
    refetchInterval: 30_000,
    staleTime: 15_000,
  });
}

export function useUnreadCount() {
  return useQuery({
    queryKey: MESSAGE_QUERY_KEYS.unreadCount(),
    queryFn: messagesApi.getUnreadCount,
    refetchInterval: 15_000,
    staleTime: 10_000,
  });
}

export function useSendMessage(proposalId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: SendMessagePayload) => messagesApi.sendMessage(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.messages(proposalId) });
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.conversations() });
    },
  });
}

export function useMarkAsRead(proposalId: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => messagesApi.markAsRead(proposalId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.unreadCount() });
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.conversations() });
    },
  });
}
