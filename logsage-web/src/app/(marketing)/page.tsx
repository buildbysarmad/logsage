import type { Metadata } from 'next';
import { AnimatedNav } from '@/components/landing/AnimatedNav';
import { AnimatedHero } from '@/components/landing/AnimatedHero';
import { AnimatedFeatures } from '@/components/landing/AnimatedFeatures';
import { AnimatedStats } from '@/components/landing/AnimatedStats';
import { HowItWorks } from '@/components/landing/HowItWorks';
import { AnimatedCTA } from '@/components/landing/AnimatedCTA';

export const metadata: Metadata = {
  title: 'LogSage — Smart Log Analysis',
  description:
    'Upload your logs. Get errors grouped and root causes explained in seconds. Free. No signup required.',
};

export default function LandingPage() {
  return (
    <main className="min-h-screen bg-gray-950 dark">
      <AnimatedNav />
      <AnimatedHero />
      <AnimatedStats />
      <AnimatedFeatures />
      <HowItWorks />
      <AnimatedCTA />

      {/* Footer */}
      <footer className="border-t border-gray-800 py-12 text-center">
        <p className="text-gray-600 text-sm mb-4">
          log<span className="text-emerald-400">sage</span>.dev &copy; {new Date().getFullYear()} All rights reserved.
        </p>
        <p className="text-gray-700 text-xs">
          Stop debugging in the dark. Start analyzing logs smarter.
        </p>
      </footer>
    </main>
  );
}
