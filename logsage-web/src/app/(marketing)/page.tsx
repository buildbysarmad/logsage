import Link from 'next/link';
import type { Metadata } from 'next';
import { MarketingNav } from '@/components/marketing/MarketingNav';

export const metadata: Metadata = {
  title: 'LogSage — AI-powered Log Analysis',
  description:
    'Upload your logs. Get errors grouped and root causes explained in seconds. Free. No signup required.',
};

const formats = ['Serilog', 'NLog', 'Log4Net', 'Standard', 'Plain text'];

const AI_ENABLED = process.env.NEXT_PUBLIC_AI_ENABLED === 'true';
const PRICING_ENABLED = process.env.NEXT_PUBLIC_PRICING_ENABLED === 'true';

const features = [
  { title: 'Auto format detection', desc: 'Serilog, NLog, Log4Net, Standard — detected automatically' },
  { title: 'Smart error grouping', desc: 'Similar errors grouped by pattern, not just exact match' },
  AI_ENABLED
    ? { title: 'AI root cause', desc: 'Claude explains what caused each error and how to fix it' }
    : { title: 'Session history', desc: 'Every analysis saved. Come back anytime to review past results.' },
  { title: 'Zero friction', desc: 'No account needed. Upload a file and get results in seconds' },
];

const plans = [
  { name: 'Free', price: '$0', period: '/month', limits: ['3 analyses per day', '500 lines per file', 'Error grouping', 'No account needed'], cta: 'Start free', href: '/analyze', featured: false },
  { name: 'Pro', price: '$19', period: '/month', limits: ['Unlimited analyses', '50,000 lines per file', 'AI root cause analysis', 'Session history'], cta: 'Get Pro', href: '/register', featured: true },
  { name: 'Team', price: '$49', period: '/month', limits: ['Everything in Pro', 'API access', '5 team seats', 'Priority support'], cta: 'Get Team', href: '/register', featured: false },
];

export default function LandingPage() {
  return (
    <main className="min-h-screen bg-gray-950">
      <MarketingNav />

      {/* Hero */}
      <section className="flex flex-col items-center text-center px-4 py-24 max-w-4xl mx-auto">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full border border-emerald-800 bg-emerald-950 text-emerald-400 text-xs mb-8">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
          Free during early access · no account required
        </div>
        <h1 className="text-5xl font-bold text-white mb-6 leading-tight">
          Understand your logs
          <br />
          <span className="text-emerald-400">in 10 seconds</span>
        </h1>
        <p className="text-gray-400 text-xl mb-10 max-w-2xl">
          Upload a log file. Get errors grouped, root causes explained,
          and fixes suggested by AI. No agent to install. No config.
        </p>
        <div className="flex flex-col sm:flex-row gap-4">
          <Link
            href="/analyze"
            className="bg-emerald-600 hover:bg-emerald-500 text-white font-semibold px-8 py-3 rounded-xl text-lg transition-colors"
          >
            Analyze your logs free
          </Link>
          <Link
            href="/pricing"
            className="border border-gray-700 hover:border-gray-500 text-gray-300 font-semibold px-8 py-3 rounded-xl text-lg transition-colors"
          >
            {PRICING_ENABLED ? 'See pricing' : "It's free →"}
          </Link>
        </div>
        <p className="mt-6 text-xs text-gray-600">
          Supports: {formats.join(' · ')}
        </p>
      </section>

      {/* Features */}
      <section className="px-4 py-16 max-w-5xl mx-auto">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
          {features.map((f) => (
            <div key={f.title} className="bg-gray-900 border border-gray-800 rounded-xl p-6">
              <h3 className="text-white font-semibold mb-2">{f.title}</h3>
              <p className="text-gray-400 text-sm">{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Pricing */}
      {PRICING_ENABLED ? (
        <section className="px-4 py-16 max-w-5xl mx-auto">
          <h2 className="text-3xl font-bold text-white text-center mb-12">
            Simple pricing
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
            {plans.map((plan) => (
              <div
                key={plan.name}
                className={`rounded-xl p-6 border ${
                  plan.featured
                    ? 'border-emerald-500 bg-emerald-950'
                    : 'border-gray-800 bg-gray-900'
                }`}
              >
                {plan.featured && (
                  <span className="text-xs text-emerald-400 font-semibold uppercase tracking-wide">
                    Most popular
                  </span>
                )}
                <h3 className="text-xl font-bold text-white mt-2">{plan.name}</h3>
                <div className="flex items-baseline gap-1 mt-2 mb-6">
                  <span className="text-4xl font-bold text-white">{plan.price}</span>
                  <span className="text-gray-400 text-sm">{plan.period}</span>
                </div>
                <ul className="space-y-2 mb-6">
                  {plan.limits.map((l) => (
                    <li key={l} className="text-sm text-gray-300 flex items-center gap-2">
                      <span className="text-emerald-400">✓</span> {l}
                    </li>
                  ))}
                </ul>
                <Link
                  href={plan.href}
                  className={`block text-center py-2.5 rounded-lg font-medium text-sm transition-colors ${
                    plan.featured
                      ? 'bg-emerald-600 hover:bg-emerald-500 text-white'
                      : 'border border-gray-700 hover:border-gray-500 text-gray-300'
                  }`}
                >
                  {plan.cta}
                </Link>
              </div>
            ))}
          </div>
        </section>
      ) : (
        <section className="px-4 py-16 max-w-2xl mx-auto text-center">
          <h2 className="text-3xl font-bold text-white mb-4">It's FREE</h2>
          <p className="text-gray-400 mb-6">
            LogSage is completely free during early access.
            No credit card. No limits. Just paste your logs.
          </p>
          <Link
            href="/analyze"
            className="inline-block bg-emerald-600 hover:bg-emerald-500 text-white font-medium px-8 py-3 rounded-lg transition-colors"
          >
            Start analyzing logs →
          </Link>
        </section>
      )}

      {/* Footer */}
      <footer className="border-t border-gray-800 py-8 text-center text-gray-600 text-sm">
        <p>
          log<span className="text-emerald-400">sage</span>.dev — built by{' '}
          <span className="text-gray-500">buildbysarmad</span>
        </p>
      </footer>
    </main>
  );
}
