export type ConversationFilter = 'all' | 'unread' | 'groups' | 'direct';

export interface UserParticipant {
  id: string;
  fullName: string;
  email: string;
  role: 'owner' | 'admin' | 'member';
  department?: string;
  status: 'online' | 'offline' | 'busy' | 'away';
  avatarUrl?: string;
}

export interface MessageAttachment {
  id: string;
  name: string;
  size: string;
  type: 'pdf' | 'image' | 'doc' | 'archive';
  url?: string;
}

export interface MessageReaction {
  emoji: string;
  count: number;
  users: string[]; // List of user names
  hasReacted?: boolean;
}

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  createdAt: string;
  isSelf: boolean;
  replyTo?: {
    id: string;
    senderName: string;
    content: string;
  };
  attachments?: MessageAttachment[];
  reactions?: MessageReaction[];
  isPinned?: boolean;
  isToxic?: boolean;
  toxicWarning?: string;
}

export interface Conversation {
  id: string;
  title: string;
  isGroup: boolean;
  department?: string;
  avatarUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount?: number;
  isPinned?: boolean;
  participants: UserParticipant[];
  pinnedMessage?: Message;
}

export interface ActionItem {
  id: string;
  title: string;
  assignee: string;
  dueDate: string;
  completed: boolean;
}

export interface AISummaryReport {
  conversationId: string;
  conversationTitle: string;
  generatedAt: string;
  overview: string;
  keyDecisions: string[];
  actionItems: ActionItem[];
  deadlines: Array<{
    task: string;
    date: string;
    priority: 'high' | 'medium' | 'low';
  }>;
}

export interface SendMessagePayload {
  conversationId: string;
  content: string;
  replyToId?: string;
  attachments?: MessageAttachment[];
}

export interface CreateConversationPayload {
  title: string;
  isGroup: boolean;
  department?: string;
  participantIds: string[];
}
