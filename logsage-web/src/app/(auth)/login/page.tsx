'use client';

import { useState, useEffect } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { authApi } from '@/lib/api';
import { useAuthStore } from '@/lib/auth';
import { getTokenExpiry } from '@/lib/jwt';

export default function LoginPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const setUser = useAuthStore((s) => s.setUser);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sessionExpired, setSessionExpired] = useState(false);

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
    } catch (err: any) {
      // Check if it's an HTTP error response
      if (err?.response) {
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
      } else if (err?.request) {
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
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <h1 className="text-2xl font-semibold text-white">
            Log<span className="text-emerald-400">Sage</span>
          </h1>
          <p className="text-gray-400 text-sm mt-2">Sign in to your account</p>
        </div>

        {sessionExpired && (
          <div className="bg-yellow-900/20 border border-yellow-700/50 rounded-lg px-4 py-3 mb-4">
            <p className="text-yellow-400 text-sm">
              Your session expired. Please sign in again.
            </p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm text-gray-400 mb-1.5">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full bg-gray-900 border border-gray-700 rounded-lg px-3 py-2.5
                         text-white text-sm focus:outline-none focus:border-emerald-500"
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
                         text-white text-sm focus:outline-none focus:border-emerald-500"
            />
          </div>
          {error && <p className="text-red-400 text-sm">{error}</p>}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50
                       text-white font-medium py-2.5 rounded-lg transition-colors"
          >
            {loading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>
        <p className="text-center text-sm text-gray-500 mt-6">
          No account?{' '}
          <Link href="/register" className="text-emerald-400 hover:text-emerald-300">
            Create one free
          </Link>
        </p>
      </div>
    </div>
  );
}
