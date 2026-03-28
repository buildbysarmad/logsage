'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { analyzeApi } from '@/lib/api';
import type { SessionSummary } from '@/lib/types';

export default function SessionsPage() {
  const router = useRouter();
  const [sessions, setSessions] = useState<SessionSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('access_token');
    if (!token) { router.push('/login'); return; }

    analyzeApi.getSessions()
      .then(({ data }) => setSessions(data))
      .catch(() => setError('Could not load sessions'))
      .finally(() => setLoading(false));
  }, [router]);

  return (
    <div className="min-h-screen bg-gray-950 p-6">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-semibold text-white">Session history</h1>
          <Link href="/analyze"
            className="text-sm bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-2 rounded-lg">
            New analysis
          </Link>
        </div>

        {loading && <p className="text-gray-500">Loading sessions...</p>}
        {error && <p className="text-red-400">{error}</p>}
        {!loading && sessions.length === 0 && (
          <div className="text-center py-16">
            <p className="text-gray-500 mb-4">No sessions yet</p>
            <Link href="/analyze" className="text-emerald-400 hover:text-emerald-300 text-sm">
              Analyze your first log file
            </Link>
          </div>
        )}

        <div className="space-y-3">
          {sessions.map((session) => (
            <Link key={session.id} href={`/sessions/${session.id}`}
              className="block bg-gray-900 border border-gray-800 hover:border-gray-600
                         rounded-xl p-4 transition-colors">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <span className="text-xs bg-gray-800 text-gray-400 px-2 py-0.5 rounded">
                    {session.detectedFormat}
                  </span>
                  <span className="text-sm text-white">
                    {session.totalLines.toLocaleString()} lines
                  </span>
                  <span className="text-xs text-red-400">{session.errorCount} errors</span>
                  <span className="text-xs text-yellow-400">{session.warningCount} warnings</span>
                </div>
                <span className="text-xs text-gray-500">
                  {new Date(session.createdAt).toLocaleDateString()}
                </span>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
