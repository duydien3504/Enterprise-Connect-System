import React, { useState, useEffect, useMemo } from 'react';
import { ConversationSidebar } from './ConversationSidebar';
import { ChatTimeline } from './ChatTimeline';
import { ConversationDetailDrawer } from './ConversationDetailDrawer';
import { CreateConversationModal } from './CreateConversationModal';
import { useConversationManager } from '../hooks/useConversationManager';
import {
  MOCK_CONVERSATIONS,
  MOCK_MESSAGES_MAP,
  MOCK_AI_SUMMARIES,
} from '../mock/chatMockData';
import type {
  Conversation,
  Message,
  MessageAttachment,
} from '../types/chat.types';
import { CheckCircle2, AlertCircle } from 'lucide-react';

interface EnterpriseChatModuleProps {
  targetUserIdToChat?: string | null;
  onDirectChatHandled?: () => void;
}

export const EnterpriseChatModule: React.FC<EnterpriseChatModuleProps> = ({
  targetUserIdToChat,
  onDirectChatHandled,
}) => {
  const {
    conversations: realConversations,
    activeConversationId: realActiveId,
    activeMembers,
    activeSummary,
    isLoadingMembers,
    isSummarizing,
    feedback,
    selectConversation,
    handleCreateDirect,
    handleCreateGroup,
    handleSummarize,
  } = useConversationManager(targetUserIdToChat);

  // Notify parent that direct chat was processed
  useEffect(() => {
    if (targetUserIdToChat && onDirectChatHandled) {
      onDirectChatHandled();
    }
  }, [targetUserIdToChat, onDirectChatHandled]);

  const [activeConversationId, setActiveConversationId] = useState<string>('conv-1');
  const [messagesMap, setMessagesMap] = useState<Record<string, Message[]>>(MOCK_MESSAGES_MAP);
  const [isDrawerOpen, setIsDrawerOpen] = useState(true);
  const [drawerMode, setDrawerMode] = useState<'info' | 'ai-summary'>('info');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  // Sync real active ID when real conversations change
  useEffect(() => {
    if (realActiveId) {
      setActiveConversationId(realActiveId);
    }
  }, [realActiveId]);

  // Merge real conversations with fallback mock data
  const displayConversations = useMemo<Conversation[]>(() => {
    if (realConversations.length === 0) {
      return MOCK_CONVERSATIONS;
    }

    // Defense-in-depth: Deduplicate direct conversations by partnerId or ID
    const seenPartners = new Set<string>();
    const deduplicated = realConversations.filter((c) => {
      if (c.type === 1 && c.partnerId) {
        const key = c.partnerId.toLowerCase();
        if (seenPartners.has(key)) return false;
        seenPartners.add(key);
      }
      return true;
    });

    return deduplicated.map((c) => {
      const isGroup = c.type === 2;
      const title = isGroup
        ? (c.name || 'Nhóm chat')
        : (c.partnerName || c.name || 'Đồng nghiệp');
      const avatarUrl = isGroup
        ? (c.avatarUrl || undefined)
        : (c.partnerAvatar || c.avatarUrl || undefined);

      return {
        id: c.id,
        title,
        name: title,
        isGroup,
        type: c.type,
        memberCount: c.memberCount,
        avatarUrl,
        lastMessage: c.lastMessageAt ? 'Có tin nhắn mới' : 'Chưa có tin nhắn',
        lastMessageAt: c.lastMessageAt
          ? new Date(c.lastMessageAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
          : undefined,
        unreadCount: 0,
        participants: [],
      };
    });
  }, [realConversations]);

  const activeConversation: Conversation =
    displayConversations.find((c) => c.id === activeConversationId) ||
    displayConversations[0] || {
      id: 'default',
      title: 'Hội Thoại Doanh Nghiệp',
      isGroup: false,
      participants: [],
    };

  const activeMessages = messagesMap[activeConversationId] || [];
  const activeAISummary = MOCK_AI_SUMMARIES[activeConversationId];

  // Send message handler with Toxic Content detection simulation
  const handleSendMessage = (
    content: string,
    replyToId?: string,
    attachments?: MessageAttachment[]
  ) => {
    const replyTarget = replyToId ? activeMessages.find((m) => m.id === replyToId) : undefined;

    // Simulate Toxic Content Filter (POST /api/Messages/check-toxic)
    const isToxicContent =
      /lo\s*mat\s*khau|hack|leak|matkhau|password123/i.test(content);

    const newMessage: Message = {
      id: `msg-${Date.now()}`,
      conversationId: activeConversationId,
      senderId: 'current-user',
      senderName: 'Tôi',
      content,
      createdAt: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isSelf: true,
      replyTo: replyTarget
        ? {
            id: replyTarget.id,
            senderName: replyTarget.senderName,
            content: replyTarget.content,
          }
        : undefined,
      attachments,
      isToxic: isToxicContent,
      toxicWarning: isToxicContent
        ? 'Nội dung tin nhắn đã bị chặn tự động do chứa từ khóa nhạy cảm vi phạm chính sách an ninh mạng Zero-Trust.'
        : undefined,
    };

    setMessagesMap((prev) => ({
      ...prev,
      [activeConversationId]: [...(prev[activeConversationId] || []), newMessage],
    }));
  };

  // Add emoji reaction handler (POST /api/Messages/{id}/reaction)
  const handleAddReaction = (messageId: string, emoji: string) => {
    setMessagesMap((prev) => {
      const currentList = prev[activeConversationId] || [];
      const updated = currentList.map((msg) => {
        if (msg.id !== messageId) return msg;

        const currentReactions = msg.reactions || [];
        const existing = currentReactions.find((r) => r.emoji === emoji);

        if (existing) {
          return {
            ...msg,
            reactions: currentReactions.map((r) =>
              r.emoji === emoji
                ? { ...r, count: r.count + 1, users: [...r.users, 'Tôi'] }
                : r
            ),
          };
        } else {
          return {
            ...msg,
            reactions: [...currentReactions, { emoji, count: 1, users: ['Tôi'] }],
          };
        }
      });

      return {
        ...prev,
        [activeConversationId]: updated,
      };
    });
  };

  // Pin message handler (POST /api/Messages/{id}/pin)
  const handlePinMessage = (messageId: string) => {
    setMessagesMap((prev) => {
      const currentList = prev[activeConversationId] || [];
      const updated = currentList.map((m) =>
        m.id === messageId ? { ...m, isPinned: !m.isPinned } : m
      );
      return {
        ...prev,
        [activeConversationId]: updated,
      };
    });
  };

  const handleOpenAISummary = () => {
    setDrawerMode('ai-summary');
    setIsDrawerOpen(true);
  };

  return (
    <div className="flex-1 flex overflow-hidden h-full bg-[#071e3d] relative">
      {/* Floating Feedback Alert */}
      {feedback && (
        <div
          className={`absolute top-4 right-4 z-50 flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-semibold shadow-2xl border animate-in fade-in slide-in-from-top-2 ${
            feedback.type === 'success'
              ? 'bg-emerald-950/90 border-emerald-500/50 text-emerald-200 shadow-emerald-900/30'
              : 'bg-rose-950/90 border-rose-500/50 text-rose-200 shadow-rose-900/30'
          }`}
        >
          {feedback.type === 'success' ? (
            <CheckCircle2 className="w-4 h-4 text-emerald-400 shrink-0" />
          ) : (
            <AlertCircle className="w-4 h-4 text-rose-400 shrink-0" />
          )}
          <span>{feedback.text}</span>
        </div>
      )}

      {/* Column 1: Conversations Sidebar (320px) */}
      <ConversationSidebar
        conversations={displayConversations}
        activeConversationId={activeConversationId}
        onSelectConversation={(id) => {
          setActiveConversationId(id);
          selectConversation(id);
        }}
        onOpenCreateModal={() => setIsCreateModalOpen(true)}
      />

      {/* Column 2: Main Chat Timeline */}
      <ChatTimeline
        conversation={activeConversation}
        messages={activeMessages}
        isDrawerOpen={isDrawerOpen}
        onToggleDrawer={() => setIsDrawerOpen(!isDrawerOpen)}
        onOpenAISummary={handleOpenAISummary}
        onSendMessage={handleSendMessage}
        onAddReaction={handleAddReaction}
        onPinMessage={handlePinMessage}
      />

      {/* Column 3: Details & AI Summary Drawer (340px) */}
      <ConversationDetailDrawer
        isOpen={isDrawerOpen}
        onClose={() => setIsDrawerOpen(false)}
        conversation={activeConversation}
        members={activeMembers}
        isLoadingMembers={isLoadingMembers}
        summary={activeSummary}
        isSummarizing={isSummarizing}
        onTriggerSummarize={handleSummarize}
        aiSummary={activeAISummary}
        activeMode={drawerMode}
        onSwitchMode={(mode) => setDrawerMode(mode)}
      />

      {/* Create Conversation / Group Modal (POST /api/Conversations/direct & group) */}
      <CreateConversationModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onCreateDirect={handleCreateDirect}
        onCreateGroup={handleCreateGroup}
      />
    </div>
  );
};
