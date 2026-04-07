'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { useAuthStore } from '@/lib/auth';
import { startAuthManager, stopAuthManager } from '@/lib/authManager';

export function AppHeader() {
  const user = useAuthStore((s) => s.user);

  useEffect(() => {
    if (user) {
      startAuthManager();
      return () => stopAuthManager();
    }
  }, [user]);

  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-gray-900 border-b border-gray-800">
      <div className="flex items-center justify-between px-6 py-3">
        {/* Left: LogSage wordmark */}
        <Link href="/" className="text-lg font-semibold">
          <span className="text-white">log</span>
          <span className="text-emerald-400">sage</span>
        </Link>

        {/* Right: User profile or Sign in */}
        <div>
          {user ? (
            <Link
              href="/dashboard"
              className="flex items-center justify-center w-8 h-8 rounded-full bg-emerald-600 hover:bg-emerald-500 text-white text-sm font-medium transition-colors"
              title={user.email}
            >
              {user.email.charAt(0).toUpperCase()}
            </Link>
          ) : (
            <Link
              href="/login"
              className="text-sm text-gray-400 hover:text-white transition-colors"
            >
              Sign in
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}
