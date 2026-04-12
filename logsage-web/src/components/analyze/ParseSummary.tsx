'use client';

import { motion } from 'framer-motion';
import type { AnalysisResult } from '@/lib/types';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

interface Props {
  result: AnalysisResult;
}

export default function ParseSummary({ result }: Props) {
  const shouldReduceMotion = useReducedMotion();

  return (
    <motion.div
      className="bg-gray-900 border border-gray-800 rounded-lg p-4 mb-4"
      initial={shouldReduceMotion ? undefined : { opacity: 0, y: -10 }}
      animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
      transition={{ delay: 0.1, ...motionTransitions.smooth }}
    >
      <div className="flex items-center gap-2 mb-3">
        <h3 className="text-sm font-semibold text-white">Parse Summary</h3>
        <span className="text-xs px-2 py-0.5 bg-emerald-600 text-white rounded">
          {result.detectedFormat}
        </span>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-xs">
        <div>
          <div className="text-gray-400">Total Lines</div>
          <div className="text-white font-semibold">{result.totalLines.toLocaleString()}</div>
        </div>

        <div>
          <div className="text-gray-400">Parsed Entries</div>
          <div className="text-white font-semibold">{result.parsedEntries.toLocaleString()}</div>
        </div>

        {result.errorCount > 0 && (
          <div>
            <div className="text-red-400">Errors</div>
            <div className="text-red-300 font-semibold">{result.errorCount}</div>
          </div>
        )}

        {result.warningCount > 0 && (
          <div>
            <div className="text-yellow-400">Warnings</div>
            <div className="text-yellow-300 font-semibold">{result.warningCount}</div>
          </div>
        )}

        {result.infoCount > 0 && (
          <div>
            <div className="text-blue-400">Info</div>
            <div className="text-blue-300 font-semibold">{result.infoCount}</div>
          </div>
        )}

        {result.debugCount > 0 && (
          <div>
            <div className="text-gray-400">Debug</div>
            <div className="text-gray-300 font-semibold">{result.debugCount}</div>
          </div>
        )}

        {result.parseErrorCount > 0 && (
          <div className="col-span-2 sm:col-span-1">
            <div className="text-red-400">Parse Errors</div>
            <div className="text-red-300 font-semibold">{result.parseErrorCount}</div>
          </div>
        )}

        {result.logDuration && (
          <div>
            <div className="text-gray-400">Log Duration</div>
            <div className="text-white font-semibold">{result.logDuration}</div>
          </div>
        )}
      </div>
    </motion.div>
  );
}
