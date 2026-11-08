import axios, { type InternalAxiosRequestConfig } from 'axios';
import { ENV } from '@/core/config/env';
import { tokenStorage } from '@/core/storage/tokenStorage';
import { API_ENDPOINTS } from '@/core/api/endpoints';
import type { RefreshTokenRequest, RefreshTokenResponse } from '@/features/auth/types/auth.types';

export const apiClient = axios.create({
  baseURL: ENV.API_BASE_URL,
  withCredentials: true, // CRITICAL: Automatically transmits & receives HttpOnly Cookies across origins
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

// Single in-flight refresh promise to coordinate Cold Start Gatekeeper across concurrent requests
let activeGatekeeperPromise: Promise<string | null> | null = null;

/**
 * Cold Start Request Gatekeeper:
 * When user presses F5 or opens a new tab, RAM Access Token is initially null.
 * Instead of firing requests with no Authorization header (which causes a 401 stampede),
 * this gatekeeper holds outgoing requests until the in-memory token is acquired.
 */
export const requestGatekeeper = async (): Promise<string | null> => {
  const existingToken = tokenStorage.getAccessToken();
  if (existingToken) return existingToken;

  if (!tokenStorage.hasActiveSession()) return null;

  if (!activeGatekeeperPromise) {
    activeGatekeeperPromise = (async () => {
      try {
        const rt = tokenStorage.getRefreshToken();
        if (!rt) return null;

        const refreshPayload: RefreshTokenRequest = {
          refreshToken: rt,
          ipAddress: '',
          deviceInfo: typeof navigator !== 'undefined' ? navigator.userAgent : 'Enterprise Web Client (Chrome/Win64)',
        };

        const { data } = await axios.post<RefreshTokenResponse>(
          `${ENV.API_BASE_URL}${API_ENDPOINTS.AUTH.REFRESH_TOKEN}`,
          refreshPayload,
          {
            withCredentials: true,
            headers: { 'Content-Type': 'application/json' },
            timeout: 12000,
          }
        );

        if (data && data.accessToken) {
          const expiresIn = data.expiresIn || 900;
          tokenStorage.setAccessToken(data.accessToken, expiresIn);
          if (data.refreshToken) {
            tokenStorage.setRefreshToken(data.refreshToken);
          }
          if (data.user) {
            tokenStorage.setUser(data.user);
          }
          tokenStorage.broadcastAuthEvent('REFRESH', { accessToken: data.accessToken, expiresIn });
          return data.accessToken;
        }
        return null;
      } catch (err: unknown) {
        console.warn('[apiClient] Gatekeeper initial refresh failed (grace period active):', err);
        // Only definitively invalidate session if backend explicitly rejected with 401/403
        if (axios.isAxiosError(err) && err.response && (err.response.status === 401 || err.response.status === 403)) {
          console.warn('[apiClient] Refresh token definitively revoked on server. Terminating session.');
          tokenStorage.clear();
          sessionExpiredListeners.forEach((fn) => fn());
        }
        return null;
      } finally {
        activeGatekeeperPromise = null;
      }
    })();
  }

  return activeGatekeeperPromise;
};

// Request Interceptor: Attach JWT Access Token from RAM & Handle Multipart Boundaries
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    let token = tokenStorage.getAccessToken();

    // Cold Start Gatekeeper: If RAM token is null but session is active in Vault,
    // wait for silent refresh instead of sending unauthenticated request (which causes 401 stampede)
    const isAuthEndpoint =
      config.url?.includes(API_ENDPOINTS.AUTH.LOGIN) ||
      config.url?.includes(API_ENDPOINTS.AUTH.REFRESH_TOKEN) ||
      config.url?.includes(API_ENDPOINTS.AUTH.REGISTER);

    if (!token && tokenStorage.hasActiveSession() && !isAuthEndpoint) {
      token = await requestGatekeeper();
    }

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // CRITICAL: When sending FormData, delete Content-Type so Axios/Browser sets
    // "multipart/form-data; boundary=----WebKitFormBoundary..." automatically!
    if (config.data instanceof FormData && config.headers) {
      if (typeof (config.headers as { delete?: (k: string) => void }).delete === 'function') {
        (config.headers as { delete: (k: string) => void }).delete('Content-Type');
        (config.headers as { delete: (k: string) => void }).delete('content-type');
      }
      delete (config.headers as Record<string, unknown>)['Content-Type'];
      delete (config.headers as Record<string, unknown>)['content-type'];
    }

    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: Handle 401 & Concurrent Reactive Silent Refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Registered listeners for session expiration (redirect / return to login)
const sessionExpiredListeners: Set<() => void> = new Set();
export const onSessionExpired = (cb: () => void): (() => void) => {
  sessionExpiredListeners.add(cb);
  return () => sessionExpiredListeners.delete(cb);
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Trigger on 401 Unauthorized only once per request (skip for logout and refresh requests)
    const isRefreshOrLogout =
      originalRequest?.url?.includes(API_ENDPOINTS.AUTH.LOGOUT) ||
      originalRequest?.url?.includes(API_ENDPOINTS.AUTH.REFRESH_TOKEN);

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isRefreshOrLogout) {
      // If we are already refreshing tokens, enqueue this request
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const rt = tokenStorage.getRefreshToken() || '';
        const refreshPayload: RefreshTokenRequest = {
          refreshToken: rt,
          ipAddress: '',
          deviceInfo: typeof navigator !== 'undefined' ? navigator.userAgent : 'Enterprise Web Client (Chrome/Win64)',
        };

        // Call backend refresh endpoint with credentials (HttpOnly cookie is attached automatically)
        const refreshResponse = await axios.post<RefreshTokenResponse>(
          `${ENV.API_BASE_URL}${API_ENDPOINTS.AUTH.REFRESH_TOKEN}`,
          refreshPayload,
          {
            withCredentials: true,
            headers: { 'Content-Type': 'application/json' },
            timeout: 12000,
          }
        );

        const { accessToken, refreshToken: newRefreshToken, expiresIn, user } = refreshResponse.data;
        if (!accessToken) {
          throw new Error('No access token returned in refresh response body');
        }

        // Store new Access Token strictly in RAM (15 minutes)
        tokenStorage.setAccessToken(accessToken, expiresIn || 900);
        if (newRefreshToken) {
          tokenStorage.setRefreshToken(newRefreshToken);
        }
        if (user) {
          tokenStorage.setUser(user);
        }

        tokenStorage.broadcastAuthEvent('REFRESH', { accessToken, expiresIn });

        processQueue(null, accessToken);
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError: unknown) {
        processQueue(refreshError, null);

        // Grace Period Rule: Only invalidate session if server explicitly revoked token with 401/403
        // and we received a definitive server response (NOT a network glitch or gateway timeout)
        if (
          axios.isAxiosError(refreshError) &&
          refreshError.response &&
          (refreshError.response.status === 401 || refreshError.response.status === 403)
        ) {
          console.warn('[apiClient] Refresh token definitively revoked on server. Terminating session.');
          tokenStorage.clear();
          sessionExpiredListeners.forEach((fn) => fn());
        } else {
          console.warn('[apiClient] Network timeout or temporary glitch during token refresh. Session retained.');
        }

        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
