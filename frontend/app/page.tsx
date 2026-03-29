"use client";

import React from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { ReactLenis } from "lenis/react";
import {
  ArrowRight,
  Search,
  ArrowRightLeft,
  Star,
  Shield,
  Coins,
  Users,
  Zap,
  Twitter,
  Instagram,
  Linkedin,
  Github,
} from "lucide-react";
import { MeshGradient } from "./components/effects/MeshGradient";
import { Marquee } from "./components/effects/Marquee";
import { AnimatedNumber } from "./components/ui/AnimatedNumber";

/* ─────────────────── Data ─────────────────── */

const skillCategories = [
  "UI/UX Design",
  "Web Development",
  "Guitar Lessons",
  "Photography",
  "Video Editing",
  "Data Science",
  "Content Writing",
  "Digital Marketing",
  "Mobile App Dev",
  "Illustration",
  "Machine Learning",
  "Music Production",
  "3D Modeling",
  "SEO",
  "Public Speaking",
];

const steps = [
  {
    icon: Search,
    title: "Find a Skill Match",
    description:
      "Browse the marketplace to find someone whose skills complement yours. Filter by category, location, or proficiency.",
  },
  {
    icon: ArrowRightLeft,
    title: "Send a Proposal",
    description:
      "Propose a skill swap by selecting what you offer and what you want. Add a message to introduce yourself.",
  },
  {
    icon: Star,
    title: "Swap & Grow",
    description:
      "Complete the exchange, leave reviews, earn badges, and build your reputation as a trusted skill swapper.",
  },
];

const features = [
  {
    icon: Shield,
    title: "Verified Profiles",
    description:
      "Every user can verify their skills through document uploads. Trust is built into the platform.",
  },
  {
    icon: Coins,
    title: "Swap Credits",
    description:
      "Earn credits for completed swaps. Use them to unlock premium features or balance unequal exchanges.",
  },
  {
    icon: Users,
    title: "Community First",
    description:
      "Real-time messaging, proposal tracking, and leaderboards keep the community engaged and growing.",
  },
  {
    icon: Zap,
    title: "Smart Matching",
    description:
      "Our marketplace surfaces the best matches based on your skills, location, and swap history.",
  },
];

const stats = [
  { label: "Skills listed", value: 2400 },
  { label: "Swaps completed", value: 830 },
  { label: "Active members", value: 1200 },
];

const footerLinks = {
  Platform: [
    "Browse Skills",
    "How it Works",
    "Swap Credits",
    "Success Stories",
  ],
  Company: ["About Us", "Careers", "Blog", "Contact"],
};

/* ─────────────────── Landing ─────────────────── */

