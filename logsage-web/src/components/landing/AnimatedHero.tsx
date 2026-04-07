'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';

const SAMPLE_LOGS = [
  '[ERROR] NullReferenceException at UserService.GetProfile()',
  '[WARN] Database connection pool exhausted',
  '[ERROR] Failed to authenticate JWT token',
  '[INFO] Request completed in 234ms',
];

const PRICING_ENABLED = process.env.NEXT_PUBLIC_PRICING_ENABLED === 'true';

interface Particle {
  x: number;
  y: number;
  duration: number;
}

export function AnimatedHero() {
  const [currentLogIndex, setCurrentLogIndex] = useState(0);
  const [displayedText, setDisplayedText] = useState('');
  const [particles, setParticles] = useState<Particle[]>([]);

  // Initialize particles on client side only
  useEffect(() => {
    const initialParticles: Particle[] = Array.from({ length: 20 }, () => ({
      x: Math.random() * 100,
      y: Math.random() * 100,
      duration: Math.random() * 10 + 10,
    }));
    setParticles(initialParticles);
  }, []);

  useEffect(() => {
    const currentLog = SAMPLE_LOGS[currentLogIndex];
    let charIndex = 0;

    const typeInterval = setInterval(() => {
      if (charIndex < currentLog.length) {
        setDisplayedText(currentLog.slice(0, charIndex + 1));
        charIndex++;
      } else {
        clearInterval(typeInterval);
        setTimeout(() => {
          setCurrentLogIndex((prev) => (prev + 1) % SAMPLE_LOGS.length);
          setDisplayedText('');
        }, 2000);
      }
    }, 50);

    return () => clearInterval(typeInterval);
  }, [currentLogIndex]);

  const words = ['Understand', 'your', 'logs'];
  const subWords = ['in', '10', 'seconds'];

  return (
    <section className="relative min-h-[85vh] flex flex-col items-center justify-center text-center px-4 pt-16 pb-4 overflow-hidden">
      {/* Animated gradient background */}
      <div className="absolute inset-0 -z-10">
        <motion.div
          className="absolute inset-0 bg-gradient-to-br from-indigo-950/40 via-gray-950 to-violet-950/40"
          animate={{
            backgroundPosition: ['0% 0%', '100% 100%'],
          }}
          transition={{
            duration: 20,
            repeat: Infinity,
            repeatType: 'reverse',
          }}
        />
        {/* Animated grid lines */}
        <div className="absolute inset-0 bg-[linear-gradient(rgba(52,211,153,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(52,211,153,0.03)_1px,transparent_1px)] bg-[size:72px_72px] [mask-image:radial-gradient(ellipse_80%_50%_at_50%_50%,#000,transparent)]" />

        {/* Floating particles */}
        {particles.map((particle, i) => (
          <motion.div
            key={i}
            className="absolute w-1 h-1 bg-emerald-400/30 rounded-full"
            style={{
              left: `${particle.x}%`,
              top: `${particle.y}%`,
            }}
            animate={{
              y: ['0%', '100%'],
              opacity: [0, 1, 0],
            }}
            transition={{
              duration: particle.duration,
              repeat: Infinity,
              ease: 'linear',
            }}
          />
        ))}
      </div>

      <div className="max-w-5xl mx-auto relative z-10">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="inline-flex items-center gap-2 px-4 py-2 rounded-full border border-emerald-800/50 bg-emerald-950/30 backdrop-blur-sm text-emerald-400 text-sm mb-6"
        >
          <motion.span
            className="w-2 h-2 rounded-full bg-emerald-400"
            animate={{ opacity: [1, 0.3, 1] }}
            transition={{ duration: 2, repeat: Infinity }}
          />
          Free during early access · no account required
        </motion.div>

        <h1 className="text-6xl md:text-7xl font-bold text-white mb-4 leading-tight">
          <div className="flex flex-wrap justify-center gap-3 mb-2">
            {words.map((word, i) => (
              <motion.span
                key={i}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: i * 0.2 }}
              >
                {word}
              </motion.span>
            ))}
          </div>
          <div className="flex flex-wrap justify-center gap-3">
            {subWords.map((word, i) => (
              <motion.span
                key={i}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: 0.6 + i * 0.2 }}
                className={word === 'seconds' ? 'text-transparent bg-gradient-to-r from-emerald-400 to-violet-400 bg-clip-text' : ''}
              >
                {word}
              </motion.span>
            ))}
          </div>
        </h1>

        <motion.p
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 1 }}
          className="text-gray-400 text-xl md:text-2xl mb-8 max-w-3xl mx-auto"
        >
          Upload a log file. Get errors grouped, root causes explained,
          and actionable insights. No agent to install. No config.
        </motion.p>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 1.2 }}
          className="flex flex-col sm:flex-row gap-4 justify-center items-center mb-8"
        >
          <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
            <Link
              href="/analyze"
              className="relative group inline-block"
            >
              <div className="absolute -inset-1 bg-gradient-to-r from-emerald-600 to-violet-600 rounded-xl blur opacity-50 group-hover:opacity-100 transition duration-300" />
              <div className="relative bg-gradient-to-r from-emerald-600 to-emerald-500 hover:from-emerald-500 hover:to-emerald-400 text-white font-semibold px-8 py-4 rounded-xl text-lg transition-all duration-300">
                Analyze your logs free
              </div>
            </Link>
          </motion.div>
          <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
            <Link
              href="/pricing"
              className="inline-block border border-gray-700 hover:border-gray-500 bg-gray-900/50 backdrop-blur-sm text-gray-300 hover:text-white font-semibold px-8 py-4 rounded-xl text-lg transition-all duration-300"
            >
              {PRICING_ENABLED ? 'See pricing' : "It's free →"}
            </Link>
          </motion.div>
        </motion.div>

        {/* Mock terminal with typing animation */}
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 1.4 }}
          className="max-w-3xl mx-auto mb-8"
        >
          <div className="bg-gray-950/80 backdrop-blur-sm border border-gray-800 rounded-xl shadow-2xl overflow-hidden">
            <div className="flex items-center gap-2 px-4 py-3 bg-gray-900/50 border-b border-gray-800">
              <div className="w-3 h-3 rounded-full bg-red-500" />
              <div className="w-3 h-3 rounded-full bg-yellow-500" />
              <div className="w-3 h-3 rounded-full bg-green-500" />
              <span className="ml-2 text-xs text-gray-500 font-mono">application.log</span>
            </div>
            <div className="p-6 font-mono text-sm min-h-[120px]">
              {SAMPLE_LOGS.slice(0, currentLogIndex).map((log, i) => (
                <div
                  key={i}
                  className={`mb-2 ${
                    log.includes('ERROR')
                      ? 'text-red-400'
                      : log.includes('WARN')
                      ? 'text-yellow-400'
                      : 'text-blue-400'
                  }`}
                >
                  {log}
                </div>
              ))}
              <div
                className={`${
                  SAMPLE_LOGS[currentLogIndex].includes('ERROR')
                    ? 'text-red-400'
                    : SAMPLE_LOGS[currentLogIndex].includes('WARN')
                    ? 'text-yellow-400'
                    : 'text-blue-400'
                }`}
              >
                {displayedText}
                <motion.span
                  className="inline-block w-2 h-4 bg-emerald-400 ml-1"
                  animate={{ opacity: [1, 0] }}
                  transition={{ duration: 0.8, repeat: Infinity }}
                />
              </div>
            </div>
          </div>
        </motion.div>

        {/* Scroll indicator */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.6, delay: 2 }}
          className="flex flex-col items-center gap-2"
        >
          <span className="text-gray-500 text-xs">Scroll to explore</span>
          <motion.div
            animate={{ y: [0, 8, 0] }}
            transition={{ duration: 1.5, repeat: Infinity }}
            className="text-emerald-400 text-2xl"
          >
            ↓
          </motion.div>
        </motion.div>
      </div>
    </section>
  );
}
