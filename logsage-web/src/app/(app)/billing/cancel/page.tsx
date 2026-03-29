import Link from 'next/link';

export default function BillingCancelPage() {
  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-2xl font-bold text-white mb-2">Checkout cancelled</h1>
        <p className="text-gray-400 mb-8">No changes were made to your account.</p>
        <div className="flex gap-4 justify-center">
          <Link href="/analyze"
            className="border border-gray-700 text-gray-300 px-6 py-2.5 rounded-xl text-sm hover:border-gray-500 transition-colors">
            Back to analyze
          </Link>
          <Link href="/billing/upgrade"
            className="bg-emerald-600 hover:bg-emerald-500 text-white px-6 py-2.5 rounded-xl text-sm transition-colors">
            Try again
          </Link>
        </div>
      </div>
    </div>
  );
}
