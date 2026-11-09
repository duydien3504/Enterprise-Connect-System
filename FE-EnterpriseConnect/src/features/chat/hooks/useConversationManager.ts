import { useState, useEffect, useCallback, useMemo } from 'react';
import { chatApi } from '../api/chatApi';
import type {
  ConversationItemDto,
  ConversationMemberDto,
  ConversationSummaryResult,
} from '../types/chat.types';

export interface ChatFeedback {
  type: 'success' | 'error';
  text: string;
}

export function useConversationManager(initialTargetUserId?: string | null) {
  const [conversations, setConversations] = useState<ConversationItemDto[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);

  // Cache maps for conversation-specific data
  const [membersMap, setMembersMap] = useState<Record<string, ConversationMemberDto[]>>({});
  const [summariesMap, setSummariesMap] = useState<Record<string, ConversationSummaryResult>>({});

  // Loading flags
  const [isLoadingConversations, setIsLoadingConversations] = useState(false);
  const [isLoadingMembers, setIsLoadingMembers] = useState(false);
  const [isSummarizing, setIsSummarizing] = useState(false);
  const [feedback, setFeedback] = useState<ChatFeedback | null>(null);

  // Auto clear feedback after 4 seconds
  useEffect(() => {
    if (!feedback) return;
    const timer = setTimeout(() => setFeedback(null), 4000);
    return () => clearTimeout(timer);
  }, [feedback]);

  // Load conversations list
  const loadConversations = useCallback(async () => {
    try {
      setIsLoadingConversations(true);
      const list = await chatApi.getConversations();
      setConversations(list || []);

      // If no active conversation yet and list has items, select the first one
      if (list && list.length > 0) {
        setActiveConversationId((prev) => {
          if (prev && list.some((c) => c.id === prev)) return prev;
          return list[0].id;
        });
      }
    } catch {
      setFeedback({ type: 'error', text: 'Không thể tải danh sách cuộc hội thoại. Vui lòng thử lại.' });
    } finally {
      setIsLoadingConversations(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  // Load members of a conversation
  const loadMembers = useCallback(async (conversationId: string) => {
    if (!conversationId) return;
    try {
      setIsLoadingMembers(true);
      const members = await chatApi.getConversationMembers(conversationId);
      setMembersMap((prev) => ({ ...prev, [conversationId]: members || [] }));
    } catch {
      // Non-blocking error
    } finally {
      setIsLoadingMembers(false);
    }
  }, []);

  // When active conversation changes, load members if not cached
  useEffect(() => {
    if (activeConversationId && !membersMap[activeConversationId]) {
      loadMembers(activeConversationId);
    }
  }, [activeConversationId, membersMap, loadMembers]);

  // Select conversation
  const selectConversation = useCallback(
    (id: string) => {
      setActiveConversationId(id);
      if (!membersMap[id]) {
        loadMembers(id);
      }
    },
    [membersMap, loadMembers]
  );

  // Handle direct conversation creation/opening
  const handleCreateDirect = useCallback(
    async (targetUserId: string) => {
      try {
        setIsLoadingConversations(true);
        const { conversationId } = await chatApi.createDirectConversation(targetUserId);
        await loadConversations();
        setActiveConversationId(conversationId);
        loadMembers(conversationId);
        setFeedback({ type: 'success', text: 'Đã mở cuộc trò chuyện trực tiếp.' });
        return conversationId;
      } catch {
        setFeedback({ type: 'error', text: 'Không thể tạo cuộc trò chuyện 1-1. Vui lòng thử lại.' });
        return null;
      } finally {
        setIsLoadingConversations(false);
      }
    },
    [loadConversations, loadMembers]
  );

  // If initialTargetUserId is passed (e.g. from Contacts tab), trigger handleCreateDirect
  useEffect(() => {
    if (initialTargetUserId) {
      handleCreateDirect(initialTargetUserId);
    }
  }, [initialTargetUserId, handleCreateDirect]);

  // Handle group conversation creation
  const handleCreateGroup = useCallback(
    async (groupName: string, memberIds: string[]) => {
      try {
        setIsLoadingConversations(true);
        const { conversationId } = await chatApi.createGroupConversation({
          groupName,
          memberIds,
        });
        await loadConversations();
        setActiveConversationId(conversationId);
        loadMembers(conversationId);
        setFeedback({ type: 'success', text: `Đã tạo nhóm "${groupName}" thành công.` });
        return conversationId;
      } catch {
        setFeedback({ type: 'error', text: 'Không thể tạo nhóm trò chuyện mới. Vui lòng kiểm tra lại.' });
        return null;
      } finally {
        setIsLoadingConversations(false);
      }
    },
    [loadConversations, loadMembers]
  );

  // Handle AI Summarization
  const handleSummarize = useCallback(
    async (count = 50, language = 'vi') => {
      if (!activeConversationId) return;
      try {
        setIsSummarizing(true);
        const result = await chatApi.summarizeConversation(activeConversationId, count, language);
        setSummariesMap((prev) => ({
          ...prev,
          [activeConversationId]: result,
        }));
        setFeedback({ type: 'success', text: 'Trợ lý AI đã tổng hợp thành công cuộc trò chuyện!' });
      } catch {
        setFeedback({
          type: 'error',
          text: 'Không thể tóm tắt cuộc hội thoại. Vui lòng đảm bảo hội thoại có đủ tin nhắn.',
        });
      } finally {
        setIsSummarizing(false);
      }
    },
    [activeConversationId]
  );

  // Computed active conversation & active members/summary
  const activeConversation = useMemo(() => {
    return (
      conversations.find((c) => c.id === activeConversationId) ||
      conversations[0] ||
      null
    );
  }, [conversations, activeConversationId]);

  const activeMembers = useMemo(() => {
    if (!activeConversationId) return [];
    return membersMap[activeConversationId] || [];
  }, [membersMap, activeConversationId]);

  const activeSummary = useMemo(() => {
    if (!activeConversationId) return null;
    return summariesMap[activeConversationId] || null;
  }, [summariesMap, activeConversationId]);

  return {
    conversations,
    activeConversationId,
    activeConversation,
    activeMembers,
    activeSummary,
    isLoadingConversations,
    isLoadingMembers,
    isSummarizing,
    feedback,
    setFeedback,
    loadConversations,
    selectConversation,
    loadMembers,
    handleCreateDirect,
    handleCreateGroup,
    handleSummarize,
  };
}
