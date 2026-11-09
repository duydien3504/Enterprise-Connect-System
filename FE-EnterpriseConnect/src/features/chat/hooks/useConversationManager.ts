import { useState, useEffect, useCallback, useMemo } from 'react';
import { chatApi } from '../api/chatApi';
import { userResolver } from '../api/userResolver';
import { tokenStorage } from '@/core/storage/tokenStorage';
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
  const currentUser = tokenStorage.getUser();
  const currentUserId = currentUser?.id;

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

  // Load members and enrich with profiles
  const loadMembers = useCallback(
    async (conversationId: string) => {
      if (!conversationId) return;
      try {
        setIsLoadingMembers(true);
        const rawMembers = await chatApi.getConversationMembers(conversationId);

        // Enrich each member with FullName, Avatar, Department from User Resolver
        const enrichedMembers: ConversationMemberDto[] = await Promise.all(
          rawMembers.map(async (m) => {
            const isSelf = Boolean(
              currentUserId && m.userId.toLowerCase() === currentUserId.toLowerCase()
            );
            const profile = await userResolver.resolveUser(m.userId);

            return {
              ...m,
              fullName: isSelf ? `${currentUser?.fullName || 'Bạn'} (Bạn)` : profile.fullName,
              avatarUrl: isSelf ? currentUser?.avatarUrl : profile.avatarUrl,
              email: isSelf ? currentUser?.email : profile.email,
              departmentName: isSelf ? currentUser?.departmentName : profile.departmentName,
              position: isSelf ? currentUser?.position : profile.position,
            };
          })
        );

        setMembersMap((prev) => ({ ...prev, [conversationId]: enrichedMembers }));
        return enrichedMembers;
      } catch {
        return [];
      } finally {
        setIsLoadingMembers(false);
      }
    },
    [currentUserId, currentUser]
  );

  // Load conversations list and resolve partner names for Direct 1-1 chats
  const loadConversations = useCallback(async () => {
    try {
      setIsLoadingConversations(true);
      const list = await chatApi.getConversations();

      // Resolve partner info for each 1-1 direct conversation
      const enrichedList = await Promise.all(
        (list || []).map(async (conv) => {
          if (conv.type === 1) {
            try {
              const members = await chatApi.getConversationMembers(conv.id);
              const otherMember = members.find(
                (m) => currentUserId && m.userId.toLowerCase() !== currentUserId.toLowerCase()
              );
              if (otherMember) {
                const partner = await userResolver.resolveUser(otherMember.userId);
                return {
                  ...conv,
                  partnerId: otherMember.userId,
                  partnerName: partner.fullName,
                  partnerAvatar: partner.avatarUrl,
                };
              }
            } catch {
              // Ignore and keep original
            }
          }
          return conv;
        })
      );

      setConversations(enrichedList);

      // Select conversation
      if (enrichedList.length > 0) {
        setActiveConversationId((prev) => {
          if (prev && enrichedList.some((c) => c.id === prev)) return prev;
          return enrichedList[0].id;
        });
      }
    } catch {
      setFeedback({ type: 'error', text: 'Không thể tải danh sách cuộc hội thoại. Vui lòng thử lại.' });
    } finally {
      setIsLoadingConversations(false);
    }
  }, [currentUserId]);

  // Initial load
  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

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
        const partner = await userResolver.resolveUser(targetUserId);
        const { conversationId } = await chatApi.createDirectConversation(targetUserId);

        await loadConversations();
        setActiveConversationId(conversationId);
        await loadMembers(conversationId);

        setFeedback({
          type: 'success',
          text: `Đã mở cuộc trò chuyện với ${partner.fullName}.`,
        });
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
        await loadMembers(conversationId);
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
    currentUser,
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
