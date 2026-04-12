'use client';

import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { motion, AnimatePresence } from 'framer-motion';
import { analyzeApi } from '@/lib/api';
import type { AnalysisResult, ErrorGroup } from '@/lib/types';
import ErrorGroupSidebar from '@/components/analyze/ErrorGroupSidebar';
import LogDetailPanel from '@/components/analyze/LogDetailPanel';
import AiAnalysisPanel from '@/components/analyze/AiAnalysisPanel';
import { AnimatedBackground } from '@/components/motion/AnimatedBackground';
import { useReducedMotion, motionVariants, motionTransitions } from '@/lib/motion';

export default function AnalyzePage() {
  const [result, setResult] = useState<AnalysisResult | null>(null);
  const [selected, setSelected] = useState<ErrorGroup | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<'upload' | 'paste'>('upload');
  const [paste, setPaste] = useState('');
  const shouldReduceMotion = useReducedMotion();

  const analyze = async (rawLog: string) => {
    // Validate input is not empty
    if (!rawLog || !rawLog.trim()) {
      setError('Please enter log content to analyze.');
      return;
    }

    // Validate line count before submission
    const lineCount = rawLog.split('\n').length;
    if (lineCount > 5000) {
      setError(`Your log has ${lineCount.toLocaleString()} lines. The limit is 5,000.`);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const { data } = await analyzeApi.text(rawLog);
      setResult(data);
      setSelected(data.errorGroups[0] ?? null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Analysis failed';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const onDrop = useCallback(async (files: File[]) => {
    if (!files[0]) return;

    // Validate file is not empty
    if (files[0].size === 0) {
      setError('File is empty. Please select a file with log content.');
      return;
    }

    // Validate file size before upload
    const fileSizeMB = files[0].size / (1024 * 1024);
    if (files[0].size > 2 * 1024 * 1024) {
      setError(`File is too large (${fileSizeMB.toFixed(1)}MB). Maximum size is 2MB.`);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const { data } = await analyzeApi.file(files[0]);
      setResult(data);
      setSelected(data.errorGroups[0] ?? null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'Upload failed';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'text/plain': ['.log', '.txt'] },
    maxFiles: 1,
    disabled: loading,
  });

  const aiForSelected = result?.aiAnalysis.find(
    (a) => a.groupKey === selected?.groupKey
  );

  if (!result) {
    return (
      <div className="min-h-[calc(100vh-49px)] flex flex-col items-center justify-center p-8 relative">
        <AnimatedBackground />

        <motion.div
          className="relative z-10 w-full max-w-xl"
          initial={shouldReduceMotion ? false : motionVariants.fadeInUp.initial}
          animate={shouldReduceMotion ? false : motionVariants.fadeInUp.animate}
          transition={motionTransitions.smooth}
        >
          <motion.p
            className="text-gray-400 mb-8 text-sm text-center"
            initial={shouldReduceMotion ? false : { opacity: 0, y: -10 }}
            animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
            transition={{ delay: 0.1, ...motionTransitions.smooth }}
          >
            Paste your logs. Know what broke. In 10 seconds.
          </motion.p>

          <motion.div
            className="flex gap-2 mb-6 justify-center"
            initial={shouldReduceMotion ? false : { opacity: 0, y: 10 }}
            animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
            transition={{ delay: 0.2, ...motionTransitions.smooth }}
          >
            {(['upload', 'paste'] as const).map((t) => (
              <motion.button
                key={t}
                onClick={() => setTab(t)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-all relative ${
                  tab === t
                    ? 'bg-emerald-600 text-white shadow-[0_0_20px_rgba(52,211,153,0.3)]'
                    : 'bg-gray-800 text-gray-400 hover:text-white hover:shadow-[0_0_15px_rgba(52,211,153,0.15)]'
                }`}
                whileHover={shouldReduceMotion ? {} : { scale: 1.05, y: -2 }}
                whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}
                animate={shouldReduceMotion || tab !== t ? {} : {
                  boxShadow: [
                    '0 0 20px rgba(52,211,153,0.3)',
                    '0 0 30px rgba(52,211,153,0.5)',
                    '0 0 20px rgba(52,211,153,0.3)',
                  ],
                }}
                transition={tab === t ? { duration: 2, repeat: Infinity } : {}}
              >
                {t === 'upload' ? 'File upload' : 'Paste text'}
              </motion.button>
            ))}
          </motion.div>

          <AnimatePresence mode="wait">
            {tab === 'upload' ? (
              <motion.div
                key="upload"
                initial={shouldReduceMotion ? undefined : { opacity: 0, x: -20 }}
                animate={shouldReduceMotion ? undefined : { opacity: 1, x: 0 }}
                exit={shouldReduceMotion ? undefined : { opacity: 0, x: 20 }}
                transition={motionTransitions.smooth}
                className="w-full"
              >
                <div
                  {...getRootProps()}
                  className={`relative w-full border-2 border-dashed rounded-xl p-12 text-center cursor-pointer transition-all duration-300 ${
                    isDragActive
                      ? 'border-emerald-400 bg-emerald-950/30 scale-105'
                      : 'border-gray-700 hover:border-gray-500 bg-gray-900/50'
                  }`}
                >
                  <input {...getInputProps()} />
                  <motion.div
                    animate={shouldReduceMotion ? {} : isDragActive ? { scale: [1, 1.1, 1] } : {}}
                    transition={{ repeat: isDragActive ? Infinity : 0, duration: 1 }}
                    className="text-4xl mb-4"
                  >
                    📤
                  </motion.div>
                  <p className="text-gray-400 text-sm">
                    {loading
                      ? 'Analyzing...'
                      : isDragActive
                      ? 'Drop your log file here...'
                      : 'Drag & drop a .log or .txt file, or click to browse'}
                  </p>

                  {loading && !shouldReduceMotion && (
                    <motion.div
                      className="absolute inset-0 border-2 border-emerald-400 rounded-xl"
                      animate={{ opacity: [0.3, 1, 0.3] }}
                      transition={{ repeat: Infinity, duration: 1.5 }}
                    />
                  )}
                </div>
              </motion.div>
            ) : (
              <motion.div
                key="paste"
                initial={shouldReduceMotion ? undefined : { opacity: 0, x: 20 }}
                animate={shouldReduceMotion ? undefined : { opacity: 1, x: 0 }}
                exit={shouldReduceMotion ? undefined : { opacity: 0, x: -20 }}
                transition={motionTransitions.smooth}
                className="w-full"
              >
                <textarea
                  value={paste}
                  onChange={(e) => setPaste(e.target.value)}
                  placeholder="Paste your log content here..."
                  className="w-full h-48 bg-gray-900 text-gray-200 text-sm font-mono
                             border border-gray-700 rounded-xl p-4 resize-none
                             focus:outline-none focus:border-emerald-500 transition-all duration-300
                             focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
                />
                <motion.button
                  onClick={() => analyze(paste)}
                  disabled={!paste.trim() || loading}
                  className="relative mt-3 w-full bg-emerald-600 hover:bg-emerald-500
                             disabled:opacity-50 disabled:cursor-not-allowed
                             text-white font-medium py-2.5 rounded-lg transition-all overflow-hidden
                             shadow-[0_0_20px_rgba(52,211,153,0.3)]"
                  whileHover={shouldReduceMotion || !paste.trim() || loading ? {} : {
                    scale: 1.02,
                    boxShadow: '0 0 30px rgba(52,211,153,0.5)',
                  }}
                  whileTap={shouldReduceMotion || !paste.trim() || loading ? {} : { scale: 0.98 }}
                >
                  <span className="relative z-10">
                    {loading ? 'Analyzing...' : 'Analyze logs'}
                  </span>
                  <AnimatePresence>
                    {loading && (
                      <motion.div
                        className="absolute inset-0 bg-gradient-to-r from-emerald-600 via-emerald-400 to-emerald-600"
                        initial={{ x: '-100%' }}
                        animate={{ x: '100%' }}
                        exit={{ opacity: 0 }}
                        transition={{ repeat: Infinity, duration: 1, ease: 'linear' }}
                      />
                    )}
                  </AnimatePresence>
                </motion.button>
              </motion.div>
            )}
          </AnimatePresence>

          <AnimatePresence>
            {error && (
              <motion.p
                className="mt-4 text-red-400 text-sm text-center"
                initial={shouldReduceMotion ? undefined : { opacity: 0, y: -10 }}
                animate={shouldReduceMotion ? undefined : { opacity: 1, y: 0 }}
                exit={shouldReduceMotion ? undefined : { opacity: 0, y: -10 }}
                transition={motionTransitions.smooth}
              >
                {error}
              </motion.p>
            )}
          </AnimatePresence>

          <motion.p
            className="mt-8 text-xs text-gray-600 text-center"
            initial={shouldReduceMotion ? false : { opacity: 0 }}
            animate={shouldReduceMotion ? false : { opacity: 1 }}
            transition={{ delay: 0.4, ...motionTransitions.smooth }}
          >
            Free during early access · 5,000 lines · 2MB max · No account needed · No judgement on your logs 🙈
          </motion.p>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="h-[calc(100vh-49px)] flex flex-col bg-gray-950 text-gray-200 overflow-hidden">
      {/* Stats bar */}
      <motion.div
        className="flex items-center justify-between px-4 py-2.5 border-b border-gray-800 bg-gray-900 shrink-0"
        initial={shouldReduceMotion ? false : { opacity: 0, y: -20 }}
        animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
        transition={motionTransitions.smooth}
      >
        <div className="flex items-center gap-3 text-xs text-gray-400">
          <motion.span
            className="bg-gray-800 px-2 py-0.5 rounded"
            initial={shouldReduceMotion ? false : { scale: 0 }}
            animate={shouldReduceMotion ? false : { scale: 1 }}
            transition={{ delay: 0.1, ...motionTransitions.springy }}
          >
            {result.detectedFormat}
          </motion.span>
          <motion.span
            initial={shouldReduceMotion ? false : { opacity: 0 }}
            animate={shouldReduceMotion ? false : { opacity: 1 }}
            transition={{ delay: 0.2 }}
          >
            {result.totalLines.toLocaleString()} lines
          </motion.span>
          <motion.span
            className="text-red-400"
            initial={shouldReduceMotion ? false : { opacity: 0 }}
            animate={shouldReduceMotion ? false : { opacity: 1 }}
            transition={{ delay: 0.3 }}
          >
            {result.errorCount} errors
          </motion.span>
          <motion.span
            className="text-yellow-400"
            initial={shouldReduceMotion ? false : { opacity: 0 }}
            animate={shouldReduceMotion ? false : { opacity: 1 }}
            transition={{ delay: 0.4 }}
          >
            {result.warningCount} warnings
          </motion.span>
          {result.infoCount > 0 && (
            <motion.span
              className="text-blue-400"
              initial={shouldReduceMotion ? false : { opacity: 0 }}
              animate={shouldReduceMotion ? false : { opacity: 1 }}
              transition={{ delay: 0.5 }}
            >
              {result.infoCount} info
            </motion.span>
          )}
          {result.debugCount > 0 && (
            <motion.span
              className="text-gray-400"
              initial={shouldReduceMotion ? false : { opacity: 0 }}
              animate={shouldReduceMotion ? false : { opacity: 1 }}
              transition={{ delay: 0.6 }}
            >
              {result.debugCount} debug
            </motion.span>
          )}
          {result.parseErrorCount > 0 && (
            <motion.span
              className="text-red-500 font-semibold"
              initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.8 }}
              animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
              transition={{ delay: 0.7 }}
            >
              ⚠ {result.parseErrorCount} parse errors
            </motion.span>
          )}
          {result.wasTruncated && (
            <motion.span
              className="text-orange-400"
              initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.8 }}
              animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
              transition={{ delay: 0.5, ...motionTransitions.springy }}
            >
              truncated to 500 lines
            </motion.span>
          )}
        </div>

        {/* Prominent New Analysis Button */}
        <motion.button
          onClick={() => { setResult(null); setSelected(null); setPaste(''); setError(null); }}
          className="relative flex items-center gap-2 bg-emerald-600 hover:bg-emerald-500 text-white font-medium px-4 py-1.5 rounded-lg transition-all overflow-hidden group"
          initial={shouldReduceMotion ? false : { opacity: 0, scale: 0.9 }}
          animate={shouldReduceMotion ? false : { opacity: 1, scale: 1 }}
          transition={{ delay: 0.6, ...motionTransitions.springy }}
          whileHover={shouldReduceMotion ? {} : { scale: 1.05 }}
          whileTap={shouldReduceMotion ? {} : { scale: 0.95 }}
        >
          <span className="text-lg">✨</span>
          <span className="text-sm relative z-10">New Analysis</span>
          <motion.div
            className="absolute inset-0 bg-gradient-to-r from-emerald-400/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity"
            initial={false}
          />
        </motion.button>
      </motion.div>

      {/* 3-panel layout */}
      <div className="flex flex-1 overflow-hidden min-h-0">
        <ErrorGroupSidebar
          groups={result.errorGroups}
          selected={selected}
          onSelect={setSelected}
        />

        <LogDetailPanel group={selected} />

        {false && (
          <AiAnalysisPanel analysis={aiForSelected ?? null} group={selected} />
        )}
      </div>
    </div>
  );
}
