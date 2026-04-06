'use client';

import { motion } from 'framer-motion';
import { useInView } from 'framer-motion';
import { useRef } from 'react';

const AI_ENABLED = process.env.NEXT_PUBLIC_AI_ENABLED === 'true';

const features = [
  {
    icon: '🔍',
    title: 'Auto format detection',
    desc: 'Serilog, NLog, Log4Net, Standard — detected automatically',
  },
  {
    icon: '📊',
    title: 'Smart error grouping',
    desc: 'Similar errors grouped by pattern, not just exact match',
  },
  AI_ENABLED
    ? {
        icon: '🤖',
        title: 'AI root cause',
        desc: 'Claude explains what caused each error and how to fix it',
      }
    : {
        icon: '📜',
        title: 'Session history',
        desc: 'Every analysis saved. Come back anytime to review past results.',
      },
  {
    icon: '⚡',
    title: 'Zero friction',
    desc: 'No account needed. Upload a file and get results in seconds',
  },
];

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.15,
    },
  },
};

const itemVariants = {
  hidden: { opacity: 0, y: 40 },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      duration: 0.6,
      ease: 'easeOut' as const,
    },
  },
};

export function AnimatedFeatures() {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, amount: 0.3 });

  return (
    <section ref={ref} className="px-4 py-12 max-w-6xl mx-auto">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={isInView ? { opacity: 1, y: 0 } : {}}
        transition={{ duration: 0.6 }}
        className="text-center mb-16"
      >
        <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
          Everything you need to
          <span className="text-transparent bg-gradient-to-r from-emerald-400 to-violet-400 bg-clip-text">
            {' '}
            debug faster
          </span>
        </h2>
        <p className="text-gray-400 text-lg max-w-2xl mx-auto">
          Stop drowning in logs. LogSage extracts the signal from the noise.
        </p>
      </motion.div>

      <motion.div
        variants={containerVariants}
        initial="hidden"
        animate={isInView ? 'visible' : 'hidden'}
        className="grid grid-cols-1 md:grid-cols-2 gap-6"
      >
        {features.map((f) => (
          <motion.div
            key={f.title}
            variants={itemVariants}
            whileHover={{ y: -8, scale: 1.02 }}
            className="group relative"
          >
            <div className="absolute -inset-0.5 bg-gradient-to-r from-emerald-600 to-violet-600 rounded-2xl blur opacity-0 group-hover:opacity-20 transition duration-500" />
            <div className="relative bg-gray-900/80 backdrop-blur-sm border border-gray-800 hover:border-gray-700 rounded-2xl p-8 h-full transition-all duration-300">
              <div className="text-4xl mb-4">{f.icon}</div>
              <h3 className="text-white font-semibold text-xl mb-3">{f.title}</h3>
              <p className="text-gray-400 leading-relaxed">{f.desc}</p>
            </div>
          </motion.div>
        ))}
      </motion.div>
    </section>
  );
}
