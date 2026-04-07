import { AppHeader } from '@/components/layout/AppHeader';

export default function AppLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen">
      <AppHeader />
      <div className="pt-[49px]">{children}</div>
    </div>
  );
}
