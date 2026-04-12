'use client';

import { useState } from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useReducedMotion } from '@/lib/motion';

const pricingEnabled = process.env.NEXT_PUBLIC_PRICING_ENABLED === 'true';

const FloatingParticles = () => {
  const [particles] = useState(() => Array.from({ length: 20 }, (_, i) => ({
    id: i,
    x: Math.random() * 100,
    y: Math.random() * 100,
    delay: Math.random() * 2,
    duration: 3 + Math.random() * 2,
  })));

  return (
    <div className="absolute inset-0 overflow-hidden pointer-events-none">
      {particles.map((p) => (
        <motion.div
          key={p.id}
          className="absolute w-1 h-1 bg-emerald-400/20 rounded-full"
          style={{ left: `${p.x}%`, top: `${p.y}%` }}
          animate={{
            y: [0, -30, 0],
            opacity: [0.2, 0.6, 0.2],
            scale: [1, 1.5, 1],
          }}
          transition={{
            duration: p.duration,
            repeat: Infinity,
            delay: p.delay,
            ease: 'easeInOut',
          }}
        />
      ))}
    </div>
  );
};

export default function PricingPage() {
  const shouldReduceMotion = useReducedMotion();

  if (!pricingEnabled) {
    return (
      <div className="min-h-screen bg-gradient-to-b from-[#07090d] via-[#0a0f14] to-[#07090d] px-4 py-24 flex items-center justify-center relative overflow-hidden">
        {!shouldReduceMotion && <FloatingParticles />}

        <motion.div
          className="max-w-lg mx-auto text-center relative z-10"
          initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
        >
          <motion.p
            className="text-emerald-400 text-sm font-medium tracking-widest uppercase mb-6"
            initial={shouldReduceMotion ? false : { opacity: 0, y: -10 }}
            animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
            transition={{ delay: 0.2, duration: 0.5 }}
          >
            Early Access
          </motion.p>

          <motion.div
            className="relative mb-8"
            initial={shouldReduceMotion ? false : { scale: 0.9, opacity: 0 }}
            animate={shouldReduceMotion ? false : { scale: 1, opacity: 1 }}
            transition={{ delay: 0.3, duration: 0.6, type: 'spring', stiffness: 100 }}
          >
            <motion.h1
              className="text-7xl md:text-8xl font-bold mb-6 bg-gradient-to-r from-emerald-400 via-green-300 to-emerald-400 bg-clip-text text-transparent relative"
              animate={shouldReduceMotion ? {} : {
                backgroundPosition: ['0% 50%', '100% 50%', '0% 50%'],
              }}
              transition={{ duration: 5, repeat: Infinity, ease: 'linear' }}
              style={{ backgroundSize: '200% 200%' }}
            >
              It&apos;s FREE
            </motion.h1>

            {!shouldReduceMotion && (
              <motion.div
                className="absolute inset-0 bg-gradient-to-r from-emerald-400/20 via-green-300/20 to-emerald-400/20 blur-3xl -z-10"
                animate={{
                  scale: [1, 1.2, 1],
                  opacity: [0.3, 0.5, 0.3],
                }}
                transition={{ duration: 3, repeat: Infinity, ease: 'easeInOut' }}
              />
            )}
          </motion.div>

          <motion.p
            className="text-gray-400 text-lg mb-12 leading-relaxed"
            initial={shouldReduceMotion ? false : { opacity: 0, y: 10 }}
            animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
            transition={{ delay: 0.5, duration: 0.5 }}
          >
            LogSage is completely free during early access.
            <br />
            <span className="text-emerald-400/80">No credit card. No limits. No BS.</span>
            <br />
            Just paste your logs and get insights.
          </motion.p>

          <motion.div
            initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
            animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
            transition={{ delay: 0.7, duration: 0.5 }}
          >
            <Link
              href="/analyze"
              className="group relative inline-block"
            >
              <motion.div
                className="relative bg-gradient-to-r from-emerald-600 to-green-600 hover:from-emerald-500 hover:to-green-500 text-white font-semibold px-10 py-4 rounded-xl transition-all overflow-hidden"
                whileHover={shouldReduceMotion ? {} : { scale: 1.05, y: -2 }}
                whileTap={shouldReduceMotion ? {} : { scale: 0.98 }}
              >
                <span className="relative z-10 flex items-center gap-2">
                  Start analyzing logs
                  <motion.span
                    animate={shouldReduceMotion ? {} : { x: [0, 5, 0] }}
                    transition={{ duration: 1.5, repeat: Infinity }}
                  >
                    →
                  </motion.span>
                </span>

                {!shouldReduceMotion && (
                  <>
                    <motion.div
                      className="absolute inset-0 bg-gradient-to-r from-white/0 via-white/20 to-white/0"
                      animate={{ x: ['-100%', '100%'] }}
                      transition={{ duration: 2, repeat: Infinity, ease: 'linear' }}
                    />
                    <div className="absolute -inset-1 bg-gradient-to-r from-emerald-600 to-green-600 rounded-xl blur opacity-30 group-hover:opacity-60 transition-opacity -z-10" />
                  </>
                )}
              </motion.div>
            </Link>
          </motion.div>

          <motion.div
            className="mt-12 flex items-center justify-center gap-6 text-sm text-gray-500"
            initial={shouldReduceMotion ? false : { opacity: 0 }}
            animate={shouldReduceMotion ? false : { opacity: 1 }}
            transition={{ delay: 1, duration: 0.5 }}
          >
            <div className="flex items-center gap-2">
              <span className="text-emerald-400">✓</span>
              <span>5,000 lines</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-emerald-400">✓</span>
              <span>No signup</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-emerald-400">✓</span>
              <span>Instant results</span>
            </div>
          </motion.div>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-950 via-gray-900 to-gray-950 px-4 py-16 relative overflow-hidden">
      {!shouldReduceMotion && <FloatingParticles />}

      <div className="max-w-4xl mx-auto text-center relative z-10">
        <motion.h1
          className="text-5xl font-bold text-white mb-4"
          initial={shouldReduceMotion ? false : { opacity: 0, y: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
        >
          Simple pricing
        </motion.h1>

        <motion.p
          className="text-gray-400 mb-16"
          initial={shouldReduceMotion ? false : { opacity: 0 }}
          animate={shouldReduceMotion ? false : { opacity: 1 }}
          transition={{ delay: 0.2, duration: 0.5 }}
        >
          Start free. Upgrade when you need more.
        </motion.p>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          {[
            { name: 'Free', price: '$0', features: ['3 analyses/day', '500 lines max', 'Error grouping', 'No account needed'], cta: 'Start free', href: '/analyze', featured: false },
            { name: 'Pro', price: '$19/mo', features: ['Unlimited analyses', '50k lines', 'Root cause analysis', 'Session history'], cta: 'Get Pro', href: '/register', featured: true },
            { name: 'Team', price: '$49/mo', features: ['Everything in Pro', 'API access', '5 seats', 'Priority support'], cta: 'Get Team', href: '/register', featured: false },
          ].map((plan, idx) => (
            <motion.div
              key={plan.name}
              className={`rounded-xl p-6 border text-left relative group ${
                plan.featured
                  ? 'border-emerald-500 bg-gradient-to-br from-emerald-950/50 to-emerald-900/30'
                  : 'border-gray-800 bg-gray-900/50 backdrop-blur-sm'
              }`}
              initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
              animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
              transition={{ delay: 0.3 + idx * 0.1, duration: 0.5 }}
              whileHover={shouldReduceMotion ? {} : { y: -8, scale: 1.02 }}
            >
              {plan.featured && !shouldReduceMotion && (
                <motion.div
                  className="absolute -inset-0.5 bg-gradient-to-r from-emerald-600 to-green-600 rounded-xl blur opacity-20 group-hover:opacity-40 transition-opacity -z-10"
                  animate={{
                    scale: [1, 1.05, 1],
                  }}
                  transition={{ duration: 2, repeat: Infinity, ease: 'easeInOut' }}
                />
              )}

              <h2 className="text-xl font-bold text-white">{plan.name}</h2>
              <p className="text-3xl font-bold text-white mt-2 mb-6">{plan.price}</p>
              <ul className="space-y-2 mb-6">
                {plan.features.map((f, i) => (
                  <motion.li
                    key={f}
                    className="text-sm text-gray-300 flex gap-2"
                    initial={shouldReduceMotion ? false : { opacity: 0, x: -10 }}
                    animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
                    transition={{ delay: 0.5 + idx * 0.1 + i * 0.05 }}
                  >
                    <span className="text-emerald-400">✓</span>
                    {f}
                  </motion.li>
                ))}
              </ul>
              <Link href={plan.href}>
                <motion.div
                  className={`block text-center py-2.5 rounded-lg text-sm font-medium ${
                    plan.featured
                      ? 'bg-emerald-600 hover:bg-emerald-500 text-white'
                      : 'border border-gray-700 text-gray-300 hover:border-gray-500'
                  }`}
                  whileHover={shouldReduceMotion ? {} : { scale: 1.05 }}
                  whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}
                >
                  {plan.cta}
                </motion.div>
              </Link>
            </motion.div>
          ))}
        </div>

        <motion.p
          className="mt-16 text-gray-600 text-sm"
          initial={shouldReduceMotion ? false : { opacity: 0 }}
          animate={shouldReduceMotion ? false : { opacity: 1 }}
          transition={{ delay: 1, duration: 0.5 }}
        >
          Questions? Email{' '}
          <a href="mailto:buildbysarmad@gmail.com" className="text-gray-400 hover:text-white transition-colors">
            buildbysarmad@gmail.com
          </a>
        </motion.p>
      </div>
    </div>
  );
}