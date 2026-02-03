import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import { accountApi } from '@/lib/api/account';
import type { CurrentUser } from '@/lib/types/account';

/**
 * Centralized authentication state management hook
 * Provides current user data, authentication status, and logout functionality
 */
export function useAuth() {
  const queryClient = useQueryClient();
  const router = useRouter();
  const [hasToken, setHasToken] = useState(false);

  // Check token on client-side only to avoid SSR issues
  useEffect(() => {
    const checkToken = () => {
      setHasToken(!!localStorage.getItem('token'));
    };
    
    checkToken();
    
    // Listen for storage changes (cross-tab) and custom events
    window.addEventListener('storage', checkToken);
    window.addEventListener('auth-change', checkToken);
    
    return () => {
      window.removeEventListener('storage', checkToken);
      window.removeEventListener('auth-change', checkToken);
    };
  }, []);

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
      localStorage.removeItem('token');
      queryClient.clear();
      // Dispatch auth change event
      window.dispatchEvent(new Event('auth-change'));
      router.push('/login');
    } catch (error) {
      console.error('Logout failed:', error);
      // Force logout client-side even if API call fails
      localStorage.removeItem('token');
      queryClient.clear();
      // Dispatch auth change event
      window.dispatchEvent(new Event('auth-change'));
      router.push('/login');
    }
  };

  return {
    user,
    isLoading,
    isAuthenticated: !!user && hasToken,
    logout,
    error,
  };
}
