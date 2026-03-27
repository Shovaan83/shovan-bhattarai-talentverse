'use client';

import { ReactNode } from 'react';
import { ArrowLeft } from 'lucide-react';
import Link from 'next/link';

interface PageHeaderProps {
  title: string;
  subtitle?: string;
  /** Back link URL */
  backHref?: string;
  /** Right-side actions */
  actions?: ReactNode;
  /** Children appear below title (e.g., inline stats) */
  children?: ReactNode;
  className?: string;
}

/**
 * PageHeader - Consistent page header with optional back navigation
 * 
 * Use at the top of every page for consistency. Includes back button,
 * title, subtitle, and optional actions.
 */
export function PageHeader({
  title,
  subtitle,
  backHref,
  actions,
  children,
  className = '',
}: PageHeaderProps) {
  return (
    <header className={`border-b border-zinc-200 bg-white sticky top-16 z-10 ${className}`}>
      <div className="max-w-7xl mx-auto px-6 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            {backHref && (
              <Link
                href={backHref}
                className="p-2 rounded-lg bg-zinc-100 hover:bg-zinc-200 text-zinc-600 transition-colors"
              >
                <ArrowLeft className="w-5 h-5" />
              </Link>
            )}
            <div>
              <h1 className="text-2xl font-display font-bold text-zinc-900">
                {title}
              </h1>
              {subtitle && (
                <p className="text-sm text-zinc-500 mt-0.5">{subtitle}</p>
              )}
            </div>
          </div>
          {actions && (
            <div className="flex items-center gap-3">
              {actions}
            </div>
          )}
        </div>
        {children && (
          <div className="mt-4">
            {children}
          </div>
        )}
      </div>
    </header>
  );
}
