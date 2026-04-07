'use client';

import { useState, useEffect, Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { motion } from 'framer-motion';
import axios from 'axios';
import { authApi } from '@/lib/api';
import { useAuthStore } from '@/lib/auth';
import { getTokenExpiry } from '@/lib/jwt';
import { AnimatedBackground } from '@/components/motion/AnimatedBackground';
import { useReducedMotion, motionVariants, motionTransitions } from '@/lib/motion';

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const setUser = useAuthStore((s) => s.setUser);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sessionExpired, setSessionExpired] = useState(false);
  const shouldReduceMotion = useReducedMotion();

  useEffect(() => {
    if (searchParams?.get('reason') === 'expired') {
      setSessionExpired(true);
    }
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      const { data: tokens } = await authApi.login(email, password);
      localStorage.setItem('access_token', tokens.accessToken);
      localStorage.setItem('refresh_token', tokens.refreshToken);

      // Parse token expiry
      const tokenExpiry = getTokenExpiry(tokens.accessToken);

      const { data: user } = await authApi.me();
      setUser(user, tokenExpiry);
      router.push('/analyze');
    } catch (err: unknown) {
      // Check if it's an HTTP error response
      if (axios.isAxiosError(err) && err.response) {
        const status = err.response.status;

        if (status === 401) {
          setError('Incorrect email or password.');
        } else if (status === 429) {
          setError('Too many attempts. Please wait a moment and try again.');
        } else if (status === 500) {
          setError('Server error. Please try again in a moment.');
        } else {
          setError('Something went wrong. Please try again.');
        }
      } else if (axios.isAxiosError(err) && err.request) {
        // Network error - request was made but no response received
        setError('Could not connect. Please check your connection and try again.');
      } else {
        // Something else went wrong
        setError('Something went wrong. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4 relative">
      <AnimatedBackground />

      <motion.div
        className="w-full max-w-sm relative z-10"
        initial={shouldReduceMotion ? false : motionVariants.scaleUp.initial}
        animate={shouldReduceMotion ? false : motionVariants.scaleUp.animate}
        transition={motionTransitions.springy}
      >
        <motion.div
          className="text-center mb-8"
          initial={shouldReduceMotion ? false : { opacity: 0, y: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
          transition={{ delay: 0.1, ...motionTransitions.smooth }}
        >
          <h1 className="text-2xl font-semibold text-white">
            Log<span className="text-emerald-400">Sage</span>
          </h1>
          <p className="text-gray-400 text-sm mt-2">Sign in to your account</p>
        </motion.div>

        {sessionExpired && (
          <motion.div
            className="bg-yellow-900/20 border border-yellow-700/50 rounded-lg px-4 py-3 mb-4"
            initial={shouldReduceMotion ? false : { opacity: 0, x: -20 }}
            animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
            transition={motionTransitions.smooth}
          >
            <p className="text-yellow-400 text-sm">
              Your session expired. Please sign in again.
            </p>
          </motion.div>
        )}

        <motion.div
          className="bg-gray-900/80 backdrop-blur-sm border border-gray-800 rounded-xl p-6"
          initial={shouldReduceMotion ? false : { opacity: 0 }}
          animate={shouldReduceMotion ? false : { opacity: 1 }}
          transition={{ delay: 0.2, ...motionTransitions.smooth }}
        >
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm text-gray-400 mb-1.5">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg px-3 py-2.5
                           text-white text-sm focus:outline-none focus:border-emerald-500
                           transition-all duration-300 focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
              />
            </div>
            <div>
              <label className="block text-sm text-gray-400 mb-1.5">Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full bg-gray-900 border border-gray-700 rounded-lg px-3 py-2.5
                           text-white text-sm focus:outline-none focus:border-emerald-500
                           transition-all duration-300 focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
              />
            </div>

            {error && (
              <motion.p
                className="text-red-400 text-sm"
                initial={shouldReduceMotion ? false : { opacity: 0, x: -10 }}
                animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
                transition={motionTransitions.smooth}
              >
                {error}
              </motion.p>
            )}

            <motion.button
              type="submit"
              disabled={loading}
              className="relative w-full bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50
                         text-white font-medium py-2.5 rounded-lg transition-all duration-300
                         overflow-hidden group"
              whileHover={shouldReduceMotion ? {} : { scale: 1.02 }}
              whileTap={shouldReduceMotion ? {} : { scale: 0.98 }}
            >
              <span className="relative z-10">
                {loading ? 'Signing in...' : 'Sign in'}
              </span>
              <motion.div
                className="absolute inset-0 bg-gradient-to-r from-emerald-600 via-emerald-400 to-emerald-600"
                initial={{ x: '-100%' }}
                animate={loading ? { x: '100%' } : {}}
                transition={{ repeat: loading ? Infinity : 0, duration: 1, ease: 'linear' }}
              />
            </motion.button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            No account?{' '}
            <Link href="/register" className="text-emerald-400 hover:text-emerald-300 transition-colors">
              Create one free
            </Link>
          </p>
        </motion.div>
      </motion.div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="min-h-screen flex items-center justify-center"><p className="text-gray-500">Loading...</p></div>}>
      <LoginForm />
    </Suspense>
  );
}
