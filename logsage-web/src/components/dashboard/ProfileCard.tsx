'use client';

import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { authApi } from '@/lib/api';
import type { User } from '@/lib/types';
import { useReducedMotion, motionTransitions } from '@/lib/motion';

interface Props {
  user: User;
}

export function ProfileCard({ user }: Props) {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const shouldReduceMotion = useReducedMotion();

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);

    if (newPassword !== confirmPassword) {
      setMessage({ type: 'error', text: 'New passwords do not match' });
      return;
    }

    if (newPassword.length < 8) {
      setMessage({ type: 'error', text: 'Password must be at least 8 characters' });
      return;
    }

    setLoading(true);
    try {
      await authApi.changePassword(currentPassword, newPassword);
      setMessage({ type: 'success', text: 'Password changed successfully' });
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: unknown) {
      const errorMsg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
        || 'Failed to change password';
      setMessage({ type: 'error', text: errorMsg });
    } finally {
      setLoading(false);
    }
  };

  const accountAge = user.createdAt
    ? new Date(user.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
    : 'Unknown';

  return (
    <motion.div
      className="bg-gray-900 border border-gray-800 rounded-xl p-6"
      initial={shouldReduceMotion ? false : { opacity: 0, y: 20 }}
      animate={shouldReduceMotion ? false : { opacity: 1, y: 0 }}
      transition={{ delay: 0.1, ...motionTransitions.smooth }}
    >
      <h2 className="text-lg font-semibold text-white mb-6">Profile</h2>

      <div className="space-y-4 mb-6">
        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, x: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
          transition={{ delay: 0.2, ...motionTransitions.smooth }}
        >
          <label className="block text-xs font-medium text-gray-500 mb-1">Email</label>
          <p className="text-sm text-white">{user.email}</p>
        </motion.div>

        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, x: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
          transition={{ delay: 0.3, ...motionTransitions.smooth }}
        >
          <label className="block text-xs font-medium text-gray-500 mb-1">Account created</label>
          <p className="text-sm text-white">{accountAge}</p>
        </motion.div>

        <motion.div
          initial={shouldReduceMotion ? false : { opacity: 0, x: -20 }}
          animate={shouldReduceMotion ? false : { opacity: 1, x: 0 }}
          transition={{ delay: 0.4, ...motionTransitions.smooth }}
        >
          <label className="block text-xs font-medium text-gray-500 mb-1">Current plan</label>
          <p className="text-sm text-white capitalize">{user.plan}</p>
        </motion.div>
      </div>

      <hr className="border-gray-800 mb-6" />

      <form onSubmit={handleChangePassword} className="space-y-4">
        <h3 className="text-sm font-semibold text-white mb-3">Change password</h3>

        <div>
          <label htmlFor="current" className="block text-xs font-medium text-gray-500 mb-1">
            Current password
          </label>
          <input
            id="current"
            type="password"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            className="w-full bg-gray-800 text-gray-200 text-sm border border-gray-700
                       rounded-lg px-3 py-2 focus:outline-none focus:border-emerald-500
                       transition-all duration-300 focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
            required
          />
        </div>

        <div>
          <label htmlFor="new" className="block text-xs font-medium text-gray-500 mb-1">
            New password
          </label>
          <input
            id="new"
            type="password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            className="w-full bg-gray-800 text-gray-200 text-sm border border-gray-700
                       rounded-lg px-3 py-2 focus:outline-none focus:border-emerald-500
                       transition-all duration-300 focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
            required
          />
        </div>

        <div>
          <label htmlFor="confirm" className="block text-xs font-medium text-gray-500 mb-1">
            Confirm new password
          </label>
          <input
            id="confirm"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            className="w-full bg-gray-800 text-gray-200 text-sm border border-gray-700
                       rounded-lg px-3 py-2 focus:outline-none focus:border-emerald-500
                       transition-all duration-300 focus:shadow-[0_0_0_3px_rgba(52,211,153,0.1)]"
            required
          />
        </div>

        <AnimatePresence mode="wait">
          {message && (
            <motion.p
              key={message.text}
              className={`text-sm ${message.type === 'error' ? 'text-red-400' : 'text-emerald-400'}`}
              initial={shouldReduceMotion ? undefined : { opacity: 0, x: -10 }}
              animate={shouldReduceMotion ? undefined : { opacity: 1, x: 0 }}
              exit={shouldReduceMotion ? undefined : { opacity: 0, x: 10 }}
              transition={motionTransitions.smooth}
            >
              {message.text}
            </motion.p>
          )}
        </AnimatePresence>

        <motion.button
          type="submit"
          disabled={loading}
          className="relative w-full bg-emerald-600 hover:bg-emerald-500 disabled:opacity-50
                     disabled:cursor-not-allowed text-white font-medium py-2 rounded-lg
                     transition-all text-sm overflow-hidden
                     shadow-[0_0_20px_rgba(52,211,153,0.3)]"
          whileHover={shouldReduceMotion || loading ? {} : {
            scale: 1.02,
            boxShadow: '0 0 30px rgba(52,211,153,0.5)',
          }}
          whileTap={shouldReduceMotion || loading ? {} : { scale: 0.98 }}
        >
          <span className="relative z-10">
            {loading ? 'Changing...' : 'Change password'}
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
      </form>
    </motion.div>
  );
}
