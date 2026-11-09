import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { chatApi } from '../api/chatApi';
import { userResolver } from '../api/userResolver';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { signalRManager } from '@/core/signalr/signalRManager';
import type {
  ConversationItemDto,
  ConversationMemberDto,
  ConversationSummaryResult,
  Message,
  MessageDto,
} from '../types/chat.types';

export interface ChatFeedback {
  type: 'success' | 'error';
  text: string;
}

export function useConversationManager(initialTargetUserId?: string | null) {
  const currentUser = tokenStorage.getUser();
  const currentUserId = currentUser?.id;

  const [conversations, setConversations] = useState<ConversationItemDto[]>([]);
  const conversationsRef = useRef<ConversationItemDto[]>([]);
  conversationsRef.current = conversations;

  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const handledTargetIdRef = useRef<string | null>(null);

  // Cache maps for conversation-specific data
  const [membersMap, setMembersMap] = useState<Record<string, ConversationMemberDto[]>>({});
  const [messagesMap, setMessagesMap] = useState<Record<string, Message[]>>({});
  const [pinnedMessagesMap, setPinnedMessagesMap] = useState<Record<string, Message[]>>({});
  const [summariesMap, setSummariesMap] = useState<Record<string, ConversationSummaryResult>>({});

  // Loading flags
  const [isLoadingConversations, setIsLoadingConversations] = useState(false);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
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

  // Load real messages & pinned messages from Swagger endpoints (GET /api/Messages/conversation/{id})
  const loadMessages = useCallback(
    async (conversationId: string) => {
      if (!conversationId) return [];
      try {
        setIsLoadingMessages(true);
        const [rawMessages, rawPinned] = await Promise.all([
          chatApi.getConversationMessages(conversationId, 0, 50).catch(() => [] as MessageDto[]),
          chatApi.getPinnedMessages(conversationId).catch(() => [] as MessageDto[]),
        ]);

        const convertDtoToMessage = async (dto: MessageDto): Promise<Message> => {
          const isSelf = Boolean(
            currentUserId && dto.senderId.toLowerCase() === currentUserId.toLowerCase()
          );

          let senderName = isSelf ? 'Tôi' : 'Đồng nghiệp';
          let senderAvatar: string | undefined = isSelf ? currentUser?.avatarUrl || undefined : undefined;

          if (dto.senderId === '00000000-0000-0000-0000-000000000000') {
            senderName = 'Trợ lý AI';
          } else if (!isSelf) {
            try {
              const profile = await userResolver.resolveUser(dto.senderId);
              senderName = profile.fullName;
              senderAvatar = profile.avatarUrl || undefined;
            } catch {
              // fallback
            }
          }

          const reactionCounts: Record<string, { count: number; users: string[] }> = {};
          for (const r of dto.reactions || []) {
            if (!reactionCounts[r]) reactionCounts[r] = { count: 0, users: [] };
            reactionCounts[r].count += 1;
          }
          const formattedReactions = Object.entries(reactionCounts).map(([emoji, data]) => ({
            emoji,
            count: data.count,
            users: data.users,
          }));

          const time = new Date(dto.createdAt).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
          });

          return {
            id: dto.id,
            conversationId: dto.conversationId,
            senderId: dto.senderId,
            senderName,
            senderAvatar,
            content: dto.content,
            createdAt: time,
            isSelf,
            isPinned: dto.isPinned,
            type: dto.type,
            status: dto.status,
            reactions: formattedReactions,
          };
        };

        const convertedMessages = await Promise.all(rawMessages.map(convertDtoToMessage));
        const convertedPinned = await Promise.all(rawPinned.map(convertDtoToMessage));

        // Link replyTo quotes
        const msgMap = new Map<string, Message>();
        convertedMessages.forEach((m) => msgMap.set(m.id, m));
        for (let i = 0; i < convertedMessages.length; i++) {
          const raw = rawMessages[i];
          if (raw?.replyToId && msgMap.has(raw.replyToId)) {
            const target = msgMap.get(raw.replyToId)!;
            convertedMessages[i].replyTo = {
              id: target.id,
              senderName: target.senderName,
              content: target.content,
            };
          }
        }

        setMessagesMap((prev) => ({ ...prev, [conversationId]: convertedMessages }));
        setPinnedMessagesMap((prev) => ({ ...prev, [conversationId]: convertedPinned }));
        return convertedMessages;
      } catch {
        return [];
      } finally {
        setIsLoadingMessages(false);
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

      // Deduplicate direct conversations by partnerId
      const uniqueList: ConversationItemDto[] = [];
      const seenPartners = new Set<string>();

      for (const conv of enrichedList) {
        if (conv.type === 1 && conv.partnerId) {
          const key = conv.partnerId.toLowerCase();
          if (seenPartners.has(key)) {
            continue; // Skip duplicate direct conversation with the same partner
          }
          seenPartners.add(key);
        }
        uniqueList.push(conv);
      }

      setConversations(uniqueList);

      // Join all user conversation groups on SignalR
      uniqueList.forEach((c) => {
        signalRManager.joinConversation(c.id);
      });

      // Select conversation
      if (uniqueList.length > 0) {
        setActiveConversationId((prev) => {
          if (prev && uniqueList.some((c) => c.id === prev)) return prev;
          return uniqueList[0].id;
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

  // Realtime SignalR Listeners for incoming messages and typing events
  const [typingUser, setTypingUser] = useState<{ conversationId: string; userId: string; name: string } | null>(null);

  useEffect(() => {
    const unsubscribeMsg = signalRManager.onReceiveMessage(async (msg: MessageDto) => {
      console.log('[SignalR] Received realtime message:', msg);
      const isSelf = Boolean(currentUserId && msg.senderId.toLowerCase() === currentUserId.toLowerCase());

      let senderName = isSelf ? 'Tôi' : 'Đồng nghiệp';
      let senderAvatar: string | undefined = undefined;

      if (!isSelf) {
        try {
          const partner = await userResolver.resolveUser(msg.senderId);
          senderName = partner.fullName;
          senderAvatar = partner.avatarUrl ?? undefined;
        } catch {
          // fallback
        }
      } else {
        senderAvatar = currentUser?.avatarUrl ?? undefined;
      }

      const formattedMsg: Message = {
        id: msg.id,
        conversationId: msg.conversationId,
        senderId: msg.senderId,
        senderName,
        senderAvatar,
        content: msg.content,
        createdAt: new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        isSelf,
        isPinned: msg.isPinned,
        type: msg.type,
        status: msg.status,
      };

      // 1. Update messages map
      setMessagesMap((prev) => {
        const currentList = prev[msg.conversationId] || [];
        if (currentList.some((m) => m.id === msg.id)) {
          return prev;
        }
        const filtered = currentList.filter(
          (m) => !(m.id.startsWith('temp-') && m.content === msg.content)
        );
        return {
          ...prev,
          [msg.conversationId]: [...filtered, formattedMsg],
        };
      });

      // 2. Update conversations list in sidebar
      setConversations((prev) => {
        const exists = prev.some((c) => c.id === msg.conversationId);
        if (!exists) {
          loadConversations();
          return prev;
        }

        const updated = prev.map((c) => {
          if (c.id === msg.conversationId) {
            return {
              ...c,
              lastMessage: msg.content,
              lastMessageAt: new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
            };
          }
          return c;
        });

        return [...updated].sort((a, b) => {
          if (a.id === msg.conversationId) return -1;
          if (b.id === msg.conversationId) return 1;
          return 0;
        });
      });
    });

    const unsubscribeTyping = signalRManager.onUserTyping(async (convId, userId, isTyping) => {
      if (currentUserId && userId.toLowerCase() === currentUserId.toLowerCase()) return;

      if (isTyping) {
        try {
          const partner = await userResolver.resolveUser(userId);
          setTypingUser({ conversationId: convId, userId, name: partner.fullName });
        } catch {
          setTypingUser({ conversationId: convId, userId, name: 'Đồng nghiệp' });
        }
      } else {
        setTypingUser((prev) => (prev?.userId.toLowerCase() === userId.toLowerCase() ? null : prev));
      }
    });

    const unsubscribeReaction = signalRManager.onMessageReacted(async (messageId, userId, emoji, isAdded) => {
      console.log('[SignalR] Realtime MessageReacted:', { messageId, userId, emoji, isAdded });
      const isSelf = Boolean(currentUserId && userId.toLowerCase() === currentUserId.toLowerCase());
      let userName = isSelf ? 'Bạn' : 'Đồng nghiệp';
      if (!isSelf) {
        try {
          const partner = await userResolver.resolveUser(userId);
          userName = partner.fullName;
        } catch {
          // fallback
        }
      }

      setMessagesMap((prev) => {
        let changed = false;
        const updatedMap: Record<string, Message[]> = {};
        for (const [convId, msgList] of Object.entries(prev)) {
          if (msgList.some((m) => m.id === messageId)) {
            changed = true;
            updatedMap[convId] = msgList.map((m) => {
              if (m.id !== messageId) return m;
              const existing = [...(m.reactions || [])];
              const idx = existing.findIndex((r) => r.emoji === emoji);

              if (isAdded) {
                if (idx >= 0) {
                  const curr = existing[idx];
                  const users = curr.users.includes(userName) ? curr.users : [...curr.users, userName];
                  existing[idx] = { ...curr, count: curr.count + 1, users, hasReacted: isSelf ? true : curr.hasReacted };
                } else {
                  existing.push({ emoji, count: 1, users: [userName], hasReacted: isSelf });
                }
              } else {
                if (idx >= 0) {
                  const curr = existing[idx];
                  const users = curr.users.filter((u) => u !== userName);
                  if (curr.count <= 1) {
                    existing.splice(idx, 1);
                  } else {
                    existing[idx] = { ...curr, count: curr.count - 1, users, hasReacted: isSelf ? false : curr.hasReacted };
                  }
                }
              }
              return { ...m, reactions: existing };
            });
          } else {
            updatedMap[convId] = msgList;
          }
        }
        return changed ? updatedMap : prev;
      });
    });

    const unsubscribePinned = signalRManager.onMessagePinned((messageId, isPinned) => {
      console.log('[SignalR] Realtime MessagePinned:', { messageId, isPinned });
      setMessagesMap((prev) => {
        let changed = false;
        const updatedMap: Record<string, Message[]> = {};
        for (const [convId, msgList] of Object.entries(prev)) {
          if (msgList.some((m) => m.id === messageId)) {
            changed = true;
            updatedMap[convId] = msgList.map((m) =>
              m.id === messageId ? { ...m, isPinned } : m
            );
          } else {
            updatedMap[convId] = msgList;
          }
        }
        return changed ? updatedMap : prev;
      });
    });

    const unsubscribeToxic = signalRManager.onToxicWarning((warning) => {
      setFeedback({ type: 'error', text: warning });
    });

    return () => {
      unsubscribeMsg();
      unsubscribeTyping();
      unsubscribeReaction();
      unsubscribePinned();
      unsubscribeToxic();
    };
  }, [currentUserId, currentUser, loadConversations]);

  const activeTypingName = useMemo(() => {
    if (typingUser && activeConversationId && typingUser.conversationId.toLowerCase() === activeConversationId.toLowerCase()) {
      return typingUser.name;
    }
    return null;
  }, [typingUser, activeConversationId]);

  // When active conversation changes, join its SignalR group and load members and messages if not cached
  useEffect(() => {
    if (activeConversationId) {
      signalRManager.joinConversation(activeConversationId);
      if (!membersMap[activeConversationId]) {
        loadMembers(activeConversationId);
      }
      if (!messagesMap[activeConversationId]) {
        loadMessages(activeConversationId);
      }
    }
  }, [activeConversationId, membersMap, messagesMap, loadMembers, loadMessages]);

  // Select conversation
  const selectConversation = useCallback(
    (id: string) => {
      setActiveConversationId(id);
      signalRManager.joinConversation(id);
      loadMembers(id);
      loadMessages(id);
    },
    [loadMembers, loadMessages]
  );

  // Handle direct conversation creation/opening (Singleton pattern per partner)
  const handleCreateDirect = useCallback(
    async (targetUserId: string) => {
      try {
        setIsLoadingConversations(true);
        const partner = await userResolver.resolveUser(targetUserId);

        // 1. In-memory check: if conversation already exists, immediately switch to it
        const existing = conversationsRef.current.find(
          (c) => c.type === 1 && c.partnerId?.toLowerCase() === targetUserId.toLowerCase()
        );
        if (existing) {
          setActiveConversationId(existing.id);
          loadMembers(existing.id);
          loadMessages(existing.id);
          setFeedback({
            type: 'success',
            text: `Đã mở cuộc trò chuyện với ${partner.fullName}.`,
          });
          return existing.id;
        }

        // 2. Otherwise create/fetch via backend API
        const { conversationId } = await chatApi.createDirectConversation(targetUserId);

        await loadConversations();
        setActiveConversationId(conversationId);
        await loadMembers(conversationId);
        await loadMessages(conversationId);

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
    [loadConversations, loadMembers, loadMessages]
  );

  // If initialTargetUserId is passed (e.g. from Contacts tab), trigger handleCreateDirect once
  useEffect(() => {
    if (initialTargetUserId && handledTargetIdRef.current !== initialTargetUserId) {
      handledTargetIdRef.current = initialTargetUserId;
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
        await loadMessages(conversationId);
        setFeedback({ type: 'success', text: `Đã tạo nhóm "${groupName}" thành công.` });
        return conversationId;
      } catch {
        setFeedback({ type: 'error', text: 'Không thể tạo nhóm trò chuyện mới. Vui lòng kiểm tra lại.' });
        return null;
      } finally {
        setIsLoadingConversations(false);
      }
    },
    [loadConversations, loadMembers, loadMessages]
  );

  // Handle sending message with Optimistic UI & Toxic Content check
  const handleSendMessage = useCallback(
    async (content: string, replyToId?: string) => {
      if (!activeConversationId || !content.trim()) return;

      const trimmedContent = content.trim();
      const tempId = `temp-${Date.now()}`;
      const nowTime = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

      // Optimistic UI message
      const optimisticMsg: Message = {
        id: tempId,
        conversationId: activeConversationId,
        senderId: currentUserId || 'self',
        senderName: 'Tôi',
        senderAvatar: currentUser?.avatarUrl || undefined,
        content: trimmedContent,
        createdAt: nowTime,
        isSelf: true,
      };

      setMessagesMap((prev) => ({
        ...prev,
        [activeConversationId]: [...(prev[activeConversationId] || []), optimisticMsg],
      }));

      // Immediately update sidebar for sender
      setConversations((prev) => {
        const updated = prev.map((c) => {
          if (c.id === activeConversationId) {
            return {
              ...c,
              lastMessage: trimmedContent,
              lastMessageAt: nowTime,
            };
          }
          return c;
        });
        return [...updated].sort((a, b) => {
          if (a.id === activeConversationId) return -1;
          if (b.id === activeConversationId) return 1;
          return 0;
        });
      });

      try {
        const result = await chatApi.sendMessage({
          conversationId: activeConversationId,
          content: trimmedContent,
          replyToId,
        });

        if (result.isToxicBlocked) {
          const toxicMsg: Message = {
            id: `toxic-${Date.now()}`,
            conversationId: activeConversationId,
            senderId: currentUserId || 'self',
            senderName: 'Bộ Lọc An Ninh Nội Bộ',
            content: trimmedContent,
            createdAt: nowTime,
            isSelf: true,
            isToxic: true,
            toxicWarning:
              result.toxicReason || 'Nội dung tin nhắn bị chặn do vi phạm chính sách bảo mật nội bộ.',
          };

          setMessagesMap((prev) => ({
            ...prev,
            [activeConversationId]: (prev[activeConversationId] || []).map((m) =>
              m.id === tempId ? toxicMsg : m
            ),
          }));

          setFeedback({
            type: 'error',
            text: `Tin nhắn bị chặn: ${result.toxicReason || 'Vi phạm chính sách an toàn'}`,
          });
          return;
        }

        // Successfully sent: replace optimistic temp message with real server message
        if (result.message) {
          const serverMsg: Message = {
            id: result.message.id,
            conversationId: result.message.conversationId,
            senderId: result.message.senderId,
            senderName: 'Tôi',
            senderAvatar: currentUser?.avatarUrl,
            content: result.message.content,
            createdAt: new Date(result.message.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
            isSelf: true,
            isPinned: result.message.isPinned,
            type: result.message.type,
            status: result.message.status,
          };
          setMessagesMap((prev) => ({
            ...prev,
            [activeConversationId]: (prev[activeConversationId] || [])
              .filter((m) => m.id !== tempId && m.id !== serverMsg.id)
              .concat(serverMsg),
          }));
        } else {
          await loadMessages(activeConversationId);
        }
      } catch (err: any) {
        setMessagesMap((prev) => ({
          ...prev,
          [activeConversationId]: (prev[activeConversationId] || []).filter((m) => m.id !== tempId),
        }));
        const errMsg = err?.response?.data?.message || 'Không thể gửi tin nhắn. Vui lòng thử lại.';
        setFeedback({ type: 'error', text: errMsg });
      }
    },
    [activeConversationId, currentUserId, currentUser, loadMessages]
  );

  // Handle pin/unpin message (POST /api/Messages/{id}/pin)
  const handleTogglePin = useCallback(
    async (messageId: string) => {
      if (!activeConversationId) return;
      try {
        const { isPinned, message } = await chatApi.togglePinMessage(messageId);
        setMessagesMap((prev) => ({
          ...prev,
          [activeConversationId]: (prev[activeConversationId] || []).map((m) =>
            m.id === messageId ? { ...m, isPinned } : m
          ),
        }));

        setFeedback({
          type: 'success',
          text: message || (isPinned ? 'Đã ghim tin nhắn.' : 'Đã bỏ ghim tin nhắn.'),
        });

        // Trigger load messages to synchronize pinned list
        await loadMessages(activeConversationId);
      } catch {
        setFeedback({ type: 'error', text: 'Không thể thay đổi trạng thái ghim tin nhắn.' });
      }
    },
    [activeConversationId, loadMessages]
  );

  // Handle reaction (POST /api/Messages/{id}/reaction)
  const handleReact = useCallback(
    async (messageId: string, emoji: string) => {
      if (!activeConversationId) return;
      try {
        await chatApi.reactMessage(messageId, emoji);
      } catch (err) {
        console.warn('[Chat] Failed to react message:', err);
      }
    },
    [activeConversationId]
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

  // Computed active conversation & active members/summary/messages
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

  const activeMessages = useMemo(() => {
    if (!activeConversationId) return [];
    return messagesMap[activeConversationId] || [];
  }, [messagesMap, activeConversationId]);

  const activePinnedMessages = useMemo(() => {
    if (!activeConversationId) return [];
    return pinnedMessagesMap[activeConversationId] || [];
  }, [pinnedMessagesMap, activeConversationId]);

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
    activeMessages,
    activePinnedMessages,
    activeSummary,
    activeTypingName,
    isLoadingConversations,
    isLoadingMessages,
    isLoadingMembers,
    isSummarizing,
    feedback,
    setFeedback,
    loadConversations,
    loadMessages,
    selectConversation,
    loadMembers,
    handleCreateDirect,
    handleCreateGroup,
    handleSendMessage,
    handleTogglePin,
    handleReact,
    handleSummarize,
  };
}
