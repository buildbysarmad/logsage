'use client';

import { motion } from 'framer-motion';
import type { ErrorGroup } from '@/lib/types';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

const levelDot: Record<string, string> = {
  Fatal: 'bg-red-500',
  Error: 'bg-red-400',
  Warning: 'bg-yellow-400',
  Info: 'bg-blue-400',
  Debug: 'bg-gray-400',
  Trace: 'bg-gray-600',
};

const levelBadge: Record<string, string> = {
  Fatal: 'bg-red-900/80 text-red-300 border border-red-700/50',
  Error: 'bg-red-900/80 text-red-300 border border-red-700/50',
  Warning: 'bg-yellow-900/80 text-yellow-300 border border-yellow-700/50',
  Info: 'bg-blue-900/80 text-blue-300 border border-blue-700/50',
  Debug: 'bg-gray-800/80 text-gray-400 border border-gray-700/50',
  Trace: 'bg-gray-800/80 text-gray-500 border border-gray-700/50',
};

const levelGlow: Record<string, string> = {
  Fatal: '0 0 8px rgba(239, 68, 68, 0.4)',
  Error: '0 0 8px rgba(248, 113, 113, 0.4)',
  Warning: '0 0 8px rgba(251, 191, 36, 0.4)',
  Info: '0 0 8px rgba(59, 130, 246, 0.4)',
  Debug: '0 0 8px rgba(156, 163, 175, 0.3)',
  Trace: '0 0 8px rgba(107, 114, 128, 0.3)',
};

/**
 * Extracts a concise keyword/title from error group for sidebar display
 */
function extractKeywords(group: ErrorGroup): string {
  // Priority 1: Use exception type if available (e.g., "SqlException", "NullReferenceException")
  if (group.exceptionType) {
    return group.exceptionType;
  }

  // Priority 2: Extract key phrases from representative message
  const message = group.representativeMessage;

  // Pattern 1: Extract exception name from message like "System.NullReferenceException: ..."
  const exceptionMatch = message.match(/([A-Z][a-z]+(?:[A-Z][a-z]+)*Exception)/);
  if (exceptionMatch) {
    return exceptionMatch[1];
  }

  // Pattern 2: Extract key action phrases (e.g., "Failed to connect", "Unable to process")
  const actionMatch = message.match(/^(Failed to|Unable to|Could not|Cannot|Error:|Unhandled exception in)\s+(.{1,30})/i);
  if (actionMatch) {
    const phrase = (actionMatch[1] + ' ' + actionMatch[2]).trim();
    return phrase.length > 40 ? phrase.substring(0, 37) + '...' : phrase;
  }

  // Pattern 3: Extract first meaningful part before colon or newline
  const colonSplit = message.split(/[:\n]/)[0];
  if (colonSplit && colonSplit.length > 5) {
    return colonSplit.length > 40 ? colonSplit.substring(0, 37) + '...' : colonSplit;
  }

  // Pattern 4: Take first N words as fallback
  const words = message.split(/\s+/).filter(w => w.length > 0);
  const firstWords = words.slice(0, 5).join(' ');
  return firstWords.length > 40 ? firstWords.substring(0, 37) + '...' : firstWords;
}

/**
 * Extracts a short subtitle for additional context
 */
function extractSubtitle(group: ErrorGroup): string | null {
  // Show source if available and different from exception type
  if (group.source && group.source !== group.exceptionType) {
    return group.source.length > 25 ? group.source.substring(0, 22) + '...' : group.source;
  }
  return null;
}

interface Props {
  groups: ErrorGroup[];
  selected: ErrorGroup | null;
  onSelect: (g: ErrorGroup) => void;
}

export default function ErrorGroupSidebar({ groups, selected, onSelect }: Props) {
  const shouldReduceMotion = useReducedMotion();

  if (groups.length === 0) {
    return (
      <aside className="w-64 shrink-0 border-r border-gray-800 bg-gray-900 flex items-center justify-center">
        <p className="text-xs text-gray-500 text-center px-4">
          No errors or warnings found in this log file.
        </p>
      </aside>
    );
  }

  return (
    <aside className="w-64 shrink-0 border-r border-gray-800 bg-gray-900 flex flex-col overflow-hidden">
      <motion.div
        className="px-3 py-2.5 border-b border-gray-800 shrink-0 bg-gray-950/50"
        initial={shouldReduceMotion ? undefined : { opacity: 0, y: -10 }}
        animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
        transition={motionTransitions.smooth}
      >
        <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
          {groups.length} error group{groups.length !== 1 ? 's' : ''}
        </p>
      </motion.div>

      <div className="flex-1 overflow-y-auto scrollbar-thin min-h-0">
        {groups.map((group, index) => {
          const keywords = extractKeywords(group);
          const subtitle = extractSubtitle(group);
          const isSelected = selected?.groupKey === group.groupKey;

          return (
            <motion.button
              key={group.groupKey}
              onClick={() => onSelect(group)}
              className={`w-full text-left px-3 py-3 border-b border-gray-800
                          transition-all duration-200 hover:bg-gray-800 ${
                            isSelected
                              ? 'bg-emerald-950/30 border-l-2 border-l-emerald-500'
                              : ''
                          }`}
              initial={shouldReduceMotion ? undefined : { opacity: 0, x: -20 }}
              animate={shouldReduceMotion ? undefined : { opacity: 1, x: 0 }}
              transition={{ delay: index * 0.03, ...motionTransitions.smooth }}
              whileHover={shouldReduceMotion ? {} : { x: 2 }}
            >
              <div className="flex items-start gap-2">
                {/* Level indicator dot with glow */}
                <motion.span
                  className={`mt-1 w-2 h-2 rounded-full shrink-0 ${
                    levelDot[group.level] ?? 'bg-gray-500'
                  }`}
                  animate={shouldReduceMotion || !isSelected ? {} : {
                    boxShadow: [
                      '0 0 0px rgba(52, 211, 153, 0)',
                      '0 0 8px rgba(52, 211, 153, 0.4)',
                      '0 0 0px rgba(52, 211, 153, 0)',
                    ],
                  }}
                  transition={{ duration: 2, repeat: Infinity }}
                />

                <div className="flex-1 min-w-0">
                  {/* Keywords/Title */}
                  <p className="text-xs font-medium text-gray-200 leading-snug mb-1.5">
                    {keywords}
                  </p>

                  {/* Subtitle (source) if available */}
                  {subtitle && (
                    <p className="text-xs text-gray-500 truncate mb-1.5">
                      {subtitle}
                    </p>
                  )}

                  {/* Level badge for all levels */}
                  <motion.span
                    className={`inline-block text-[10px] font-medium px-1.5 py-0.5 rounded ${
                      levelBadge[group.level] ?? 'bg-gray-800 text-gray-400'
                    }`}
                    whileHover={shouldReduceMotion ? {} : {
                      scale: 1.05,
                      boxShadow: levelGlow[group.level] ?? '0 0 8px rgba(156, 163, 175, 0.3)',
                    }}
                  >
                    {group.level}
                  </motion.span>
                </div>

                {/* Count badge */}
                <motion.span
                  className={`text-xs font-semibold px-1.5 py-0.5 rounded shrink-0 ${
                    levelBadge[group.level] ?? 'bg-gray-800 text-gray-400'
                  }`}
                  whileHover={shouldReduceMotion ? {} : { scale: 1.1 }}
                >
                  {group.count}
                </motion.span>
              </div>
            </motion.button>
          );
        })}
      </div>
    </aside>
  );
}
