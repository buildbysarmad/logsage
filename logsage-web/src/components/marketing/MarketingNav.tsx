'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { useAuthStore } from '@/lib/auth';
import { authApi } from '@/lib/api';

export function MarketingNav() {
  const user = useAuthStore((s) => s.user);
  const isLoading = useAuthStore((s) => s.isLoading);
  const setUser = useAuthStore((s) => s.setUser);
  const setLoading = useAuthStore((s) => s.setLoading);

  useEffect(() => {
    // Fetch user on mount if not already loaded
    if (user === null && !isLoading) {
      const token = localStorage.getItem('access_token');
      if (token) {
        setLoading(true);
        authApi
          .me()
          .then(({ data }) => setUser(data))
          .catch(() => setUser(null))
          .finally(() => setLoading(false));
      }
    }
  }, [user, isLoading, setUser, setLoading]);

  return (
    <nav className="flex items-center justify-between px-6 py-4 border-b border-gray-800 max-w-6xl mx-auto">
      <Link href="/" className="text-xl font-semibold text-white">
        log<span className="text-emerald-400">sage</span>
      </Link>
      <div className="flex items-center gap-4">
        <Link
          href="/pricing"
          className="text-sm text-gray-400 hover:text-white transition-colors"
        >
          Pricing
        </Link>
        {user ? (
          <Link
            href="/dashboard"
            className="flex items-center justify-center w-8 h-8 rounded-full bg-emerald-600 hover:bg-emerald-500 text-white text-sm font-medium transition-colors"
            title={user.email}
          >
            {user.email.charAt(0).toUpperCase()}
          </Link>
        ) : (
          <>
            <Link
              href="/login"
              className="text-sm text-gray-400 hover:text-white transition-colors"
            >
              Sign in
            </Link>
            <Link
              href="/analyze"
              className="text-sm bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-1.5 rounded-lg transition-colors"
            >
              Try free
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}
