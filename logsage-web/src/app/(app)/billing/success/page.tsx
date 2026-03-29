import Link from 'next/link';

export default function BillingSuccessPage() {
  return (
    <div className="min-h-screen bg-gray-950 flex items-center justify-center p-4">
      <div className="text-center">
        <div className="w-16 h-16 bg-emerald-900 rounded-full flex items-center justify-center mx-auto mb-6">
          <span className="text-emerald-400 text-2xl">✓</span>
        </div>
        <h1 className="text-2xl font-bold text-white mb-2">Welcome to Pro!</h1>
        <p className="text-gray-400 mb-8">
          Your account has been upgraded. AI analysis is now available.
        </p>
        <Link href="/analyze"
          className="bg-emerald-600 hover:bg-emerald-500 text-white px-8 py-3 rounded-xl font-medium transition-colors">
          Start analyzing
        </Link>
      </div>
    </div>
  );
}
