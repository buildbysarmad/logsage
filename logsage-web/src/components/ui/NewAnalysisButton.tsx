'use client';

import Link from 'next/link';
import { motion } from 'framer-motion';
import { useReducedMotion } from '@/lib/motion';

interface NewAnalysisButtonProps {
  children: React.ReactNode;
  /** Render as a Next.js Link */
  href?: string;
  /** Render as a button with an onClick handler */
  onClick?: () => void;
  /** sm = nav/header (default), lg = hero/CTA sections */
  size?: 'sm' | 'lg';
  disabled?: boolean;
  className?: string;
}

/**
 * Unified call-to-action button for all "New Analysis / Try free / Analyze logs" CTAs.
 * Handles both Link and button variants. Animates consistently with the app theme.
 */
export function NewAnalysisButton({
  children,
  href,
  onClick,
  size = 'sm',
  disabled = false,
  className = '',
}: NewAnalysisButtonProps) {
  const reduce = useReducedMotion();
  const isLg = size === 'lg';

  // ── Inner button/link styles ───────────────────────────────────────────────
  const innerClass = [
    'relative group inline-flex items-center justify-center gap-2 overflow-hidden',
    'bg-gradient-to-r from-emerald-600 to-emerald-500',
    'hover:from-emerald-500 hover:to-emerald-400',
    'text-white transition-colors duration-200',
    isLg
      ? 'px-8 py-4 text-lg font-semibold rounded-xl'
      : 'px-4 py-2 text-sm font-medium rounded-lg',
    disabled ? 'opacity-50 cursor-not-allowed pointer-events-none' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  // ── Shimmer: sweeps left → right on hover ─────────────────────────────────
  const shimmer = !reduce && (
    <span
      aria-hidden
      className="absolute inset-0 pointer-events-none bg-gradient-to-r from-transparent via-white/15 to-transparent
                 -translate-x-full group-hover:translate-x-full transition-transform duration-500 ease-in-out"
    />
  );

  // ── Arrow: only shown for lg, bounces right continuously ──────────────────
  const arrow = isLg && (
    reduce ? (
      <span>→</span>
    ) : (
      <motion.span
        className="inline-block"
        animate={{ x: [0, 5, 0] }}
        transition={{ duration: 1.5, repeat: Infinity, ease: 'easeInOut' }}
      >
        →
      </motion.span>
    )
  );

  // ── Outer blur glow behind the button (lg only) ───────────────────────────
  const outerGlow = isLg && !reduce && (
    <span
      aria-hidden
      className="absolute -inset-1 -z-10 rounded-xl bg-gradient-to-r from-emerald-600 to-emerald-500
                 blur opacity-30 group-hover:opacity-60 transition-opacity duration-300"
    />
  );

  // ── Wrapper motion props ───────────────────────────────────────────────────
  const wrapperMotion =
    reduce || disabled
      ? {}
      : {
          whileHover: {
            scale: isLg ? 1.04 : 1.05,
            boxShadow: `0 0 ${isLg ? '30px' : '22px'} rgba(52,211,153,0.5)`,
          },
          whileTap: { scale: 0.97 },
        };

  const wrapperStyle = !reduce && !disabled
    ? { boxShadow: '0 0 14px rgba(52,211,153,0.18)' }
    : undefined;

  const body = (
    <>
      {shimmer}
      <span className="relative z-10 flex items-center gap-2">
        {children}
        {arrow}
      </span>
    </>
  );

  const wrapperClass = `relative inline-block group ${isLg ? 'rounded-xl' : 'rounded-lg'}`;

  if (href) {
    return (
      <motion.div
        className={wrapperClass}
        style={wrapperStyle}
        {...wrapperMotion}
      >
        {outerGlow}
        <Link href={href} className={innerClass}>
          {body}
        </Link>
      </motion.div>
    );
  }

  return (
    <motion.div
      className={wrapperClass}
      style={wrapperStyle}
      {...wrapperMotion}
    >
      {outerGlow}
      <button onClick={onClick} disabled={disabled} className={innerClass}>
        {body}
      </button>
    </motion.div>
  );
}
