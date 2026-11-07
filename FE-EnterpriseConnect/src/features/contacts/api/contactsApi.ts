import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type { Contact } from '../types/contact.types';

export const contactsApi = {
  getContacts: async (): Promise<Contact[]> => {
    const { data } = await apiClient.get<Contact[]>(API_ENDPOINTS.CONTACTS.BASE);
    return data;
  },

  sendRequest: async (targetUserId: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.CONTACTS.REQUEST(targetUserId));
  },

  respondRequest: async (contactId: string, accept: boolean): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.CONTACTS.RESPOND(contactId), { accept });
  },

  blockUser: async (targetUserId: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.CONTACTS.BLOCK(targetUserId));
  },
};
