'use client';

import { motion, useInView } from 'framer-motion';
import { useRef } from 'react';
import { NewAnalysisButton } from '@/components/ui/NewAnalysisButton';

export function AnimatedCTA() {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, amount: 0.5 });

  return (
    <section ref={ref} className="px-4 py-12 max-w-5xl mx-auto">
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={isInView ? { opacity: 1, scale: 1 } : {}}
        transition={{ duration: 0.6 }}
        className="relative overflow-hidden rounded-3xl"
      >
        {/* Pulsing background glow */}
        <motion.div
          className="absolute inset-0 bg-gradient-to-r from-emerald-600/30 via-violet-600/30 to-emerald-600/30"
          animate={{
            backgroundPosition: ['0% 50%', '100% 50%', '0% 50%'],
          }}
          transition={{
            duration: 5,
            repeat: Infinity,
            ease: 'linear',
          }}
          style={{ backgroundSize: '200% 200%' }}
        />

        <div className="relative bg-gradient-to-br from-emerald-950/50 via-gray-950/50 to-violet-950/50 backdrop-blur-xl border border-gray-800 p-12 md:p-16 text-center">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={isInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6, delay: 0.2 }}
          >
            <h2 className="text-4xl md:text-5xl font-bold text-white mb-6">
              Ready to debug
              <span className="text-transparent bg-gradient-to-r from-emerald-400 to-violet-400 bg-clip-text">
                {' '}
                10x faster
              </span>
              ?
            </h2>
            <p className="text-gray-400 text-lg md:text-xl mb-10 max-w-2xl mx-auto">
              Start analyzing your logs for free. No credit card required.
              No account needed. Just paste and go.
            </p>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={isInView ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.6, delay: 0.4 }}
          >
            <NewAnalysisButton href="/analyze" size="lg">
              Start analyzing for free
            </NewAnalysisButton>
          </motion.div>

          <motion.p
            initial={{ opacity: 0 }}
            animate={isInView ? { opacity: 1 } : {}}
            transition={{ duration: 0.6, delay: 0.6 }}
            className="mt-6 text-sm text-gray-500"
          >
            Join developers debugging smarter every day
          </motion.p>
        </div>
      </motion.div>
    </section>
  );
}
