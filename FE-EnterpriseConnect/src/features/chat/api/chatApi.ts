import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type {
  ConversationItemDto,
  ConversationMemberDto,
  ConversationSummaryResult,
  CreateGroupPayload,
  MessageDto,
  SendMessagePayload,
  SendMessageResultDto,
} from '../types/chat.types';

export const chatApi = {
  /**
   * Lấy danh sách cuộc hội thoại của người dùng (GET /api/Conversations)
   */
  getConversations: async (): Promise<ConversationItemDto[]> => {
    const { data } = await apiClient.get<ConversationItemDto[]>(API_ENDPOINTS.CONVERSATIONS.BASE);
    return data;
  },

  /**
   * Tạo hoặc mở cuộc hội thoại 1-1 với đồng nghiệp (POST /api/Conversations/direct)
   */
  createDirectConversation: async (targetUserId: string): Promise<{ conversationId: string }> => {
    const { data } = await apiClient.post<{ conversationId: string }>(
      API_ENDPOINTS.CONVERSATIONS.DIRECT,
      { targetUserId }
    );
    return data;
  },

  /**
   * Tạo nhóm trò chuyện dự án/phòng ban mới (POST /api/Conversations/group)
   */
  createGroupConversation: async (payload: CreateGroupPayload): Promise<{ conversationId: string }> => {
    const { data } = await apiClient.post<{ conversationId: string }>(
      API_ENDPOINTS.CONVERSATIONS.GROUP,
      payload
    );
    return data;
  },

  /**
   * Tóm tắt cuộc hội thoại bằng AI Copilot (POST /api/Conversations/{id}/summarize)
   */
  summarizeConversation: async (
    conversationId: string,
    count = 50,
    language = 'vi'
  ): Promise<ConversationSummaryResult> => {
    const { data } = await apiClient.post<ConversationSummaryResult>(
      API_ENDPOINTS.CONVERSATIONS.SUMMARIZE(conversationId),
      null,
      {
        params: { count, language },
      }
    );
    return data;
  },

  /**
   * Lấy danh sách thành viên trong cuộc hội thoại (GET /api/Conversations/{id}/members)
   */
  getConversationMembers: async (conversationId: string): Promise<ConversationMemberDto[]> => {
    const { data } = await apiClient.get<ConversationMemberDto[]>(
      API_ENDPOINTS.CONVERSATIONS.MEMBERS(conversationId)
    );
    return data;
  },

  /**
   * Lấy danh sách tin nhắn của cuộc trò chuyện (GET /api/Messages/conversation/{conversationId})
   */
  getConversationMessages: async (
    conversationId: string,
    skip = 0,
    take = 50
  ): Promise<MessageDto[]> => {
    const { data } = await apiClient.get<MessageDto[]>(
      API_ENDPOINTS.MESSAGES.BY_CONVERSATION(conversationId),
      {
        params: { skip, take },
      }
    );
    return data;
  },

  /**
   * Lấy danh sách tin nhắn đã ghim (GET /api/Messages/conversation/{conversationId}/pinned)
   */
  getPinnedMessages: async (conversationId: string): Promise<MessageDto[]> => {
    const { data } = await apiClient.get<MessageDto[]>(
      API_ENDPOINTS.MESSAGES.PINNED(conversationId)
    );
    return data;
  },

  /**
   * Gửi tin nhắn mới (POST /api/Messages)
   */
  sendMessage: async (payload: SendMessagePayload): Promise<SendMessageResultDto> => {
    const { data } = await apiClient.post<SendMessageResultDto>(API_ENDPOINTS.MESSAGES.BASE, {
      conversationId: payload.conversationId,
      content: payload.content,
      type: payload.type || 1,
      replyToId: payload.replyToId || null,
    });
    return data;
  },

  /**
   * Ghim hoặc bỏ ghim tin nhắn (POST /api/Messages/{id}/pin)
   */
  togglePinMessage: async (messageId: string): Promise<{ isPinned: boolean; message: string }> => {
    const { data } = await apiClient.post<{ isPinned: boolean; message: string }>(
      API_ENDPOINTS.MESSAGES.PIN(messageId)
    );
    return data;
  },

  /**
   * Thả cảm xúc vào tin nhắn (POST /api/Messages/{id}/reaction)
   */
  reactMessage: async (messageId: string, emoji: string): Promise<{ message: string }> => {
    const { data } = await apiClient.post<{ message: string }>(
      API_ENDPOINTS.MESSAGES.REACTION(messageId),
      { emoji }
    );
    return data;
  },
};
