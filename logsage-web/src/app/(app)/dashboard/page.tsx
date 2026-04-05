'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { authApi, analyzeApi } from '@/lib/api';
import { useAuthStore } from '@/lib/auth';
import type { SessionSummary } from '@/lib/types';
import { ProfileCard } from '@/components/dashboard/ProfileCard';
import { StatsCard } from '@/components/dashboard/StatsCard';
import { SessionHistoryTable } from '@/components/dashboard/SessionHistoryTable';

export default function DashboardPage() {
  const router = useRouter();

  // Use Zustand's persisted state
  const user = useAuthStore((s) => s.user);
  const isStoreLoading = useAuthStore((s) => s.isLoading);
  const tokenExpiry = useAuthStore((s) => s.tokenExpiry);
  const setUser = useAuthStore((s) => s.setUser);

  const [sessions, setSessions] = useState<SessionSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Wait for Zustand to finish rehydrating
    if (isStoreLoading) {
      return;
    }

    const token = localStorage.getItem('access_token');
    if (!token) {
      router.push('/login');
      return;
    }

    // Check if we need to fetch user data
    const isTokenExpired = tokenExpiry ? Date.now() > tokenExpiry : true;
    const needsUserFetch = !user || isTokenExpired;

    // Only fetch user if needed
    const userPromise = needsUserFetch
      ? authApi.me().then((res) => {
          setUser(res.data, tokenExpiry);
          return res.data;
        })
      : Promise.resolve(user);

    Promise.all([
      userPromise,
      analyzeApi.getSessions(),
    ])
      .then(([, sessionsRes]) => {
        setSessions(sessionsRes.data);
      })
      .catch(() => {
        setError('Failed to load dashboard data');
      })
      .finally(() => setLoading(false));
  }, [router, user, isStoreLoading, tokenExpiry, setUser]);

  if (isStoreLoading || loading) {
    return (
      <div className="min-h-screen bg-gray-950 flex items-center justify-center">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="min-h-screen bg-gray-950 flex items-center justify-center">
        <p className="text-red-400">{error || 'Failed to load user data'}</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950">
      <div className="max-w-7xl mx-auto p-6">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-semibold text-white">Dashboard</h1>
          <Link
            href="/analyze"
            className="text-sm bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-2 rounded-lg transition-colors"
          >
            New analysis
          </Link>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
          <div className="lg:col-span-1">
            <ProfileCard user={user} />
          </div>
          <div className="lg:col-span-2 space-y-6">
            <StatsCard sessions={sessions} />
            <SessionHistoryTable sessions={sessions} />
          </div>
        </div>
      </div>
    </div>
  );
}
