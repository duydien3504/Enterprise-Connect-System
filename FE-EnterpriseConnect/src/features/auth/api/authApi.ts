import { apiClient } from '@/core/api/apiClient';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import { tokenStorage } from '@/core/storage/tokenStorage';
import type { LoginRequest, RegisterRequest, AuthResponse, UserProfile } from '../types/auth.types';

export const authApi = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    const { data } = await apiClient.post<AuthResponse>(API_ENDPOINTS.AUTH.LOGIN, credentials);
    if (data.accessToken) {
      tokenStorage.setAccessToken(data.accessToken);
      if (data.refreshToken) tokenStorage.setRefreshToken(data.refreshToken);
      if (data.user) tokenStorage.setUser(data.user);
    }
    return data;
  },

  register: async (payload: RegisterRequest): Promise<AuthResponse> => {
    const { data } = await apiClient.post<AuthResponse>(API_ENDPOINTS.AUTH.REGISTER, payload);
    return data;
  },

  logout: async (): Promise<void> => {
    try {
      await apiClient.post(API_ENDPOINTS.AUTH.LOGOUT);
    } finally {
      tokenStorage.clear();
    }
  },

  getProfile: async (): Promise<UserProfile> => {
    const { data } = await apiClient.get<UserProfile>(API_ENDPOINTS.USERS.PROFILE);
    return data;
  },
};
