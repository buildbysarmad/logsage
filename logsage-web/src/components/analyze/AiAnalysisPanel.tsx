'use client';

import { useState } from 'react';
import type { AiGroupAnalysis, ErrorGroup } from '@/lib/types';

const severityColor: Record<string, string> = {
  CRITICAL: 'bg-red-900 text-red-300 border-red-700',
  HIGH: 'bg-orange-900 text-orange-300 border-orange-700',
  MEDIUM: 'bg-yellow-900 text-yellow-300 border-yellow-700',
  LOW: 'bg-green-900 text-green-300 border-green-700',
};

interface Props {
  analysis: AiGroupAnalysis | null;
  group: ErrorGroup | null;
}

export default function AiAnalysisPanel({ analysis, group }: Props) {
  const [copied, setCopied] = useState(false);

  // Hide AI panel when AI feature is disabled
  if (process.env.NEXT_PUBLIC_AI_ENABLED !== 'true') {
    return null;
  }

  const copyFix = async () => {
    if (!analysis?.suggestedFix) return;
    await navigator.clipboard.writeText(analysis.suggestedFix);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  if (!group) {
    return (
      <aside className="w-72 shrink-0 bg-gray-900 flex items-center justify-center">
        <p className="text-xs text-gray-600 text-center px-4">
          Select an error group to see AI analysis
        </p>
      </aside>
    );
  }

  return (
    <aside className="w-72 shrink-0 bg-gray-900 flex flex-col overflow-y-auto">
      <div className="px-4 py-3 border-b border-gray-800 flex items-center justify-between shrink-0">
        <span className="text-sm font-medium text-white">AI analysis</span>
        <span className="text-xs px-2 py-0.5 rounded-full bg-emerald-900 text-emerald-400">
          Claude
        </span>
      </div>

      {!analysis ? (
        <div className="flex-1 flex flex-col items-center justify-center p-6 text-center">
          <p className="text-sm text-gray-500 mb-3">
            AI analysis available on Pro plan
          </p>
          <a
            href="/pricing"
            className="text-xs bg-emerald-600 hover:bg-emerald-500 text-white px-4 py-2 rounded-lg transition-colors"
          >
            Upgrade to Pro
          </a>
        </div>
      ) : (
        <div className="flex-1 p-4 space-y-4">
          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
              Severity
            </p>
            <span className={`inline-flex items-center px-2.5 py-1 rounded border text-xs font-semibold ${
              severityColor[analysis.severity] ?? severityColor.MEDIUM
            }`}>
              {analysis.severity}
            </span>
          </div>

          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
              Root cause
            </p>
            <p className="text-xs text-gray-300 leading-relaxed bg-gray-800 rounded-lg p-3">
              {analysis.rootCause}
            </p>
          </div>

          <div>
            <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-2">
              Suggested fix
            </p>
            <pre className="text-xs text-blue-300 leading-relaxed bg-blue-950 border border-blue-900 rounded-lg p-3 whitespace-pre-wrap font-mono">
              {analysis.suggestedFix}
            </pre>
            <button
              onClick={copyFix}
              className="mt-2 w-full text-xs border border-gray-700 hover:border-gray-500 text-gray-400 hover:text-white py-1.5 rounded-lg transition-colors"
            >
              {copied ? '✓ Copied' : 'Copy fix'}
            </button>
          </div>
        </div>
      )}
    </aside>
  );
}
