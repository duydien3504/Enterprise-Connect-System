import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type {
  ContactItem,
  GetUsersResult,
  GetUsersParams,
  ContactFilterType,
  SendContactResponse,
  ActionResponse,
} from '../types/contact.types';

export const contactsApi = {
  /**
   * Lấy danh sách liên hệ theo phân loại
   * @param type 'accepted' | 'pending_received' | 'pending_sent' | 'blocked'
   */
  getContacts: async (type: ContactFilterType = 'accepted'): Promise<ContactItem[]> => {
    const { data } = await apiClient.get<ContactItem[]>(API_ENDPOINTS.CONTACTS.BASE, {
      params: { type },
    });
    return data;
  },

  /**
   * Tra cứu người dùng toàn hệ thống có phân trang & lọc
   */
  searchUsers: async (params: GetUsersParams = {}): Promise<GetUsersResult> => {
    const { data } = await apiClient.get<GetUsersResult>(API_ENDPOINTS.USERS.LIST, {
      params: {
        name: params.name || undefined,
        departmentId: params.departmentId || undefined,
        departmentName: params.departmentName || undefined,
        page: params.page || 1,
        pageSize: params.pageSize || 20,
      },
    });
    return data;
  },

  /**
   * Gửi lời mời kết bạn tới người dùng đích
   */
  sendRequest: async (receiverId: string): Promise<SendContactResponse> => {
    const { data } = await apiClient.post<SendContactResponse>(
      API_ENDPOINTS.CONTACTS.REQUEST(receiverId)
    );
    return data;
  },

  /**
   * Phản hồi lời mời kết bạn (Chấp nhận hoặc Từ chối)
   */
  respondRequest: async (contactId: string, accept: boolean): Promise<ActionResponse> => {
    const { data } = await apiClient.post<ActionResponse>(
      API_ENDPOINTS.CONTACTS.RESPOND(contactId),
      { accept }
    );
    return data;
  },

  /**
   * Hủy kết bạn (Xóa khỏi danh bạ)
   */
  removeContact: async (targetUserId: string): Promise<ActionResponse> => {
    const { data } = await apiClient.delete<ActionResponse>(
      API_ENDPOINTS.CONTACTS.REMOVE(targetUserId)
    );
    return data;
  },

  /**
   * Chặn người dùng
   */
  blockUser: async (targetUserId: string): Promise<ActionResponse> => {
    const { data } = await apiClient.post<ActionResponse>(
      API_ENDPOINTS.CONTACTS.BLOCK(targetUserId)
    );
    return data;
  },

  /**
   * Bỏ chặn người dùng
   */
  unblockUser: async (targetUserId: string): Promise<ActionResponse> => {
    const { data } = await apiClient.post<ActionResponse>(
      API_ENDPOINTS.CONTACTS.UNBLOCK(targetUserId)
    );
    return data;
  },
};
