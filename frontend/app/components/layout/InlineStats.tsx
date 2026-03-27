'use client';

import { ReactNode } from 'react';

export interface StatItem {
  label: string;
  value: string | number;
  /** Optional icon */
  icon?: ReactNode;
  /** Optional color accent for the value */
  valueColor?: 'default' | 'teal' | 'violet' | 'gold';
}

interface InlineStatsProps {
  stats: StatItem[];
  className?: string;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
}

/**
 * InlineStats - Stats displayed inline with vertical dividers
 * 
 * Use this instead of a grid of stat cards. Clean, minimal, and scannable.
 */
export function InlineStats({
  stats,
  className = '',
  size = 'md',
}: InlineStatsProps) {
  const sizeConfig = {
    sm: {
      value: 'text-lg font-semibold',
      label: 'text-xs',
      gap: 'gap-6',
      dividerHeight: 'h-6',
    },
    md: {
      value: 'text-2xl font-semibold',
      label: 'text-xs',
      gap: 'gap-8',
      dividerHeight: 'h-8',
    },
    lg: {
      value: 'text-3xl font-bold',
      label: 'text-sm',
      gap: 'gap-10',
      dividerHeight: 'h-10',
    },
  }[size];

  const valueColorClass = {
    default: 'text-zinc-900',
    teal: 'text-[#1D9E75]',
    violet: 'text-[#3C2A8A]',
    gold: 'text-[#EF9F27]',
  };

  return (
    <div className={`flex items-center ${sizeConfig.gap} ${className}`}>
      {stats.map((stat, index) => (
        <div key={stat.label} className="flex items-center gap-8">
          <div className="flex items-center gap-2">
            {stat.icon && (
              <span className="text-zinc-400">{stat.icon}</span>
            )}
            <div>
              <p className={`${sizeConfig.value} ${valueColorClass[stat.valueColor || 'default']}`}>
                {stat.value}
              </p>
              <p className={`${sizeConfig.label} text-zinc-500 uppercase tracking-wide`}>
                {stat.label}
              </p>
            </div>
          </div>
          {index < stats.length - 1 && (
            <div className={`w-px ${sizeConfig.dividerHeight} bg-zinc-200`} />
          )}
        </div>
      ))}
    </div>
  );
}
