import React, { useState } from 'react';
import { ConversationSidebar } from './ConversationSidebar';
import { ChatTimeline } from './ChatTimeline';
import { ConversationDetailDrawer } from './ConversationDetailDrawer';
import { CreateConversationModal } from './CreateConversationModal';
import {
  MOCK_CONVERSATIONS,
  MOCK_MESSAGES_MAP,
  MOCK_AI_SUMMARIES,
  MOCK_DIRECTORY_USERS,
} from '../mock/chatMockData';
import type {
  Conversation,
  Message,
  MessageAttachment,
  CreateConversationPayload,
} from '../types/chat.types';

export const EnterpriseChatModule: React.FC = () => {
  const [conversations, setConversations] = useState<Conversation[]>(MOCK_CONVERSATIONS);
  const [activeConversationId, setActiveConversationId] = useState<string>('conv-1');
  const [messagesMap, setMessagesMap] = useState<Record<string, Message[]>>(MOCK_MESSAGES_MAP);
  const [isDrawerOpen, setIsDrawerOpen] = useState(true);
  const [drawerMode, setDrawerMode] = useState<'info' | 'ai-summary'>('info');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const activeConversation =
    conversations.find((c) => c.id === activeConversationId) || conversations[0];
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

    // Update message timeline
    setMessagesMap((prev) => ({
      ...prev,
      [activeConversationId]: [...(prev[activeConversationId] || []), newMessage],
    }));

    // Update conversation sidebar last message
    setConversations((prev) =>
      prev.map((c) =>
        c.id === activeConversationId
          ? {
              ...c,
              lastMessage: isToxicContent ? '[Tin nhắn vi phạm chính sách]' : content || 'Đã gửi tệp đính kèm',
              lastMessageAt: newMessage.createdAt,
            }
          : c
      )
    );
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

  // Create new conversation/group (POST /api/Conversations/direct or group)
  const handleCreateConversation = (payload: CreateConversationPayload) => {
    const newConvId = `conv-${Date.now()}`;
    const selectedParticipants = MOCK_DIRECTORY_USERS.filter((u) =>
      payload.participantIds.includes(u.id)
    );

    const newConv: Conversation = {
      id: newConvId,
      title: payload.title,
      isGroup: payload.isGroup,
      department: payload.department,
      lastMessage: 'Hội thoại vừa được khởi tạo.',
      lastMessageAt: 'Vừa xong',
      unreadCount: 0,
      isPinned: false,
      participants: selectedParticipants,
    };

    setConversations((prev) => [newConv, ...prev]);
    setActiveConversationId(newConvId);
    setMessagesMap((prev) => ({
      ...prev,
      [newConvId]: [
        {
          id: `m-init-${Date.now()}`,
          conversationId: newConvId,
          senderId: 'system',
          senderName: 'Hệ Thống EnterpriseConnect',
          content: `Hội thoại "${payload.title}" đã được tạo thành công với chứng chỉ bảo mật Zero-Trust.`,
          createdAt: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          isSelf: false,
        },
      ],
    }));
  };

  const handleOpenAISummary = () => {
    setDrawerMode('ai-summary');
    setIsDrawerOpen(true);
  };

  return (
    <div className="flex-1 flex overflow-hidden h-full bg-[#071e3d]">
      {/* Column 1: Conversations Sidebar (320px) */}
      <ConversationSidebar
        conversations={conversations}
        activeConversationId={activeConversationId}
        onSelectConversation={(id) => {
          setActiveConversationId(id);
          // Mark as read
          setConversations((prev) =>
            prev.map((c) => (c.id === id ? { ...c, unreadCount: 0 } : c))
          );
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
        aiSummary={activeAISummary}
        activeMode={drawerMode}
        onSwitchMode={(mode) => setDrawerMode(mode)}
      />

      {/* Create Conversation / Group Modal */}
      <CreateConversationModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onCreate={handleCreateConversation}
      />
    </div>
  );
};
