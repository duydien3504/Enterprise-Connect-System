export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  createdAt: string;
  isRecalled?: boolean;
  isPinned?: boolean;
}

export interface Conversation {
  id: string;
  title: string;
  isGroup: boolean;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount?: number;
  avatarUrl?: string;
  participants: Array<{
    id: string;
    fullName: string;
    status?: 'online' | 'offline' | 'busy' | 'away';
  }>;
}

export interface SendMessagePayload {
  conversationId: string;
  content: string;
}
