'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2 } from 'lucide-react';

/**
 * Dashboard redirect page
 * Redirects authenticated users to /marketplace
 * This makes /marketplace the default landing page for logged-in users
 */
export default function DashboardPage() {
  const router = useRouter();

  useEffect(() => {
    // Check if user is authenticated
    const token = localStorage.getItem('token');
    
    if (token) {
      // Redirect to marketplace as the main dashboard
      router.replace('/marketplace');
    } else {
      // Not authenticated, redirect to login
      router.replace('/login');
    }
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
