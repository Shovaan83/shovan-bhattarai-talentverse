import axios from 'axios';

/**
 * Authentication utility functions
 */

interface RefreshTokenResponse {
  success: boolean;
  data: string;
}

interface ServiceResponse<T> {
  success: boolean;
  data: T;
}

interface AuthUserState {
  isProfileComplete: boolean;
}

/**
 * Store auth token and dispatch auth-change event
 * Use this instead of directly calling localStorage.setItem('token', ...)
 */
export function setAuthToken(token: string) {
  localStorage.setItem('token', token);
  // Dispatch custom event to notify all components of auth state change
  window.dispatchEvent(new Event('auth-change'));
}

/**
 * Remove auth token and dispatch auth-change event
 * Use this instead of directly calling localStorage.removeItem('token')
 */
export function clearAuthToken() {
  localStorage.removeItem('token');
  // Dispatch custom event to notify all components of auth state change
  window.dispatchEvent(new Event('auth-change'));
}

/**
 * Clear all local auth state. The httpOnly refresh cookie is cleared by logout.
 */
export function clearAuthSession() {
  localStorage.removeItem('token');
  localStorage.removeItem('rememberMe');
  localStorage.removeItem('userEmail');
  window.dispatchEvent(new Event('auth-change'));
}

/**
 * Get current auth token
 */
export function getAuthToken(): string | null {
  return localStorage.getItem('token');
}

/**
 * Refresh the short-lived access token from the httpOnly refresh-token cookie.
 * Returns null when the cookie is missing, expired, or invalid.
 */
export async function refreshAccessToken(): Promise<string | null> {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    const response = await axios.post<RefreshTokenResponse>(
      '/account/refresh',
      undefined,
      {
        baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5249/api',
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );

    if (!response.data.success || !response.data.data) {
      localStorage.removeItem('token');
      return null;
    }

    setAuthToken(response.data.data);
    return response.data.data;
  } catch {
    localStorage.removeItem('token');
    return null;
  }
}

/**
 * Ensure an access token exists, restoring it from the refresh cookie when needed.
 */
export async function ensureAuthToken(): Promise<string | null> {
  if (typeof window === 'undefined') {
    return null;
  }

  const existingToken = getAuthToken();
  if (existingToken) {
    return existingToken;
  }

  return refreshAccessToken();
}

/**
 * Ensure the user has a valid session and return profile completion state.
 * Protected app surfaces use this to force incomplete OAuth/email users through onboarding.
 */
export async function ensureAuthenticatedUser(): Promise<AuthUserState | null> {
  const token = await ensureAuthToken();
  if (!token) {
    return null;
  }

  try {
    const response = await axios.get<ServiceResponse<AuthUserState>>(
      '/account/me',
      {
        baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5249/api',
        withCredentials: true,
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (!response.data.success || !response.data.data) {
      return null;
    }

    return response.data.data;
  } catch {
    return null;
  }
}
