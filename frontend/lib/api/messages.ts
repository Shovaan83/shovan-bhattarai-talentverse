import axiosInstance from "@/lib/axios";
import type {
  Conversation,
  Message,
  MessageListResponse,
  SendMessagePayload,
} from "@/lib/types/messages";

export const messagesApi = {
  sendMessage: async (payload: SendMessagePayload): Promise<Message> => {
    const response = await axiosInstance.post<{ data: Message }>("/messages", payload);
    return response.data.data;
  },

  getMessages: async (
    proposalId: number,
    page = 1,
    pageSize = 50
  ): Promise<MessageListResponse> => {
    const response = await axiosInstance.get<{ data: MessageListResponse }>(
      `/messages/proposal/${proposalId}`,
      { params: { page, pageSize } }
    );
    return response.data.data;
  },

  getConversations: async (): Promise<Conversation[]> => {
    const response = await axiosInstance.get<{ data: Conversation[] }>("/messages/conversations");
    return response.data.data;
  },

  markAsRead: async (proposalId: number): Promise<number> => {
    const response = await axiosInstance.put<{ data: number }>(
      `/messages/proposal/${proposalId}/read`
    );
    return response.data.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const response = await axiosInstance.get<{ data: number }>("/messages/unread-count");
    return response.data.data;
  },
};
