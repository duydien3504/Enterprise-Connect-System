import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type { Conversation, Message, SendMessagePayload } from '../types/chat.types';

export const chatApi = {
  getConversations: async (): Promise<Conversation[]> => {
    const { data } = await apiClient.get<Conversation[]>(API_ENDPOINTS.CONVERSATIONS.BASE);
    return data;
  },

  getMessages: async (conversationId: string, page = 1, pageSize = 30): Promise<Message[]> => {
    const { data } = await apiClient.get<Message[]>(API_ENDPOINTS.MESSAGES.BASE, {
      params: { conversationId, page, pageSize },
    });
    return data;
  },

  sendMessage: async (payload: SendMessagePayload): Promise<Message> => {
    const { data } = await apiClient.post<Message>(API_ENDPOINTS.MESSAGES.BASE, payload);
    return data;
  },
};
