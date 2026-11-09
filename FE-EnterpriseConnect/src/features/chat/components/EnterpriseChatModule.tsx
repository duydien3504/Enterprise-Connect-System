import React, { useState, useEffect, useMemo } from 'react';
import { ConversationSidebar } from './ConversationSidebar';
import { ChatTimeline } from './ChatTimeline';
import { ConversationDetailDrawer } from './ConversationDetailDrawer';
import { CreateConversationModal } from './CreateConversationModal';
import { useConversationManager } from '../hooks/useConversationManager';
import type { Conversation } from '../types/chat.types';
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
    activeMessages,
    activePinnedMessages,
    activeSummary,
    isLoadingConversations,
    isLoadingMessages,
    isLoadingMembers,
    isSummarizing,
    feedback,
    selectConversation,
    handleCreateDirect,
    handleCreateGroup,
    handleSendMessage,
    handleTogglePin,
    handleReact,
    handleSummarize,
  } = useConversationManager(targetUserIdToChat);

  const [mobileView, setMobileView] = useState<'list' | 'timeline'>('list');

  // Notify parent that direct chat was processed
  useEffect(() => {
    if (targetUserIdToChat) {
      setMobileView('timeline');
      if (onDirectChatHandled) {
        onDirectChatHandled();
      }
    }
  }, [targetUserIdToChat, onDirectChatHandled]);

  const [activeConversationId, setActiveConversationId] = useState<string>('');
  const [isDrawerOpen, setIsDrawerOpen] = useState(true);
  const [drawerMode, setDrawerMode] = useState<'info' | 'ai-summary'>('info');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  // Sync real active ID when real conversations change
  useEffect(() => {
    if (realActiveId) {
      setActiveConversationId(realActiveId);
    }
  }, [realActiveId]);

  // Transform real conversations for sidebar display
  const displayConversations = useMemo<Conversation[]>(() => {
    if (realConversations.length === 0) {
      return [];
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
      id: activeConversationId || 'empty',
      title: 'Hội Thoại Doanh Nghiệp',
      isGroup: false,
      participants: [],
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

      {/* Column 1: Conversations Sidebar (320px on desktop, full width on mobile when in list view) */}
      <div
        className={`h-full ${
          mobileView === 'list'
            ? 'flex flex-1 w-full md:flex-none md:w-80'
            : 'hidden md:flex md:w-80 shrink-0'
        }`}
      >
        <ConversationSidebar
          conversations={displayConversations}
          activeConversationId={activeConversationId}
          isLoading={isLoadingConversations}
          onSelectConversation={(id) => {
            setActiveConversationId(id);
            selectConversation(id);
            setMobileView('timeline');
          }}
          onOpenCreateModal={() => setIsCreateModalOpen(true)}
        />
      </div>

      {/* Column 2: Main Chat Timeline with Real Messages, Pinned Banner, Reactions */}
      <div
        className={`h-full ${
          mobileView === 'timeline'
            ? 'flex flex-1 w-full'
            : 'hidden md:flex md:flex-1'
        }`}
      >
        <ChatTimeline
          conversation={activeConversation}
          messages={activeMessages}
          isLoadingMessages={isLoadingMessages}
          pinnedMessages={activePinnedMessages}
          isDrawerOpen={isDrawerOpen}
          onToggleDrawer={() => setIsDrawerOpen(!isDrawerOpen)}
          onOpenAISummary={handleOpenAISummary}
          onSendMessage={handleSendMessage}
          onAddReaction={handleReact}
          onPinMessage={handleTogglePin}
          onBackToList={() => setMobileView('list')}
        />
      </div>

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
