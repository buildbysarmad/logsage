'use client';

import type { SessionSummary } from '@/lib/types';

interface Props {
  sessions: SessionSummary[];
}

export function StatsCard({ sessions }: Props) {
  const totalSessions = sessions.length;
  const totalLines = sessions.reduce((sum, s) => sum + s.totalLines, 0);

  const formatCounts = sessions.reduce((acc, s) => {
    acc[s.detectedFormat] = (acc[s.detectedFormat] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const mostCommonFormat = Object.entries(formatCounts)
    .sort((a, b) => b[1] - a[1])[0]?.[0] || 'N/A';

  // Most common error type - for now we'll just show total errors
  const totalErrors = sessions.reduce((sum, s) => sum + s.errorCount, 0);

  return (
    <div className="bg-gray-900 border border-gray-800 rounded-xl p-6">
      <h2 className="text-lg font-semibold text-white mb-6">Usage statistics</h2>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <p className="text-xs text-gray-500 mb-1">Total sessions</p>
          <p className="text-2xl font-semibold text-white">{totalSessions.toLocaleString()}</p>
        </div>

        <div>
          <p className="text-xs text-gray-500 mb-1">Lines analyzed</p>
          <p className="text-2xl font-semibold text-white">{totalLines.toLocaleString()}</p>
        </div>

        <div>
          <p className="text-xs text-gray-500 mb-1">Most common format</p>
          <p className="text-sm font-medium text-white">{mostCommonFormat}</p>
        </div>

        <div>
          <p className="text-xs text-gray-500 mb-1">Total errors found</p>
          <p className="text-sm font-medium text-red-400">{totalErrors.toLocaleString()}</p>
        </div>
      </div>
    </div>
  );
}
