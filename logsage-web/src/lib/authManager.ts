import { useAuthStore } from './auth';
import { authApi } from './api';

let expiryCheckInterval: NodeJS.Timeout | null = null;
let visibilityListener: (() => void) | null = null;

/**
 * Proactive session expiry check
 * Runs every 60 seconds to check if the token has expired
 */
function startExpiryCheck() {
  if (expiryCheckInterval) return;

  expiryCheckInterval = setInterval(() => {
    const { isSessionExpired, clearUser } = useAuthStore.getState();

    if (isSessionExpired()) {
      console.log('[AuthManager] Session expired (proactive check)');
      clearUser();
      if (typeof window !== 'undefined') {
        window.location.href = '/login?reason=expired';
      }
    }
  }, 60000); // Check every 60 seconds
}

/**
 * Visibility check - validates session when user returns to tab
 * Catches sessions that expired while the tab was in the background
 */
function startVisibilityCheck() {
  if (typeof document === 'undefined' || visibilityListener) return;

  visibilityListener = async () => {
    if (document.visibilityState === 'visible') {
      const { user, clearUser } = useAuthStore.getState();

      // Only check if user is logged in
      if (!user) return;

      try {
        await authApi.me();
      } catch (error: any) {
        if (error?.response?.status === 401) {
          console.log('[AuthManager] Session expired (visibility check)');
          clearUser();
          if (typeof window !== 'undefined') {
            window.location.href = '/login?reason=expired';
          }
        }
      }
    }
  };

  document.addEventListener('visibilitychange', visibilityListener);
}

/**
 * Start auth lifecycle monitoring
 * Call this once when the app mounts (if user is logged in)
 */
export function startAuthManager() {
  const { user } = useAuthStore.getState();

  // Only start if user is logged in
  if (!user) return;

  console.log('[AuthManager] Starting auth lifecycle monitoring');
  // Removed: startExpiryCheck() - no automatic periodic sign-out
  startVisibilityCheck();
}

/**
 * Stop auth lifecycle monitoring
 * Call this on cleanup or logout
 */
export function stopAuthManager() {
  console.log('[AuthManager] Stopping auth lifecycle monitoring');

  if (expiryCheckInterval) {
    clearInterval(expiryCheckInterval);
    expiryCheckInterval = null;
  }

  if (visibilityListener && typeof document !== 'undefined') {
    document.removeEventListener('visibilitychange', visibilityListener);
    visibilityListener = null;
  }
}
