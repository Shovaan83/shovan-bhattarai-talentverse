'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2 } from 'lucide-react';

export default function OAuthCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token = searchParams.get('token');
    const isNewUser = searchParams.get('isNewUser') === 'true';
    const requiresOnboarding = searchParams.get('requiresOnboarding') === 'true';

    // Handle error from backend
    if (!token) {
      setError('Authentication failed. Please try again.');
      setTimeout(() => router.push('/login'), 3000);
      return;
    }

    // ⭐ Store access token in localStorage (hybrid approach)
    // Refresh token is already in httpOnly cookie from backend
    localStorage.setItem('token', token);

    // Redirect based on user state
    if (requiresOnboarding) {
      // New user or incomplete profile - redirect to onboarding
      router.push('/onboarding');
    } else {
      // Existing user with complete profile - redirect to dashboard
      router.push('/dashboard');
    }
  }, [searchParams, router]);

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="bg-white p-8 rounded-xl shadow-md max-w-md w-full">
          <div className="text-center">
            <div className="text-red-500 text-5xl mb-4">✗</div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Authentication Failed</h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <p className="text-sm text-gray-500">Redirecting to login...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="bg-white p-8 rounded-xl shadow-md max-w-md w-full">
        <div className="text-center">
          <Loader2 className="w-12 h-12 animate-spin text-emerald-600 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Completing Sign In</h2>
          <p className="text-gray-600">Please wait while we finish setting up your account...</p>
        </div>
      </div>
    </div>
  );
}
