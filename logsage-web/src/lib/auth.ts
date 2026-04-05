import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from './types';

interface AuthState {
  user: User | null;
  tokenExpiry: number | null;
  isLoading: boolean;
  setUser: (user: User | null, tokenExpiry?: number | null) => void;
  setLoading: (loading: boolean) => void;
  clearUser: () => void;
  isSessionExpired: () => boolean;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      tokenExpiry: null,
      isLoading: true,
      setUser: (user, tokenExpiry = null) => set({ user, tokenExpiry }),
      setLoading: (isLoading) => set({ isLoading }),
      clearUser: () => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('access_token');
          localStorage.removeItem('refresh_token');
        }
        set({ user: null, tokenExpiry: null });
      },
      isSessionExpired: () => {
        const { tokenExpiry } = get();
        if (!tokenExpiry) return false;
        return Date.now() > tokenExpiry;
      },
      logout: () => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('access_token');
          localStorage.removeItem('refresh_token');
        }
        set({ user: null, tokenExpiry: null });
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ user: state.user, tokenExpiry: state.tokenExpiry }),
      onRehydrateStorage: () => (state) => {
        // Set isLoading to false once rehydration is complete
        state?.setLoading(false);
      },
    }
  )
);

export const isPro = (user: User | null) =>
  user?.plan === 'pro' || user?.plan === 'team';
