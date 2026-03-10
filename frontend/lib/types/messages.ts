export interface Message {
  messageId: number;
  proposalId: number;
  senderId: string;
  senderUsername: string;
  senderProfilePicture?: string;
  messageContent: string;
  sentAt: string;
  isRead: boolean;
  isOwnMessage: boolean;
}

export interface SendMessagePayload {
  proposalId: number;
  messageContent: string;
}

export interface Conversation {
  proposalId: number;
  otherUserId: string;
  otherUsername: string;
  otherUserProfilePicture?: string;
  offeringSkillName: string;
  receivingSkillName: string;
  proposalStatus: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface MessageListResponse {
  messages: Message[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}
