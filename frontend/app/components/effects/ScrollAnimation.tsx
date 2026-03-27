"use client";

import { cn } from "@/lib/utils";
import { motion } from "framer-motion";
import React from "react";

type Direction = "up" | "down" | "left" | "right";

const generateVariants = (direction: Direction) => {
  const axis = direction === "left" || direction === "right" ? "x" : "y";
  const value = direction === "right" || direction === "down" ? 100 : -100;

  return {
    hidden: { filter: "blur(10px)", opacity: 0, [axis]: value },
    visible: {
      filter: "blur(0px)",
      opacity: 1,
      [axis]: 0,
      transition: {
        duration: 0.5,
        ease: "easeOut" as const,
      },
    },
  };
};

const defaultViewport = { amount: 0.3, margin: "0px 0px -200px 0px" };

interface ScrollElementProps {
  children: React.ReactNode;
  className?: string;
  viewport?: {
    amount?: number;
    margin?: string;
    once?: boolean;
  };
  delay?: number;
  direction?: Direction;
}

export default function ScrollElement({
  children,
  className,
  viewport = defaultViewport,
  delay = 0,
  direction = "down",
}: ScrollElementProps) {
  const baseVariants = generateVariants(direction);

  return (
    <motion.div
      whileInView="visible"
      initial="hidden"
      variants={{
        hidden: baseVariants.hidden,
        visible: {
          ...baseVariants.visible,
          transition: {
            ...baseVariants.visible.transition,
            delay,
          },
        },
      }}
      viewport={viewport}
      className={cn(className)}
    >
      {children}
    </motion.div>
  );
}

export { ScrollElement };
