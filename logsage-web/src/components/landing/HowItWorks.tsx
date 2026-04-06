'use client';

import { motion, useInView } from 'framer-motion';
import { useRef } from 'react';

const steps = [
  {
    number: '01',
    title: 'Upload your logs',
    desc: 'Drag and drop or paste your log file. We support all major formats.',
    icon: '📤',
  },
  {
    number: '02',
    title: 'Automatic analysis',
    desc: 'Our engine detects the format and groups similar errors instantly.',
    icon: '⚙️',
  },
  {
    number: '03',
    title: 'Get insights',
    desc: 'View grouped errors, patterns, and detailed root cause analysis.',
    icon: '💡',
  },
];

export function HowItWorks() {
  const ref = useRef(null);
  const isInView = useInView(ref, { once: true, amount: 0.3 });

  return (
    <section ref={ref} className="px-4 py-12 max-w-5xl mx-auto">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={isInView ? { opacity: 1, y: 0 } : {}}
        transition={{ duration: 0.6 }}
        className="text-center mb-16"
      >
        <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
          How it works
        </h2>
        <p className="text-gray-400 text-lg max-w-2xl mx-auto">
          Three simple steps to understand what&apos;s breaking your application
        </p>
      </motion.div>

      <div className="relative">
        {/* Animated connecting line */}
        <motion.div
          className="absolute left-8 md:left-1/2 top-0 bottom-0 w-0.5 bg-gradient-to-b from-emerald-600 via-violet-600 to-transparent"
          initial={{ scaleY: 0 }}
          animate={isInView ? { scaleY: 1 } : {}}
          transition={{ duration: 1.5, ease: 'easeInOut' }}
          style={{ transformOrigin: 'top' }}
        />

        <div className="space-y-16">
          {steps.map((step, index) => (
            <motion.div
              key={step.number}
              initial={{ opacity: 0, x: index % 2 === 0 ? -40 : 40 }}
              animate={isInView ? { opacity: 1, x: 0 } : {}}
              transition={{ duration: 0.6, delay: index * 0.2 }}
              className={`relative flex items-center gap-8 ${
                index % 2 === 0 ? 'md:flex-row' : 'md:flex-row-reverse'
              } flex-col md:gap-16`}
            >
              {/* Timeline dot */}
              <motion.div
                className="absolute left-8 md:left-1/2 w-4 h-4 rounded-full bg-gradient-to-r from-emerald-600 to-violet-600 -ml-[7px] md:-ml-2"
                initial={{ scale: 0 }}
                animate={isInView ? { scale: 1 } : {}}
                transition={{ duration: 0.4, delay: index * 0.2 + 0.5 }}
              >
                <motion.div
                  className="absolute inset-0 rounded-full bg-gradient-to-r from-emerald-600 to-violet-600"
                  animate={{ scale: [1, 1.5, 1], opacity: [1, 0, 1] }}
                  transition={{ duration: 2, repeat: Infinity, delay: index * 0.3 }}
                />
              </motion.div>

              {/* Content card */}
              <div className={`flex-1 ${index % 2 === 0 ? 'md:text-right' : 'md:text-left'} text-left ml-20 md:ml-0`}>
                <motion.div
                  whileHover={{ scale: 1.05 }}
                  className="inline-block bg-gray-900/80 backdrop-blur-sm border border-gray-800 rounded-2xl p-8 max-w-md"
                >
                  <div className="text-5xl mb-4">{step.icon}</div>
                  <div className="text-emerald-400 font-mono text-sm mb-2">{step.number}</div>
                  <h3 className="text-white font-bold text-2xl mb-3">{step.title}</h3>
                  <p className="text-gray-400 leading-relaxed">{step.desc}</p>
                </motion.div>
              </div>

              {/* Spacer for alternating layout */}
              <div className="flex-1 hidden md:block" />
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  );
}
