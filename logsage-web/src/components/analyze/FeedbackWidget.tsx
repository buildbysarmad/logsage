"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { analyzeApi } from "@/lib/api";
import { useReducedMotion } from "@/lib/motion";

interface FeedbackWidgetProps {
  sessionToken?: string;
}

export function FeedbackWidget({ sessionToken }: FeedbackWidgetProps) {
  const [score, setScore] = useState<1 | -1 | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const shouldReduceMotion = useReducedMotion();

  if (!sessionToken || submitted) return null;

  const handleFeedback = async (newScore: 1 | -1) => {
    if (score !== null) return; // Already submitted

    try {
      setScore(newScore);
      await analyzeApi.submitFeedback(sessionToken, newScore);
      setSubmitted(true);

      // Hide after 2 seconds
      setTimeout(() => setScore(null), 2000);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to submit feedback";
      setError(message);
      setScore(null);
    }
  };

  return (
    <motion.div
      className="flex items-center gap-2 text-xs text-gray-400"
      initial={shouldReduceMotion ? false : { opacity: 0, y: -10 }}
      animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
      transition={{ delay: 0.8 }}
    >
      <AnimatePresence mode="wait">
        {error ? (
          <motion.span
            key="error"
            className="text-red-400"
            initial={shouldReduceMotion ? undefined : { opacity: 0 }}
            animate={shouldReduceMotion ? undefined : { opacity: 1 }}
            exit={shouldReduceMotion ? undefined : { opacity: 0 }}
          >
            {error}
          </motion.span>
        ) : submitted ? (
          <motion.span
            key="thanks"
            className="text-emerald-400"
            initial={
              shouldReduceMotion ? undefined : { opacity: 0, scale: 0.8 }
            }
            animate={shouldReduceMotion ? undefined : { opacity: 1, scale: 1 }}
            exit={shouldReduceMotion ? undefined : { opacity: 0 }}
          >
            Thanks for your feedback!
          </motion.span>
        ) : (
          <motion.div
            key="buttons"
            className="flex items-center gap-2"
            initial={shouldReduceMotion ? undefined : { opacity: 0 }}
            animate={shouldReduceMotion ? undefined : { opacity: 1 }}
            exit={shouldReduceMotion ? undefined : { opacity: 0 }}
          >
            <span>Was this helpful?</span>
            <motion.button
              onClick={() => handleFeedback(1)}
              disabled={score !== null}
              className="text-lg hover:text-emerald-400 transition-colors disabled:opacity-50"
              whileHover={
                shouldReduceMotion || score !== null
                  ? {}
                  : { scale: 1.2, y: -2 }
              }
              whileTap={
                shouldReduceMotion || score !== null ? {} : { scale: 0.9 }
              }
              aria-label="Thumbs up"
            >
              👍
            </motion.button>
            <motion.button
              onClick={() => handleFeedback(-1)}
              disabled={score !== null}
              className="text-lg hover:text-red-400 transition-colors disabled:opacity-50"
              whileHover={
                shouldReduceMotion || score !== null
                  ? {}
                  : { scale: 1.2, y: -2 }
              }
              whileTap={
                shouldReduceMotion || score !== null ? {} : { scale: 0.9 }
              }
              aria-label="Thumbs down"
            >
              👎
            </motion.button>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
}
