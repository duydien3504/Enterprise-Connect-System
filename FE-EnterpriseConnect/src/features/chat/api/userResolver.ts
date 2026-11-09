import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { contactsApi } from '@/features/contacts/api/contactsApi';

export interface UserBasicProfile {
  id: string;
  fullName: string;
  avatarUrl?: string | null;
  email?: string;
  departmentName?: string | null;
  position?: string | null;
}

// In-memory cache to avoid duplicate network calls
const profileCache = new Map<string, UserBasicProfile>();

export const userResolver = {
  /**
   * Truy vấn thông tin người dùng từ Cache -> Contacts -> API Users/{id}
   */
  resolveUser: async (userId: string): Promise<UserBasicProfile> => {
    if (!userId) {
      return { id: '', fullName: 'Người dùng' };
    }

    const normalizedId = userId.toLowerCase();
    const currentUser = tokenStorage.getUser();

    // 1. Kiểm tra nếu là chính người dùng hiện tại
    if (currentUser && currentUser.id?.toLowerCase() === normalizedId) {
      const selfProfile: UserBasicProfile = {
        id: currentUser.id,
        fullName: currentUser.fullName || 'Bạn',
        avatarUrl: currentUser.avatarUrl,
        email: currentUser.email,
        departmentName: currentUser.departmentName,
        position: currentUser.position,
      };
      profileCache.set(normalizedId, selfProfile);
      return selfProfile;
    }

    // 2. Kiểm tra bộ nhớ cache
    if (profileCache.has(normalizedId)) {
      return profileCache.get(normalizedId)!;
    }

    // 3. Thử tìm trong danh bạ bạn bè đã nạp
    try {
      const contacts = await contactsApi.getContacts('accepted');
      const foundInContact = contacts.find((c) => c.userId?.toLowerCase() === normalizedId);
      if (foundInContact) {
        const contactProfile: UserBasicProfile = {
          id: foundInContact.userId,
          fullName: foundInContact.fullName,
          avatarUrl: foundInContact.avatarUrl,
          email: foundInContact.email,
          departmentName: foundInContact.departmentName,
          position: foundInContact.position,
        };
        profileCache.set(normalizedId, contactProfile);
        return contactProfile;
      }
    } catch {
      // Tiếp tục fallback sang API Users
    }

    // 4. Gọi API trực tiếp GET /api/Users/{id} qua API Gateway
    try {
      const { data } = await apiClient.get<any>(API_ENDPOINTS.USERS.BY_ID(userId));
      if (data) {
        const resolvedProfile: UserBasicProfile = {
          id: data.id || userId,
          fullName: data.fullName || `Nhân sự ${userId.slice(0, 8)}`,
          avatarUrl: data.avatarUrl || null,
          email: data.email || '',
          departmentName: data.departmentName || null,
          position: data.position || null,
        };
        profileCache.set(normalizedId, resolvedProfile);
        return resolvedProfile;
      }
    } catch {
      // Fallback nếu không truy vấn được
    }

    const fallback: UserBasicProfile = {
      id: userId,
      fullName: `Nhân sự ${userId.slice(0, 8)}`,
    };
    profileCache.set(normalizedId, fallback);
    return fallback;
  },

  /**
   * Phân giải đồng thời nhiều tài khoản
   */
  resolveUsers: async (userIds: string[]): Promise<Map<string, UserBasicProfile>> => {
    const uniqueIds = Array.from(new Set(userIds.filter(Boolean)));
    const resultMap = new Map<string, UserBasicProfile>();

    await Promise.all(
      uniqueIds.map(async (id) => {
        const profile = await userResolver.resolveUser(id);
        resultMap.set(id.toLowerCase(), profile);
      })
    );

    return resultMap;
  },
};
