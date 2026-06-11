'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2 } from 'lucide-react';
import { ensureAuthenticatedUser } from '@/lib/utils/auth';

/**
 * Dashboard redirect page
 * Redirects authenticated users to /marketplace
 * This makes /marketplace the default landing page for logged-in users
 */
export default function DashboardPage() {
  const router = useRouter();

  useEffect(() => {
    let isMounted = true;

    const redirectAfterSessionCheck = async () => {
      const user = await ensureAuthenticatedUser();
      if (!isMounted) {
        return;
      }

      if (!user) {
        router.replace('/login');
        return;
      }

      router.replace(user.isProfileComplete ? '/marketplace' : '/onboarding');
    };

    redirectAfterSessionCheck();

    return () => {
      isMounted = false;
    };
  }, [router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#FAFAFA]">
      <div className="text-center">
        <Loader2 className="w-12 h-12 animate-spin text-[#1D9E75] mx-auto mb-4" />
        <p className="text-zinc-500">Redirecting to marketplace...</p>
      </div>
    </div>
  );
}