export default function LandingPage() {
  return (
    <ReactLenis root>
      <main className="bg-zinc-900">
        {/* ━━━━━━ STICKY WRAPPER: sections stack on top of each other ━━━━━━ */}
        <div className="wrapper">
          {/* ── Section 1 · Hero ── */}
          <section className="relative min-h-screen w-full grid place-content-center sticky top-0 overflow-hidden bg-zinc-900">
            <MeshGradient
              className="absolute inset-0 opacity-40"
              colors={["#3C2A8A", "#1D9E75", "#534AB7", "#0F6E56"]}
            />
            {/* Subtle grid overlay */}
            <div className="absolute inset-0 bg-[linear-gradient(to_right,#ffffff08_1px,transparent_1px),linear-gradient(to_bottom,#ffffff08_1px,transparent_1px)] bg-[size:54px_54px] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_100%)]" />

            <div className="relative z-10 max-w-5xl mx-auto px-6 text-center py-20">
              {/* Pill badge */}
              <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
                className="inline-flex items-center gap-2 bg-[#1D9E75]/10 border border-[#1D9E75]/30 rounded-full px-4 py-1.5 mb-8"
              >
                <span className="text-[#5DCAA5] text-sm font-medium">
                  Skill exchange, reimagined
                </span>
              </motion.div>

              <motion.h1
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.6, delay: 0.1 }}
                className="font-display text-5xl md:text-7xl lg:text-8xl font-bold text-white leading-tight mb-6"
              >
                Trade skills.
                <br />
                <span className="text-[#5DCAA5]">Grow together.</span>
              </motion.h1>

              <motion.p
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.6, delay: 0.2 }}
                className="text-lg md:text-xl text-white/60 max-w-xl mx-auto mb-10"
              >
                Exchange what you know for what you want to learn. No money.
                Just skills.
              </motion.p>

              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.6, delay: 0.3 }}
                className="flex items-center justify-center gap-4 flex-col sm:flex-row"
              >
                <Link href="/register">
                  <button className="bg-[#1D9E75] text-white font-semibold px-8 py-3.5 rounded-xl hover:bg-[#0F6E56] transition-colors text-base active:scale-[0.98]">
                    Start swapping
                  </button>
                </Link>
                <Link href="/marketplace">
                  <button className="text-white/60 hover:text-white text-sm flex items-center gap-1.5 transition-colors px-4 py-3">
                    Browse skills <ArrowRight className="w-4 h-4" />
                  </button>
                </Link>
              </motion.div>

              {/* Social proof */}
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.6, delay: 0.5 }}
                className="mt-14 flex items-center justify-center gap-3 text-sm text-white/30"
              >
                <div className="flex -space-x-2">
                  {[1, 2, 3, 4].map((i) => (
                    <div
                      key={i}
                      className="w-8 h-8 rounded-full border-2 border-zinc-900 bg-zinc-700 flex items-center justify-center"
                    >
                      <span className="text-xs font-medium text-[#5DCAA5]">
                        {String.fromCharCode(64 + i)}
                      </span>
                    </div>
                  ))}
                </div>
                <p>Trusted by 1,000+ creators</p>
              </motion.div>
            </div>
          </section>

          {/* ── Section 2 · How it works (stacks over Hero) ── */}
          <section className="relative min-h-screen w-full grid place-content-center sticky top-0 rounded-t-3xl overflow-hidden bg-[#FAFAFA]">
            <div className="absolute inset-0 bg-[linear-gradient(to_right,#e4e4e720_1px,transparent_1px),linear-gradient(to_bottom,#e4e4e720_1px,transparent_1px)] bg-[size:54px_54px] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_100%)]" />

            <div className="relative z-10 max-w-5xl mx-auto px-6 py-20">
              <div className="text-center mb-14">
                <h2 className="font-display text-4xl md:text-5xl font-bold text-zinc-900 mb-4">
                  How it works
                </h2>
                <p className="text-zinc-500 text-lg">
                  Three steps to your first swap.
                </p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {steps.map((step, i) => (
                  <motion.div
                    key={i}
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true, amount: 0.3 }}
                    transition={{ duration: 0.5, delay: i * 0.15 }}
                    className="bg-white rounded-xl border border-zinc-200 p-6 hover:border-[#1D9E75]/50 hover:shadow-lg transition-all duration-200"
                  >
                    <div className="w-12 h-12 rounded-full bg-[#E1F5EE] flex items-center justify-center mb-5">
                      <step.icon className="w-6 h-6 text-[#1D9E75]" />
                    </div>
                    <h3 className="font-display text-lg font-semibold text-zinc-900 mb-2">
                      {step.title}
                    </h3>
                    <p className="text-sm text-zinc-500 leading-relaxed">
                      {step.description}
                    </p>
                  </motion.div>
                ))}
              </div>
            </div>
          </section>

          {/* ── Section 3 · Features (stacks over How-it-works) ── */}
          <section className="relative min-h-screen w-full grid place-content-center sticky top-0 rounded-t-3xl overflow-hidden bg-zinc-900">
            <div className="absolute inset-0 bg-[linear-gradient(to_right,#ffffff08_1px,transparent_1px),linear-gradient(to_bottom,#ffffff08_1px,transparent_1px)] bg-[size:54px_54px] [mask-image:radial-gradient(ellipse_60%_50%_at_50%_0%,#000_70%,transparent_100%)]" />

            <div className="relative z-10 max-w-5xl mx-auto px-6 py-20">
              <div className="text-center mb-14">
                <h2 className="font-display text-4xl md:text-5xl font-bold text-white mb-4">
                  Built for real exchanges
                </h2>
                <p className="text-white/50 text-lg">
                  Everything you need to swap skills with confidence.
                </p>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                {features.map((feature, i) => (
                  <motion.div
                    key={i}
                    initial={{ opacity: 0, y: 30 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true, amount: 0.3 }}
                    transition={{ duration: 0.5, delay: i * 0.1 }}
                    className="bg-white/5 backdrop-blur-sm rounded-xl border border-zinc-700/50 p-6 hover:border-[#5DCAA5]/40 hover:bg-white/10 transition-all duration-200"
                  >
                    <div className="w-12 h-12 rounded-full bg-zinc-800 flex items-center justify-center mb-5">
                      <feature.icon className="w-6 h-6 text-zinc-400" />
                    </div>
                    <h3 className="font-display text-lg font-semibold text-white mb-2">
                      {feature.title}
                    </h3>
                    <p className="text-sm text-white/50 leading-relaxed">
                      {feature.description}
                    </p>
                  </motion.div>
                ))}
              </div>
            </div>
          </section>
        </div>

        {/* ━━━━━━ SPLIT: Sticky text + scrolling stat cards ━━━━━━ */}
        <section className="w-full bg-[#FAFAFA] rounded-t-3xl">
          <div className="grid grid-cols-1 md:grid-cols-2">
            {/* Left: sticky text */}
            <div className="sticky top-0 h-screen flex items-center justify-center px-8">
              <div className="max-w-sm">
                <h2 className="font-display text-4xl md:text-5xl font-bold text-zinc-900 leading-tight mb-6">
                  The numbers
                  <br />
                  speak for
                  <br />
                  <span className="text-[#1D9E75]">themselves.</span>
                </h2>
                <p className="text-zinc-500 text-base leading-relaxed">
                  Our community is growing every day. Join thousands of people
                  already exchanging skills.
                </p>
              </div>
            </div>

            {/* Right: scrolling stat cards */}
            <div className="grid gap-4 py-8 px-4">
              {stats.map((stat, i) => (
                <div
                  key={i}
                  className="bg-white rounded-2xl border border-zinc-200 p-8 flex flex-col items-center justify-center min-h-[50vh]"
                >
                  <AnimatedNumber
                    value={stat.value}
                    className="font-display text-6xl md:text-7xl font-bold text-zinc-900"
                  />
                  <p className="text-[#1D9E75] font-medium text-lg mt-3">
                    {stat.label}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* ━━━━━━ Marquee Skills Strip ━━━━━━ */}
        <section className="bg-zinc-900 border-y border-zinc-700/50 py-5 overflow-hidden">
          <Marquee pauseOnHover>
            {skillCategories.map((cat) => (
              <span
                key={cat}
                className="mx-3 px-5 py-2 rounded-full bg-white/5 text-zinc-400 text-sm border border-zinc-700/50 whitespace-nowrap"
              >
                {cat}
              </span>
            ))}
          </Marquee>
        </section>

        {/* ━━━━━━ CTA Banner ━━━━━━ */}
        <section className="py-24 bg-[#FAFAFA]">
          <div className="max-w-2xl mx-auto px-6 text-center">
            <h2 className="font-display text-4xl md:text-5xl font-bold text-zinc-900 mb-5">
              Your skills are
              <br />
              your currency.
            </h2>
            <p className="text-zinc-500 text-lg mb-10">
              Join TalentVerse and start building your reputation today.
            </p>
            <Link href="/register">
              <button className="bg-[#3C2A8A] text-white font-semibold px-10 py-4 rounded-xl hover:bg-[#534AB7] transition-colors text-base active:scale-[0.98]">
                Create your profile
              </button>
            </Link>
          </div>
        </section>

        {/* ━━━━━━ Footer ━━━━━━ */}
        <footer className="group bg-zinc-900 text-white/60 relative">
          {/* Big brand text that peeks on hover */}
          <h1 className="text-[14vw] group-hover:translate-y-4 translate-y-20 leading-[100%] uppercase font-bold text-center bg-gradient-to-r from-[#3C2A8A] to-[#1D9E75] bg-clip-text text-transparent transition-all ease-linear select-none pointer-events-none">
            TalentVerse
          </h1>

          <div className="bg-zinc-950 relative z-10 rounded-t-[2.5rem] pt-16 pb-8">
            <div className="container mx-auto px-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12">
                {/* Brand */}
                <div className="space-y-4">
                  <span className="font-display font-bold text-xl text-white">
                    Talent<span className="text-[#5DCAA5]">.</span>Verse
                  </span>
                  <p className="text-white/30 text-sm leading-relaxed">
                    Empowering the world to trade talent, not currency. Built
                    for creators, learners, and dreamers.
                  </p>
                  <div className="flex space-x-4 pt-2">
                    {[Twitter, Instagram, Linkedin, Github].map((Icon, i) => (
                      <a
                        key={i}
                        href="#"
                        className="text-white/30 hover:text-[#5DCAA5] transition-colors"
                      >
                        <Icon size={20} />
                      </a>
                    ))}
                  </div>
                </div>

                {/* Link Columns */}
                {Object.entries(footerLinks).map(([title, links]) => (
                  <div key={title}>
                    <h4 className="text-white font-display font-semibold mb-6 text-sm">
                      {title}
                    </h4>
                    <ul className="space-y-3 text-sm">
                      {links.map((link) => (
                        <li key={link}>
                          <a
                            href="#"
                            className="text-white/30 hover:text-white transition-colors"
                          >
                            {link}
                          </a>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}

                {/* Newsletter */}
                <div>
                  <h4 className="text-white font-display font-semibold mb-6 text-sm">
                    Stay Updated
                  </h4>
                  <p className="text-xs text-white/20 mb-4">
                    Join our newsletter for the latest skill trends.
                  </p>
                  <div className="flex flex-col space-y-3">
                    <input
                      type="email"
                      placeholder="Enter your email"
                      className="bg-white/5 border border-zinc-700/50 text-white px-4 py-3 rounded-lg focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75]/20 transition-colors text-sm placeholder:text-white/20"
                    />
                    <button className="bg-[#1D9E75] hover:bg-[#0F6E56] text-white font-semibold py-3 rounded-lg transition-colors text-sm">
                      Subscribe
                    </button>
                  </div>
                </div>
              </div>

              <div className="border-t border-zinc-700/30 pt-8 flex flex-col md:flex-row justify-between items-center text-xs text-white/20">
                <p>
                  © {new Date().getFullYear()} TalentVerse. All rights reserved.
                </p>
                <div className="flex space-x-6 mt-4 md:mt-0">
                  <a
                    href="#"
                    className="hover:text-white transition-colors"
                  >
                    Privacy Policy
                  </a>
                  <a
                    href="#"
                    className="hover:text-white transition-colors"
                  >
                    Terms of Service
                  </a>
                </div>
              </div>
            </div>
          </div>
        </footer>
      </main>
    </ReactLenis>
  );
}
