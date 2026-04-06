'use client';

import { motion } from 'framer-motion';
import type { ErrorGroup } from '@/lib/types';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

const lineClass: Record<string, string> = {
  Fatal: 'border-red-500 bg-red-950 text-red-200',
  Error: 'border-red-400 bg-red-950 text-red-200',
  Warning: 'border-yellow-400 bg-yellow-950 text-yellow-200',
  Info: 'border-blue-400 bg-blue-950 text-blue-200',
  Debug: 'border-gray-600 bg-gray-900 text-gray-400',
  Trace: 'border-gray-700 bg-gray-900 text-gray-500',
};

interface Props {
  group: ErrorGroup | null;
}

export default function LogDetailPanel({ group }: Props) {
  const shouldReduceMotion = useReducedMotion();

  if (!group) {
    return (
      <motion.div
        className="flex-1 bg-gray-900 flex items-center justify-center border-r border-gray-800"
        initial={shouldReduceMotion ? undefined : { opacity: 0, y: 20 }}
        animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
        transition={{ delay: 0.3, ...motionTransitions.smooth }}
      >
        <p className="text-sm text-gray-600">Select an error group to view log lines</p>
      </motion.div>
    );
  }

  return (
    <motion.div
      className="flex-1 flex flex-col overflow-hidden border-r border-gray-800 bg-gray-950"
      initial={shouldReduceMotion ? undefined : { opacity: 0, y: 20 }}
      animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
      transition={{ delay: 0.3, ...motionTransitions.smooth }}
      key={group.groupKey}
    >
      <motion.div
        className="px-4 py-3 border-b border-gray-800 bg-gray-900 shrink-0"
        initial={shouldReduceMotion ? undefined : { opacity: 0, y: -10 }}
        animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
        transition={{ delay: 0.4, ...motionTransitions.smooth }}
      >
        <p className="text-sm font-medium text-white">
          {group.exceptionType ?? group.representativeMessage}
        </p>
        <div className="flex gap-4 mt-1 text-xs text-gray-400">
          <span>{group.count} occurrences</span>
          {group.firstSeen && <span>First: {group.firstSeen.slice(11, 19)}</span>}
          {group.lastSeen && <span>Last: {group.lastSeen.slice(11, 19)}</span>}
          {group.source && <span>{group.source}</span>}
        </div>
      </motion.div>

      <div className="flex-1 overflow-y-auto p-4 space-y-1.5 scrollbar-thin min-h-0">
        {group.entries.map((entry, i) => (
          <motion.div
            key={i}
            className={`font-mono text-xs p-2.5 rounded border-l-2 leading-relaxed ${
              lineClass[entry.level] ?? 'border-gray-600 bg-gray-900 text-gray-400'
            }`}
            initial={shouldReduceMotion ? undefined : { opacity: 0, x: -10 }}
            animate={shouldReduceMotion ? undefined : { opacity: 1, x: 0 }}
            transition={{ delay: 0.5 + i * 0.02, ...motionTransitions.smooth }}
          >
            {entry.timestamp && (
              <span className="opacity-60 mr-2">{entry.timestamp.slice(11, 19)}</span>
            )}
            <span className="font-semibold mr-2">{entry.level.toUpperCase()}</span>
            <span>{entry.message}</span>
            {entry.stackTrace && (
              <pre className="mt-1.5 text-xs opacity-60 whitespace-pre-wrap overflow-x-auto scrollbar-thin">
                {entry.stackTrace}
              </pre>
            )}
          </motion.div>
        ))}
      </div>
    </motion.div>
  );
}
