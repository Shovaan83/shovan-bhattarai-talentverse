'use client';

import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { ensureAuthenticatedUser } from '@/lib/utils/auth';

export default function MarketplaceLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);

  useEffect(() => {
    let isMounted = true;

    const verifySession = async () => {
      const user = await ensureAuthenticatedUser();
      if (!isMounted) {
        return;
      }

      if (!user) {
        router.push('/login');
        return;
      }

      if (!user.isProfileComplete) {
        router.push('/onboarding');
        return;
      }

      setIsAuthenticated(true);
    };

    verifySession();

    return () => {
      isMounted = false;
    };
  }, [router]);

  if (isAuthenticated === null) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-[#1D9E75]"></div>
      </div>
    );
  }

  return <>{children}</>;
}
