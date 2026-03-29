'use client';

import { useState } from 'react';
import { billingApi } from '@/lib/api';
import { useAuthStore } from '@/lib/auth';

const plans = [
  {
    id: 'pro',
    name: 'Pro',
    price: '$19/mo',
    priceId: process.env.NEXT_PUBLIC_PADDLE_PRO_PRICE_ID ?? '',
    features: ['Unlimited analyses', '50k lines', 'AI root cause', 'Session history'],
  },
  {
    id: 'team',
    name: 'Team',
    price: '$49/mo',
    priceId: process.env.NEXT_PUBLIC_PADDLE_TEAM_PRICE_ID ?? '',
    features: ['Everything in Pro', 'API access', '5 seats', 'Priority support'],
  },
];

export default function UpgradePage() {
  const user = useAuthStore((s) => s.user);
  const [loading, setLoading] = useState<string | null>(null);

  const handleUpgrade = async (priceId: string, planId: string) => {
    setLoading(planId);
    try {
      const { data } = await billingApi.checkout(priceId);
      window.location.href = data.checkoutUrl;
    } catch {
      alert('Could not start checkout. Please try again.');
    } finally {
      setLoading(null);
    }
  };

  if (user?.plan !== 'free') {
    return (
      <div className="min-h-screen bg-gray-950 flex items-center justify-center">
        <div className="text-center">
          <p className="text-white text-lg mb-2">You are already on the {user?.plan} plan.</p>
          <a href="/analyze" className="text-emerald-400 hover:text-emerald-300 text-sm">
            Go to analyze
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-950 px-4 py-16">
      <div className="max-w-2xl mx-auto text-center">
        <h1 className="text-3xl font-bold text-white mb-4">Upgrade LogSage</h1>
        <p className="text-gray-400 mb-12">Get unlimited analyses and AI root cause suggestions</p>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
          {plans.map((plan) => (
            <div key={plan.id} className="bg-gray-900 border border-gray-800 rounded-xl p-6 text-left">
              <h2 className="text-xl font-bold text-white">{plan.name}</h2>
              <p className="text-3xl font-bold text-white mt-1 mb-4">{plan.price}</p>
              <ul className="space-y-2 mb-6">
                {plan.features.map((f) => (
                  <li key={f} className="text-sm text-gray-300 flex gap-2">
                    <span className="text-emerald-400">✓</span>{f}
                  </li>
                ))}
              </ul>
              <button
                onClick={() => handleUpgrade(plan.priceId, plan.id)}
                disabled={!!loading}
                className="w-full bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50
                           text-white font-medium py-2.5 rounded-lg transition-colors"
              >
                {loading === plan.id ? 'Loading...' : `Get ${plan.name}`}
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
