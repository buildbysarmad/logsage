'use client';

import { motion, AnimatePresence } from 'framer-motion';
import { usePathname } from 'next/navigation';
import { useReducedMotion, motionVariants, motionTransitions } from '@/lib/motion';

interface PageTransitionProps {
  children: React.ReactNode;
}

export function PageTransition({ children }: PageTransitionProps) {
  const pathname = usePathname();
  const shouldReduceMotion = useReducedMotion();

  if (shouldReduceMotion) {
    return <>{children}</>;
  }

  return (
    <AnimatePresence mode="wait">
      <motion.div
        key={pathname}
        initial={motionVariants.fade.initial}
        animate={motionVariants.fade.animate}
        exit={motionVariants.fade.exit}
        transition={motionTransitions.smooth}
      >
        {children}
      </motion.div>
    </AnimatePresence>
  );
}
