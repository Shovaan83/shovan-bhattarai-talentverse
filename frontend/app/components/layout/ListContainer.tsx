'use client';

import { ReactNode } from 'react';

interface ListContainerProps {
  children: ReactNode;
  className?: string;
  /** Optional header content (title, actions) */
  header?: ReactNode;
}

/**
 * ListContainer - A container for list items with dividers
 * 
 * Use this instead of a grid of cards. Place ListRow components inside.
 * Creates a single white container with divided rows.
 */
export function ListContainer({
  children,
  className = '',
  header,
}: ListContainerProps) {
  return (
    <div className={`bg-white border border-zinc-200 rounded-xl overflow-hidden ${className}`}>
      {header && (
        <div className="px-5 py-4 border-b border-zinc-200 bg-zinc-50/50">
          {header}
        </div>
      )}
      <div className="divide-y divide-zinc-100">
        {children}
      </div>
    </div>
  );
}

interface ListRowProps {
  children: ReactNode;
  className?: string;
  onClick?: () => void;
  /** Show hover state */
  interactive?: boolean;
  /** Padding size */
  padding?: 'sm' | 'md' | 'lg';
}

/**
 * ListRow - A single row item within a ListContainer
 * 
 * Use for each item in a list (proposals, users, transactions, etc.)
 */
export function ListRow({
  children,
  className = '',
  onClick,
  interactive = true,
  padding = 'md',
}: ListRowProps) {
  const paddingClass = {
    sm: 'px-4 py-3',
    md: 'px-5 py-4',
    lg: 'px-6 py-5',
  }[padding];

  const interactiveClass = interactive 
    ? 'hover:bg-zinc-50 transition-colors cursor-pointer' 
    : '';

  return (
    <div
      className={`${paddingClass} ${interactiveClass} ${className}`}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={onClick ? (e) => e.key === 'Enter' && onClick() : undefined}
    >
      {children}
    </div>
  );
}
