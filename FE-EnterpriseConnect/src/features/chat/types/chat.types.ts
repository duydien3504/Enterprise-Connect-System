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
  users: string[];
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
  name?: string | null;
  isGroup: boolean;
  type?: number; // 1 = Direct, 2 = Group
  department?: string;
  avatarUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount?: number;
  memberCount?: number;
  isPinned?: boolean;
  participants: UserParticipant[];
  pinnedMessage?: Message;
}

// Backend DTOs from Messaging Service
export interface ConversationItemDto {
  id: string;
  type: number; // 1 = Direct, 2 = Group
  name?: string | null;
  avatarUrl?: string | null;
  memberCount: number;
  lastMessageAt?: string | null;
  createdAt: string;
  partnerId?: string;
  partnerName?: string;
  partnerAvatar?: string | null;
}

export interface ConversationMemberDto {
  userId: string;
  memberRole: number; // 1 = Owner/Admin, 2 = Member
  nickname?: string | null;
  joinedAt: string;
  lastReadAt?: string | null;
  lastReadMessageId?: string | null;
  // Enriched profile fields
  fullName?: string;
  avatarUrl?: string | null;
  email?: string;
  departmentName?: string | null;
  position?: string | null;
}

export interface ActionItemDto {
  id?: string;
  description: string;
  assignee?: string | null;
  dueDate?: string | null;
  isCompleted?: boolean;
}

export interface ConversationSummaryResult {
  summary: string;
  keyPoints: string[];
  decisions: string[];
  actionItems: ActionItemDto[];
  participants: string[];
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

export interface CreateDirectPayload {
  targetUserId: string;
}

export interface CreateGroupPayload {
  groupName: string;
  memberIds: string[];
}
