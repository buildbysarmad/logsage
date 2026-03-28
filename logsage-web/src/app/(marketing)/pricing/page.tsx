import Link from 'next/link';
import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Pricing — LogSage',
  description: 'Simple pricing for AI-powered log analysis. Free tier available.',
};

export default function PricingPage() {
  return (
    <div className="min-h-screen bg-gray-950 px-4 py-16">
      <div className="max-w-4xl mx-auto text-center">
        <h1 className="text-4xl font-bold text-white mb-4">Simple pricing</h1>
        <p className="text-gray-400 mb-12">Start free. Upgrade when you need more.</p>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          {[
            { name: 'Free', price: '$0', features: ['3 analyses/day', '500 lines max', 'Error grouping', 'No account needed'], cta: 'Start free', href: '/analyze', featured: false },
            { name: 'Pro', price: '$19/mo', features: ['Unlimited analyses', '50k lines', 'AI root cause', 'Session history'], cta: 'Get Pro', href: '/register', featured: true },
            { name: 'Team', price: '$49/mo', features: ['Everything in Pro', 'API access', '5 seats', 'Priority support'], cta: 'Get Team', href: '/register', featured: false },
          ].map((plan) => (
            <div key={plan.name} className={`rounded-xl p-6 border text-left ${plan.featured ? 'border-emerald-500 bg-emerald-950' : 'border-gray-800 bg-gray-900'}`}>
              <h2 className="text-xl font-bold text-white">{plan.name}</h2>
              <p className="text-3xl font-bold text-white mt-2 mb-6">{plan.price}</p>
              <ul className="space-y-2 mb-6">
                {plan.features.map((f) => (
                  <li key={f} className="text-sm text-gray-300 flex gap-2">
                    <span className="text-emerald-400">✓</span>{f}
                  </li>
                ))}
              </ul>
              <Link href={plan.href} className={`block text-center py-2.5 rounded-lg text-sm font-medium ${plan.featured ? 'bg-emerald-600 hover:bg-emerald-500 text-white' : 'border border-gray-700 text-gray-300 hover:border-gray-500'}`}>
                {plan.cta}
              </Link>
            </div>
          ))}
        </div>

        <p className="mt-12 text-gray-600 text-sm">
          Questions? Email{' '}
          <a href="mailto:buildbysarmad@gmail.com" className="text-gray-400 hover:text-white">
            buildbysarmad@gmail.com
          </a>
        </p>
      </div>
    </div>
  );
}
