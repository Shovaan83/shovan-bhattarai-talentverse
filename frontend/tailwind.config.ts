import type { Config } from "tailwindcss";

const config: Config = {
  theme: {
    extend: {
      colors: {
        brand: {
          violet: {
            900: "var(--color-violet-dark)",
            600: "var(--color-violet)",
            500: "var(--color-violet-mid)",
            100: "var(--color-violet-light)",
            50: "var(--color-page-bg)",
          },
          teal: {
            700: "var(--color-teal-dark)",
            500: "var(--color-teal)",
            300: "var(--color-teal-light)",
            50: "var(--color-teal-mint)",
          },
          gold: {
            800: "var(--color-gold-ink)",
            600: "var(--color-gold-dark)",
            500: "var(--color-gold)",
            50: "var(--color-gold-light)",
          },
        },
      },
      fontFamily: {
        display: ["var(--font-display)", "sans-serif"],
        body: ["var(--font-body)", "sans-serif"],
        mono: ["JetBrains Mono", "monospace"],
      },
    },
  },
};

export default config;
