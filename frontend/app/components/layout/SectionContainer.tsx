import { ReactNode } from 'react';

interface SectionContainerProps {
  children: ReactNode;
  className?: string;
  /** Add top/bottom border (white band effect) */
  bordered?: boolean;
  /** Background color - defaults to white */
  bg?: 'white' | 'transparent' | 'zinc-50';
}

/**
 * SectionContainer - A full-width section band for page content
 * 
 * Use this instead of individual cards to create horizontal bands of content.
 * The card-light design uses these bands to group related content.
 */
export function SectionContainer({
  children,
  className = '',
  bordered = true,
  bg = 'white',
}: SectionContainerProps) {
  const bgClass = {
    white: 'bg-white',
    transparent: 'bg-transparent',
    'zinc-50': 'bg-zinc-50',
  }[bg];

  const borderClass = bordered ? 'border-y border-zinc-200' : '';

  return (
    <section className={`${bgClass} ${borderClass} ${className}`}>
      <div className="max-w-7xl mx-auto px-6 py-6">
        {children}
      </div>
    </section>
  );
}
