import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { accountApi } from '@/lib/api/account';
import type { CurrentUser } from '@/lib/types/account';
import { clearAuthSession, ensureAuthToken } from '@/lib/utils/auth';

/**
 * Centralized authentication state management hook
 * Provides current user data, authentication status, and logout functionality
 */
export function useAuth() {
  const queryClient = useQueryClient();
  const router = useRouter();
  const [hasToken, setHasToken] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);

  // Check token on client-side only to avoid SSR issues
  useEffect(() => {
    let isMounted = true;

    const syncAuthState = async () => {
      const token = await ensureAuthToken();
      if (!isMounted) {
        return;
      }

      setHasToken(!!token);
      setIsInitialized(true);

      if (token) {
        queryClient.invalidateQueries({ queryKey: ['currentUser'] });
        queryClient.invalidateQueries({ queryKey: ['credits'] });
        queryClient.invalidateQueries({ queryKey: ['unread-count'] });
      } else {
        queryClient.removeQueries({ queryKey: ['currentUser'] });
        queryClient.removeQueries({ queryKey: ['credits'] });
        queryClient.removeQueries({ queryKey: ['unread-count'] });
      }
    };

    const initializeAuth = async () => {
      const token = await ensureAuthToken();
      if (!isMounted) {
        return;
      }

      setHasToken(!!token);
      setIsInitialized(true);
    };
    
    initializeAuth();

    const handleAuthChange = () => {
      void syncAuthState();
    };
    
    // Listen for storage changes (cross-tab) and custom events
    window.addEventListener('storage', handleAuthChange);
    window.addEventListener('auth-change', handleAuthChange);
    
    return () => {
      isMounted = false;
      window.removeEventListener('storage', handleAuthChange);
      window.removeEventListener('auth-change', handleAuthChange);
    };
  }, [queryClient]);

  // Fetch current user data
  const { data: user, isLoading, error } = useQuery<CurrentUser>({
    queryKey: ['currentUser'],
    queryFn: accountApi.getMe,
    enabled: hasToken, // Only fetch if token exists
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: false, // Don't retry on 401
  });

  // Logout function
  const logout = async () => {
    try {
      await accountApi.logout();
      // Clear React Query cache
      clearAuthSession();
      queryClient.clear();
      router.push('/login');
    } catch (error) {
      console.error('Logout failed:', error);
      // Force logout client-side even if API call fails
      clearAuthSession();
      queryClient.clear();
      router.push('/login');
    }
  };

  return {
    user,
    isLoading: !isInitialized || (hasToken && isLoading),
    isAuthenticated: !!user && hasToken,
    logout,
    error,
  };
}
