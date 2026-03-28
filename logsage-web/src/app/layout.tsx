import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'LogSage — AI Log Analysis',
  description:
    'Upload your logs. Get errors grouped and root causes explained in seconds. Free. No signup required.',
  keywords: ['log analysis', 'error debugging', 'AI', '.NET', 'developer tools'],
  openGraph: {
    title: 'LogSage — AI Log Analysis',
    description: 'Paste your logs. Know what broke and why. In 10 seconds.',
    url: 'https://LogSage.io',
    siteName: 'LogSage',
    type: 'website',
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="dark">
      <body className={`${inter.className} bg-gray-950 text-gray-100 antialiased`}>
        {children}
      </body>
    </html>
  );
}
