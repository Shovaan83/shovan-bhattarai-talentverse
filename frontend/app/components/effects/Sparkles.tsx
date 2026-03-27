"use client";

import { useEffect, useRef, useState, useCallback } from "react";

interface SparkleType {
  id: string;
  x: number;
  y: number;
  size: number;
  opacity: number;
  delay: number;
}

function generateSparkle(): SparkleType {
  return {
    id: Math.random().toString(36).substring(2, 9),
    x: Math.random() * 100,
    y: Math.random() * 100,
    size: Math.random() * 3 + 1,
    opacity: Math.random() * 0.7 + 0.3,
    delay: Math.random() * 2,
  };
}

interface SparklesProps {
  children?: React.ReactNode;
  className?: string;
  color?: string;
  count?: number;
}

/**
 * Lightweight Sparkles component using CSS animations.
 * Does NOT require @tsparticles — pure CSS implementation.
 */
export function Sparkles({
  children,
  className = "",
  color = "#5DCAA5",
  count = 8,
}: SparklesProps) {
  const [sparkles, setSparkles] = useState<SparkleType[]>([]);

  const regenerate = useCallback(() => {
    setSparkles(Array.from({ length: count }, () => generateSparkle()));
  }, [count]);

  useEffect(() => {
    regenerate();
    const interval = setInterval(regenerate, 3000);
    return () => clearInterval(interval);
  }, [regenerate]);

  return (
    <span className={`relative inline-block ${className}`}>
      {sparkles.map((sparkle) => (
        <svg
          key={sparkle.id}
          className="absolute pointer-events-none animate-sparkle-spin"
          style={{
            left: `${sparkle.x}%`,
            top: `${sparkle.y}%`,
            width: `${sparkle.size * 4}px`,
            height: `${sparkle.size * 4}px`,
            animationDelay: `${sparkle.delay}s`,
            opacity: sparkle.opacity,
          }}
          viewBox="0 0 24 24"
          fill="none"
        >
          <path
            d="M12 0L14.59 9.41L24 12L14.59 14.59L12 24L9.41 14.59L0 12L9.41 9.41L12 0Z"
            fill={color}
          />
        </svg>
      ))}
      {children}
    </span>
  );
}
