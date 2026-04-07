'use client';

import { motion } from 'framer-motion';
import { useReducedMotion } from '@/lib/motion';

export function AnimatedBackground() {
  const shouldReduceMotion = useReducedMotion();

  if (shouldReduceMotion) {
    return (
      <div className="fixed inset-0 -z-10 bg-gray-950">
        <div className="absolute inset-0 bg-[linear-gradient(rgba(52,211,153,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(52,211,153,0.03)_1px,transparent_1px)] bg-[size:72px_72px] opacity-30" />
      </div>
    );
  }

  return (
    <div className="fixed inset-0 -z-10 bg-gray-950">
      {/* Animated gradient */}
      <motion.div
        className="absolute inset-0 bg-gradient-to-br from-indigo-950/20 via-gray-950 to-violet-950/20"
        animate={{
          backgroundPosition: ['0% 0%', '100% 100%'],
        }}
        transition={{
          duration: 30,
          repeat: Infinity,
          repeatType: 'reverse',
        }}
      />

      {/* Grid pattern */}
      <div className="absolute inset-0 bg-[linear-gradient(rgba(52,211,153,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(52,211,153,0.03)_1px,transparent_1px)] bg-[size:72px_72px] opacity-30" />
    </div>
  );
}
