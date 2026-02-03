/**
 * Authentication utility functions
 */

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
 * Get current auth token
 */
export function getAuthToken(): string | null {
  return localStorage.getItem('token');
}
