'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { authApi, analyzeApi } from '@/lib/api';
import { useAuthStore } from '@/lib/auth';
import type { SessionSummary } from '@/lib/types';
import { ProfileCard } from '@/components/dashboard/ProfileCard';
import { StatsCard } from '@/components/dashboard/StatsCard';
import { SessionHistoryTable } from '@/components/dashboard/SessionHistoryTable';
import { AnimatedBackground } from '@/components/motion/AnimatedBackground';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

export default function DashboardPage() {
  const router = useRouter();
  const shouldReduceMotion = useReducedMotion();

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
      .catch((err) => {
        // Network error or auth error - interceptor will handle redirect
        // This catch should rarely execute since interceptor redirects
        console.error('[Dashboard] Failed to load data:', err);

        // If we still got here, something went wrong - force logout
        const { logout } = useAuthStore.getState();
        logout();
        router.push('/login?reason=error');
      })
      .finally(() => setLoading(false));
  }, [router, user, isStoreLoading, tokenExpiry, setUser]);

  if (isStoreLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center relative">
        <AnimatedBackground />
        <motion.div
          className="relative z-10"
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={motionTransitions.springy}
        >
          <motion.div
            className="w-16 h-16 border-4 border-emerald-400 border-t-transparent rounded-full"
            animate={shouldReduceMotion ? {} : { rotate: 360 }}
            transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
          />
          <p className="text-gray-500 mt-4 text-center">Loading...</p>
        </motion.div>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center relative">
        <AnimatedBackground />
        <motion.p
          className="text-red-400 relative z-10"
          initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
          transition={motionTransitions.smooth}
        >
          {error || 'Failed to load user data'}
        </motion.p>
      </div>
    );
  }

  return (
    <div className="min-h-screen relative">
      <AnimatedBackground />

      <div className="max-w-7xl mx-auto p-6 relative z-10">
        <motion.div
          className="flex items-center justify-between mb-8"
          initial={shouldReduceMotion ? false : { opacity: 0, y: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
          transition={motionTransitions.smooth}
        >
          <h1 className="text-2xl font-semibold text-white">Dashboard</h1>
          <motion.div whileHover={shouldReduceMotion ? {} : { scale: 1.05 }} whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}>
            <Link
              href="/analyze"
              className="text-sm bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-2 rounded-lg transition-colors"
            >
              New analysis
            </Link>
          </motion.div>
        </motion.div>

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
