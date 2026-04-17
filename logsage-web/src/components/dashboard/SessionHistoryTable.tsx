"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import type { SessionSummary } from "@/lib/types";
import {
  useReducedMotion,
  motionTransitions,
  staggerContainer,
} from "@/lib/motion";

interface Props {
  sessions: SessionSummary[];
}

export function SessionHistoryTable({ sessions }: Props) {
  const [page, setPage] = useState(0);
  const pageSize = 20;
  const shouldReduceMotion = useReducedMotion();

  const paginatedSessions = sessions.slice(
    page * pageSize,
    (page + 1) * pageSize,
  );
  const totalPages = Math.ceil(sessions.length / pageSize);

  if (sessions.length === 0) {
    return (
      <motion.div
        className="bg-gray-900 border border-gray-800 rounded-xl p-6"
        initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
        animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
        transition={{ delay: 0.3, ...motionTransitions.smooth }}
      >
        <h2 className="text-lg font-semibold text-white mb-4">
          Session history
        </h2>
        <motion.div
          className="text-center py-8"
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.4, ...motionTransitions.springy }}
        >
          <motion.div
            className="inline-block text-4xl mb-3"
            animate={shouldReduceMotion ? {} : { scale: [1, 1.1, 1] }}
            transition={{ repeat: Infinity, duration: 2 }}
          >
            📊
          </motion.div>
          <p className="text-sm text-gray-500">No sessions yet</p>
        </motion.div>
      </motion.div>
    );
  }

  return (
    <motion.div
      className="bg-gray-900 border border-gray-800 rounded-xl p-6"
      initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
      animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
      transition={{ delay: 0.3, ...motionTransitions.smooth }}
    >
      <h2 className="text-lg font-semibold text-white mb-4">Session history</h2>

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-800 text-gray-500 text-xs">
              <th className="text-left py-2 px-3 font-medium">Date</th>
              <th className="text-left py-2 px-3 font-medium">Format</th>
              <th className="text-right py-2 px-3 font-medium">Lines</th>
              <th className="text-right py-2 px-3 font-medium">Errors</th>
            </tr>
          </thead>
          <motion.tbody
            variants={shouldReduceMotion ? undefined : staggerContainer}
            initial={shouldReduceMotion ? false : "initial"}
            animate={shouldReduceMotion ? false : "animate"}
          >
            {paginatedSessions.map((session, index) => (
              <motion.tr
                key={session.id}
                className="border-b border-gray-800 hover:bg-gray-800/50 transition-colors"
                initial={shouldReduceMotion ? false : { opacity: 0, x: -20 }}
                animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
                transition={{
                  delay: index * 0.05,
                  ...motionTransitions.smooth,
                }}
                whileHover={
                  shouldReduceMotion
                    ? {}
                    : { backgroundColor: "rgba(31, 41, 55, 0.5)" }
                }
              >
                <td className="py-2 px-3 text-gray-300">
                  {new Date(session.createdAt).toLocaleDateString("en-US", {
                    month: "short",
                    day: "numeric",
                    year: "numeric",
                    hour: "2-digit",
                    minute: "2-digit",
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
                    <span className="text-yellow-400 ml-2">
                      +{session.warningCount}w
                    </span>
                  )}
                </td>
              </motion.tr>
            ))}
          </motion.tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-800">
          <motion.button
            onClick={() => setPage((p) => Math.max(0, p - 1))}
            disabled={page === 0}
            className="text-xs text-gray-400 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            whileHover={shouldReduceMotion ? {} : { scale: 1.05 }}
            whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}
          >
            Previous
          </motion.button>
          <span className="text-xs text-gray-500">
            Page {page + 1} of {totalPages}
          </span>
          <motion.button
            onClick={() => setPage((p) => Math.min(totalPages - 1, p + 1))}
            disabled={page === totalPages - 1}
            className="text-xs text-gray-400 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            whileHover={shouldReduceMotion ? {} : { scale: 1.05 }}
            whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}
          >
            Next
          </motion.button>
        </div>
      )}
    </motion.div>
  );
}
