'use client';

import { useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import api from '@/lib/axios';
import { ensureAuthToken } from '@/lib/utils/auth';

/**
 * Middleware to enforce 2FA setup for email/password users
 * Redirects to /setup-2fa if:
 * - User is authenticated (has token)
 * - User has a password (not OAuth user)
 * - User hasn't completed 2FA setup (IsTwoFactorSetupComplete = false)
 * - User is not already on /setup-2fa or auth pages
 */
export function use2FAEnforcement() {
  const router = useRouter();
  const pathname = usePathname();
  
  useEffect(() => {
    const enforce2FA = async () => {
      // Skip enforcement on auth pages and onboarding
      const authPages = ['/login', '/register', '/setup-2fa', '/oauth-callback', '/onboarding'];
      if (authPages.some(page => pathname?.startsWith(page))) {
        return;
      }
      
      const token = await ensureAuthToken();
      if (!token) {
        // Not authenticated, skip enforcement
        return;
      }
      
      try {
        // Check user's 2FA status
        const response = await api.get('/account/me');
        
        if (response.data.success && response.data.data) {
          const user = response.data.data;
          
          // Enforce 2FA setup for email/password users who haven't completed it
          // ONLY if profile is complete (don't interrupt onboarding flow)
          if (user.hasPassword && !user.isTwoFactorSetupComplete && user.isProfileComplete) {
            console.log('2FA setup required for email/password user, redirecting...');
            router.push('/setup-2fa');
          }
        }
      } catch (error) {
        console.error('Error checking 2FA status:', error);
        // Don't block user on error
      }
    };
    
    enforce2FA();
  }, [pathname, router]);
}

/**
 * Higher-order component to wrap pages that require 2FA enforcement
 */
export function with2FAEnforcement<P extends object>(
  Component: React.ComponentType<P>
): React.ComponentType<P> {
  return function WrappedComponent(props: P) {
    use2FAEnforcement();
    return <Component {...props} />;
  };
}
