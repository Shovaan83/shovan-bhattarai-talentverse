"use client";

import { motion } from "framer-motion";

interface MeshGradientProps {
  className?: string;
  colors?: string[];
}

/**
 * Animated mesh gradient background.
 * Uses CSS radial-gradients with Framer Motion animated blobs.
 * Default colors: violet + teal brand palette.
 */
export function MeshGradient({
  className = "",
  colors = ["#3C2A8A", "#1D9E75", "#534AB7", "#0F6E56"],
}: MeshGradientProps) {
  return (
    <div className={`absolute inset-0 overflow-hidden ${className}`}>
      {colors.map((color, i) => (
        <motion.div
          key={i}
          className="absolute rounded-full blur-3xl"
          style={{
            background: `radial-gradient(circle, ${color}80 0%, transparent 70%)`,
            width: "40%",
            height: "40%",
          }}
          initial={{
            x: `${20 + i * 20}%`,
            y: `${15 + i * 15}%`,
          }}
          animate={{
            x: [
              `${20 + i * 20}%`,
              `${30 + ((i + 1) * 15) % 50}%`,
              `${10 + ((i + 2) * 18) % 60}%`,
              `${20 + i * 20}%`,
            ],
            y: [
              `${15 + i * 15}%`,
              `${25 + ((i + 2) * 12) % 40}%`,
              `${5 + ((i + 1) * 20) % 50}%`,
              `${15 + i * 15}%`,
            ],
          }}
          transition={{
            duration: 12 + i * 3,
            repeat: Infinity,
            ease: "easeInOut",
          }}
        />
      ))}
    </div>
  );
}
