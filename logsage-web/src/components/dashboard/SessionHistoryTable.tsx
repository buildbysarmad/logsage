'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import type { SessionSummary } from '@/lib/types';

interface Props {
  sessions: SessionSummary[];
}

export function SessionHistoryTable({ sessions }: Props) {
  const router = useRouter();
  const [page, setPage] = useState(0);
  const pageSize = 20;

  const paginatedSessions = sessions.slice(page * pageSize, (page + 1) * pageSize);
  const totalPages = Math.ceil(sessions.length / pageSize);

  const handleView = (sessionId: string) => {
    // TODO: Load session in /analyze page
    router.push(`/analyze?session=${sessionId}`);
  };

  if (sessions.length === 0) {
    return (
      <div className="bg-gray-900 border border-gray-800 rounded-xl p-6">
        <h2 className="text-lg font-semibold text-white mb-4">Session history</h2>
        <p className="text-sm text-gray-500 text-center py-8">No sessions yet</p>
      </div>
    );
  }

  return (
    <div className="bg-gray-900 border border-gray-800 rounded-xl p-6">
      <h2 className="text-lg font-semibold text-white mb-4">Session history</h2>

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-800 text-gray-500 text-xs">
              <th className="text-left py-2 px-3 font-medium">Date</th>
              <th className="text-left py-2 px-3 font-medium">Format</th>
              <th className="text-right py-2 px-3 font-medium">Lines</th>
              <th className="text-right py-2 px-3 font-medium">Errors</th>
              <th className="text-center py-2 px-3 font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {paginatedSessions.map((session) => (
              <tr key={session.id} className="border-b border-gray-800 hover:bg-gray-800/50">
                <td className="py-2 px-3 text-gray-300">
                  {new Date(session.createdAt).toLocaleDateString('en-US', {
                    month: 'short',
                    day: 'numeric',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </td>
                <td className="py-2 px-3">
                  <span className="inline-block bg-gray-800 text-gray-400 px-2 py-0.5 rounded text-xs">
                    {session.detectedFormat}
                  </span>
                </td>
                <td className="py-2 px-3 text-right text-gray-300">
                  {session.totalLines.toLocaleString()}
                </td>
                <td className="py-2 px-3 text-right">
                  <span className="text-red-400">{session.errorCount}</span>
                  {session.warningCount > 0 && (
                    <span className="text-yellow-400 ml-2">+{session.warningCount}w</span>
                  )}
                </td>
                <td className="py-2 px-3 text-center">
                  <button
                    onClick={() => handleView(session.id)}
                    className="text-emerald-400 hover:text-emerald-300 text-xs font-medium"
                  >
                    View
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-800">
          <button
            onClick={() => setPage((p) => Math.max(0, p - 1))}
            disabled={page === 0}
            className="text-xs text-gray-400 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Previous
          </button>
          <span className="text-xs text-gray-500">
            Page {page + 1} of {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages - 1, p + 1))}
            disabled={page === totalPages - 1}
            className="text-xs text-gray-400 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
