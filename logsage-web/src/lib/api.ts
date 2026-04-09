import axios, { AxiosInstance } from 'axios';
import type { AnalysisResult, AuthTokens, User, SessionSummary } from './types';

const createApiClient = (): AxiosInstance => {
  const instance = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    withCredentials: true,
  });

  instance.interceptors.request.use((config) => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('access_token');
      if (token) config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  instance.interceptors.response.use(
    (res) => res,
    async (err) => {
      const original = err.config;

      // Handle network errors (API unreachable)
      if (!err.response) {
        console.error('[API] Network error - API unreachable:', err.message);

        // Only clear auth and redirect if this was an authenticated request
        const hasAuthToken = typeof window !== 'undefined' && localStorage.getItem('access_token');
        if (hasAuthToken) {
          console.log('[API] Clearing auth state due to network error');

          // Clear localStorage
          if (typeof window !== 'undefined') {
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
          }

          // Clear zustand store
          try {
            const { useAuthStore } = await import('./auth');
            const { logout } = useAuthStore.getState();
            logout();
          } catch (e) {
            console.error('[API] Failed to clear auth store:', e);
          }

          // Redirect to login with reason
          if (typeof window !== 'undefined') {
            window.location.href = '/login?reason=network';
          }
        }

        return Promise.reject(err);
      }

      // Handle 401 - try to refresh token
      if (err.response?.status === 401 && !original._retry) {
        original._retry = true;
        try {
          const refresh = localStorage.getItem('refresh_token');
          if (!refresh) throw new Error('No refresh token');

          const { data } = await axios.post<AuthTokens>(
            `${process.env.NEXT_PUBLIC_API_URL}/api/auth/refresh`,
            { refreshToken: refresh }
          );

          localStorage.setItem('access_token', data.accessToken);
          localStorage.setItem('refresh_token', data.refreshToken);
          original.headers.Authorization = `Bearer ${data.accessToken}`;
          return instance(original);
        } catch (refreshError) {
          // Refresh failed - clear everything and redirect
          console.log('[API] Token refresh failed, clearing auth state');

          // Clear localStorage first
          if (typeof window !== 'undefined') {
            localStorage.removeItem('access_token');
            localStorage.removeItem('refresh_token');
          }

          // Clear zustand store
          try {
            const { useAuthStore } = await import('./auth');
            const { logout } = useAuthStore.getState();
            logout();
          } catch (e) {
            console.error('[API] Failed to clear auth store:', e);
          }

          // Redirect to login
          if (typeof window !== 'undefined') {
            window.location.href = '/login?reason=expired';
          }

          return Promise.reject(refreshError);
        }
      }
      return Promise.reject(err);
    }
  );

  return instance;
};

export const api = createApiClient();

export const analyzeApi = {
  text: (rawLog: string) =>
    api.post<AnalysisResult>('/api/analyze', { rawLog }),

  file: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return api.post<AnalysisResult>('/api/analyze/upload', form);
  },

  getSessions: () =>
    api.get<SessionSummary[]>('/api/sessions'),

  getSession: (id: string) =>
    api.get<AnalysisResult>(`/api/sessions/${id}`),
};

export const authApi = {
  register: (email: string, password: string) =>
    api.post<AuthTokens>('/api/auth/register', { email, password }),

  login: (email: string, password: string) =>
    api.post<AuthTokens>('/api/auth/login', { email, password }),

  logout: () => {
    const refresh = typeof window !== 'undefined'
      ? localStorage.getItem('refresh_token') : null;
    return api.post('/api/auth/logout', { refreshToken: refresh });
  },

  me: () => api.get<User>('/api/auth/me'),

  changePassword: (currentPassword: string, newPassword: string) =>
    api.post('/api/auth/change-password', { currentPassword, newPassword }),
};

export const billingApi = {
  checkout: (priceId: string) =>
    api.post<{ checkoutUrl: string }>('/api/billing/checkout', { priceId }),

  portal: () =>
    api.get<{ portalUrl: string }>('/api/billing/portal'),
};
