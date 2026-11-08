import { authApi } from '@/features/auth/api/authApi';

/**
 * Enterprise Token Auto-Refresh Scheduler
 * Proactively refreshes the Access Token in RAM 60 seconds before it expires,
 * ensuring continuous, uninterrupted enterprise operations.
 */
let refreshTimerId: ReturnType<typeof setTimeout> | null = null;

export const tokenScheduler = {
  /**
   * Schedule the next proactive token refresh
   * @param expiresInSeconds Lifetime in seconds (default 900 = 15m)
   */
  scheduleRefresh: (expiresInSeconds: number = 900): void => {
    tokenScheduler.stop();

    // Trigger refresh 60 seconds before expiry (minimum 5s delay)
    const refreshDelaySeconds = Math.max(5, expiresInSeconds - 60);
    const delayMs = refreshDelaySeconds * 1000;

    refreshTimerId = setTimeout(async () => {
      try {
        console.info('[TokenScheduler] Proactively refreshing Access Token before RAM expiration...');
        await authApi.refreshToken();
      } catch (err) {
        console.warn('[TokenScheduler] Proactive refresh failed, falling back to reactive 401 interceptor.', err);
      }
    }, delayMs);
  },

  /**
   * Stop the scheduled refresh timer (e.g. on logout or session termination)
   */
  stop: (): void => {
    if (refreshTimerId !== null) {
      clearTimeout(refreshTimerId);
      refreshTimerId = null;
    }
  },
};
