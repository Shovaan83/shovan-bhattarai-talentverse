'use client';

import { use2FAEnforcement } from '@/lib/hooks/use2FAEnforcement';

/**
 * Client component that enforces 2FA setup globally
 * Add this to the root layout to protect all pages
 */
export function GlobalEnforcement() {
  use2FAEnforcement();
  return null;
}
