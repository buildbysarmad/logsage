'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { adminApi, authApi } from '@/lib/api';
import type { AdminAnalytics } from '@/lib/types';
import { useAuthStore } from '@/lib/auth';

export default function AdminPage() {
  const router = useRouter();
  const { user } = useAuthStore();
  const [days, setDays] = useState(30);
  const [analytics, setAnalytics] = useState<AdminAnalytics | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [checkingAuth, setCheckingAuth] = useState(true);

  // Check if user is authenticated and is admin
  useEffect(() => {
    const checkAdmin = async () => {
      try {
        if (!user) {
          const response = await authApi.me();
          if (!response.data.isAdmin) {
            router.push('/dashboard');
            return;
          }
        } else if (!user.isAdmin) {
          router.push('/dashboard');
          return;
        }
        setCheckingAuth(false);
      } catch (err) {
        router.push('/login?redirect=/admin');
      }
    };

    checkAdmin();
  }, [user, router]);

  const handleFetchAnalytics = async () => {
    setLoading(true);
    setError('');

    try {
      const response = await adminApi.getAnalytics(days);
      setAnalytics(response.data);
    } catch (err: any) {
      if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Unauthorized. Admin access required.');
        router.push('/dashboard');
      } else {
        setError('Failed to fetch analytics. Please try again.');
      }
      setAnalytics(null);
    } finally {
      setLoading(false);
    }
  };

  // Auto-fetch on mount
  useEffect(() => {
    if (!checkingAuth) {
      handleFetchAnalytics();
    }
  }, [checkingAuth]);

  if (checkingAuth) {
    return (
      <div className="min-h-screen bg-gray-950 text-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-emerald-400 mx-auto mb-4"></div>
          <p className="text-gray-400">Verifying admin access...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 text-gray-100 py-8">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold">Admin Analytics Dashboard</h1>
          <div className="flex gap-4 items-center">
            <select
              id="days"
              value={days}
              onChange={(e) => setDays(Number(e.target.value))}
              className="px-4 py-2 bg-gray-800 border border-gray-700 rounded focus:outline-none focus:border-emerald-400"
            >
              <option value={7}>Last 7 days</option>
              <option value={30}>Last 30 days</option>
              <option value={90}>Last 90 days</option>
              <option value={365}>Last year</option>
            </select>
            <button
              onClick={handleFetchAnalytics}
              disabled={loading}
              className="px-6 py-2 bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-700 disabled:cursor-not-allowed rounded font-medium transition"
            >
              {loading ? 'Loading...' : 'Refresh'}
            </button>
          </div>
        </div>

        {error && (
          <div className="bg-red-900/20 border border-red-800 rounded-lg p-4 mb-6">
            <div className="text-red-400">{error}</div>
          </div>
        )}

        {loading && !analytics && (
          <div className="flex items-center justify-center py-20">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-emerald-400 mx-auto mb-4"></div>
              <p className="text-gray-400">Loading analytics...</p>
            </div>
          </div>
        )}

        {/* Analytics Display */}
        {analytics && (
          <>
            {/* Overview Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-6">
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Total Sessions</div>
                <div className="text-2xl font-bold text-emerald-400">
                  {analytics.overview.totalSessions.toLocaleString()}
                </div>
              </div>
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Avg Success Rate</div>
                <div className="text-2xl font-bold text-blue-400">
                  {analytics.overview.averageSuccessRate.toFixed(1)}%
                </div>
              </div>
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Total Errors</div>
                <div className="text-2xl font-bold text-red-400">
                  {analytics.overview.totalErrorsDetected.toLocaleString()}
                </div>
              </div>
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Formats Detected</div>
                <div className="text-2xl font-bold text-purple-400">
                  {analytics.overview.uniqueFormatsDetected}
                </div>
              </div>
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Bytes Processed</div>
                <div className="text-2xl font-bold text-yellow-400">
                  {(analytics.overview.totalBytesProcessed / 1024 / 1024).toFixed(1)} MB
                </div>
              </div>
              <div className="bg-gray-900 rounded-lg p-4">
                <div className="text-gray-400 text-sm">Avg Duration</div>
                <div className="text-2xl font-bold text-cyan-400">
                  {analytics.overview.averageDurationMs} ms
                </div>
              </div>
            </div>

            {/* Outcome Breakdown */}
            <div className="bg-gray-900 rounded-lg p-6 mb-6">
              <h2 className="text-xl font-bold mb-4">Outcome Breakdown</h2>
              <div className="space-y-3">
                {analytics.outcomeBreakdown.map((outcome) => (
                  <div key={outcome.outcome} className="flex items-center gap-4">
                    <div className="w-32 font-medium">{outcome.outcome}</div>
                    <div className="flex-1">
                      <div className="h-8 bg-gray-800 rounded overflow-hidden">
                        <div
                          className={`h-full flex items-center px-3 text-sm ${
                            outcome.outcome === 'Success'
                              ? 'bg-emerald-600'
                              : outcome.outcome === 'PartialSuccess'
                              ? 'bg-yellow-600'
                              : 'bg-red-600'
                          }`}
                          style={{ width: `${outcome.percentage}%` }}
                        >
                          {outcome.count.toLocaleString()} ({outcome.percentage.toFixed(1)}%)
                        </div>
                      </div>
                    </div>
                    <div className="w-24 text-right text-gray-400 text-sm">
                      {outcome.averageSuccessRate.toFixed(1)}% avg
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Format Breakdown */}
            <div className="bg-gray-900 rounded-lg p-6 mb-6">
              <h2 className="text-xl font-bold mb-4">Format Breakdown</h2>
              <div className="space-y-3">
                {analytics.formatBreakdown.slice(0, 10).map((format) => (
                  <div key={format.format} className="flex items-center gap-4">
                    <div className="w-48 font-medium truncate" title={format.format}>
                      {format.format}
                    </div>
                    <div className="flex-1">
                      <div className="h-8 bg-gray-800 rounded overflow-hidden">
                        <div
                          className="h-full bg-purple-600 flex items-center px-3 text-sm"
                          style={{ width: `${format.percentage}%` }}
                        >
                          {format.count.toLocaleString()} ({format.percentage.toFixed(1)}%)
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Daily Trend */}
            <div className="bg-gray-900 rounded-lg p-6">
              <h2 className="text-xl font-bold mb-4">Daily Trend</h2>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="text-gray-400 border-b border-gray-800">
                    <tr>
                      <th className="text-left py-2 px-3">Date</th>
                      <th className="text-right py-2 px-3">Sessions</th>
                      <th className="text-right py-2 px-3">Success Rate</th>
                      <th className="text-right py-2 px-3">Total Errors</th>
                    </tr>
                  </thead>
                  <tbody>
                    {analytics.dailyTrend.slice(-14).reverse().map((day) => (
                      <tr key={day.date} className="border-b border-gray-800 hover:bg-gray-800">
                        <td className="py-2 px-3">{day.date}</td>
                        <td className="text-right py-2 px-3">{day.sessionCount.toLocaleString()}</td>
                        <td className="text-right py-2 px-3">
                          <span
                            className={
                              day.averageSuccessRate > 80
                                ? 'text-emerald-400'
                                : day.averageSuccessRate > 50
                                ? 'text-yellow-400'
                                : 'text-red-400'
                            }
                          >
                            {day.averageSuccessRate.toFixed(1)}%
                          </span>
                        </td>
                        <td className="text-right py-2 px-3">{day.totalErrors.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
