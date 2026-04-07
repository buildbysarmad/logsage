'use client';

import { useEffect, useRef } from 'react';
import { motion, useMotionValue, useSpring, useInView } from 'framer-motion';
import type { SessionSummary } from '@/lib/types';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

interface Props {
  sessions: SessionSummary[];
}

function AnimatedNumber({ value, shouldReduceMotion }: { value: number; shouldReduceMotion: boolean }) {
  const ref = useRef<HTMLSpanElement>(null);
  const isInView = useInView(ref, { once: true });
  const motionValue = useMotionValue(0);
  const springValue = useSpring(motionValue, { duration: 2000, bounce: 0 });

  useEffect(() => {
    if (isInView && !shouldReduceMotion) {
      motionValue.set(value);
    }
  }, [isInView, value, motionValue, shouldReduceMotion]);

  useEffect(() => {
    if (shouldReduceMotion) return;

    const unsubscribe = springValue.on('change', (latest) => {
      if (ref.current) {
        ref.current.textContent = Math.floor(latest).toLocaleString();
      }
    });
    return unsubscribe;
  }, [springValue, shouldReduceMotion]);

  if (shouldReduceMotion) {
    return <span ref={ref}>{value.toLocaleString()}</span>;
  }

  return <span ref={ref}>0</span>;
}

export function StatsCard({ sessions }: Props) {
  const shouldReduceMotion = useReducedMotion();
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
    <motion.div
      className="bg-gray-900 border border-gray-800 rounded-xl p-6"
      initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
      animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
      transition={{ delay: 0.2, ...motionTransitions.smooth }}
    >
      <h2 className="text-lg font-semibold text-white mb-6">Usage statistics</h2>

      <div className="grid grid-cols-2 gap-4">
        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.3, ...motionTransitions.springy }}
        >
          <p className="text-xs text-gray-500 mb-1">Total sessions</p>
          <p className="text-2xl font-semibold text-white">
            <AnimatedNumber value={totalSessions} shouldReduceMotion={shouldReduceMotion} />
          </p>
        </motion.div>

        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.4, ...motionTransitions.springy }}
        >
          <p className="text-xs text-gray-500 mb-1">Lines analyzed</p>
          <p className="text-2xl font-semibold text-white">
            <AnimatedNumber value={totalLines} shouldReduceMotion={shouldReduceMotion} />
          </p>
        </motion.div>

        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.5, ...motionTransitions.springy }}
        >
          <p className="text-xs text-gray-500 mb-1">Most common format</p>
          <p className="text-sm font-medium text-white">{mostCommonFormat}</p>
        </motion.div>

        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.6, ...motionTransitions.springy }}
        >
          <p className="text-xs text-gray-500 mb-1">Total errors found</p>
          <p className="text-sm font-medium text-red-400">
            <AnimatedNumber value={totalErrors} shouldReduceMotion={shouldReduceMotion} />
          </p>
        </motion.div>
      </div>
    </motion.div>
  );
}
