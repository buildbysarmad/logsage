import type { Metadata } from 'next';
import './globals.css';
import { PageTransition } from '@/components/motion/PageTransition';

export const metadata: Metadata = {
  title: 'LogSage — Instant Log Analysis',
  description:
    'Upload your logs. Get errors grouped and root causes explained in seconds. Free. No signup required.',
  keywords: ['log analysis', 'error debugging', 'log parser', '.NET', 'developer tools'],
  icons: {
    icon: '/icon.svg',
    apple: '/icon.svg',
  },
  openGraph: {
    title: 'LogSage — Instant Log Analysis',
    description: 'Paste your logs. Know what broke and why. In seconds.',
    url: 'https://logsage.dev',
    siteName: 'LogSage',
    type: 'website',
  },
  themeColor: '#065f46',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="dark">
      <body className="antialiased">
        <PageTransition>{children}</PageTransition>
      </body>
    </html>
  );
}
