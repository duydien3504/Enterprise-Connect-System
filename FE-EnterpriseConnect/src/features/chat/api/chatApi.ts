import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type {
  ConversationItemDto,
  ConversationMemberDto,
  ConversationSummaryResult,
  CreateGroupPayload,
  Message,
  SendMessagePayload,
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
   * Lấy danh sách tin nhắn của cuộc trò chuyện (GET /api/Messages)
   */
  getMessages: async (conversationId: string, page = 1, pageSize = 50): Promise<Message[]> => {
    const { data } = await apiClient.get<Message[]>(API_ENDPOINTS.MESSAGES.BASE, {
      params: { conversationId, page, pageSize },
    });
    return data;
  },

  /**
   * Gửi tin nhắn mới (POST /api/Messages)
   */
  sendMessage: async (payload: SendMessagePayload): Promise<Message> => {
    const { data } = await apiClient.post<Message>(API_ENDPOINTS.MESSAGES.BASE, payload);
    return data;
  },
};
