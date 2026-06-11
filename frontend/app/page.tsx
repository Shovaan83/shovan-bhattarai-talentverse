"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import { ReactLenis } from "lenis/react";
import {
  ArrowRight,
  ArrowRightLeft,
  BadgeCheck,
  Camera,
  CheckCircle2,
  ChevronRight,
  Code2,
  Coins,
  Github,
  Guitar,
  Menu,
  MessageSquare,
  PenTool,
  Search,
  ShieldCheck,
  Sparkles,
  Star,
  Trophy,
  UserRoundCheck,
  Users,
  Video,
  X,
  XCircle,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { BrandLogo } from "./components/BrandLogo";

const navLinks = [
  { label: "How it works", href: "#how-it-works" },
  { label: "Benefits", href: "#benefits" },
  { label: "Compare", href: "#comparison" },
  { label: "Preview", href: "#preview" },
];

const flowPairs = [
  { from: "Design", to: "Coding", icon: PenTool, side: "offer" },
  { from: "Guitar", to: "Photography", icon: Guitar, side: "offer" },
  { from: "Marketing", to: "Math", icon: PenTool, side: "want" },
  { from: "Writing", to: "Video Editing", icon: Video, side: "want" },
];

const problemItems = [
  {
    label: "$80",
    title: "Course Fee",
    description: "One class gives you access, but not always the specific help you need.",
    side: "left",
    tone: "red",
    marker: "Now",
  },
  {
    label: "$40",
    title: "Tutor Hour",
    description: "Private help works, but every extra question becomes another paid hour.",
    side: "right",
    tone: "blue",
    marker: "1",
  },
  {
    label: "Weeks",
    title: "Searching",
    description: "Finding the right person, schedule, and trust signal takes more time.",
    side: "left",
    tone: "yellow",
    marker: "2",
  },
  {
    label: "Still",
    title: "Skill Missing",
    description: "After all that spend, the real skill gap can still be unresolved.",
    side: "right",
    tone: "gray",
    marker: "3+",
  },
];

const howItWorks = [
  {
    number: "01",
    title: "Create your skill profile",
    description:
      "List what you can offer, what you want, your proficiency level, and a bio that makes your profile easy to trust.",
    icon: UserRoundCheck,
    tone: "offer",
  },
  {
    number: "02",
    title: "Match with someone who wants your skill",
    description:
      "Browse marketplace cards, compare offered and wanted skills, and find someone whose goals complement yours.",
    icon: Search,
    tone: "want",
  },
  {
    number: "03",
    title: "Send a proposal and complete the swap",
    description:
      "Agree on the exchange, message in real time, schedule the session, confirm completion, and collect reputation.",
    icon: MessageSquare,
    tone: "proposal",
  },
];

const benefitCards = [
  {
    title: "Verified Profiles",
    description: "Identity review and reputation signals make each new swap easier to trust.",
    icon: ShieldCheck,
  },
  {
    title: "Skill Marketplace",
    description: "Offer and want states are visually distinct, searchable, and built for fast scanning.",
    icon: Users,
  },
  {
    title: "Real-time Proposals",
    description: "Move from interest to accepted proposal without losing messages or context.",
    icon: ArrowRightLeft,
  },
  {
    title: "Swap Credits",
    description: "Completed swaps and badge milestones turn participation into useful credits.",
    icon: Coins,
  },
];

const barterlyBenefits = [
  "Trade skills directly",
  "Build reputation",
  "Earn swap credits",
  "Use verified profiles",
];

const traditionalCosts = [
  "Pay upfront",
  "No guaranteed fit",
  "Limited trust signals",
  "One-way transaction",
];

const growthNodes = [
  { label: "Offer Skill", icon: PenTool, position: "md:left-1/2 md:top-4 md:-translate-x-1/2" },
  { label: "Get Matched", icon: Search, position: "md:right-14 md:top-[36%]" },
  { label: "Complete Swap", icon: CheckCircle2, position: "md:right-28 md:bottom-10" },
  { label: "Earn Credits", icon: Coins, position: "md:left-28 md:bottom-10" },
  { label: "Build Reputation", icon: Trophy, position: "md:left-14 md:top-[36%]" },
];

const floatingSkillBadges = [
  {
    label: "Figma",
    Logo: FigmaLogoMark,
    className: "left-[3%] top-[2%] rotate-[-10deg]",
    containerClassName: "border-zinc-200",
    logoClassName: "h-12 w-12",
  },
  {
    label: "React",
    Logo: ReactLogoMark,
    className: "right-[4%] top-[3%] rotate-[8deg]",
    containerClassName: "border-blue-200 bg-blue-50",
    logoClassName: "h-12 w-12 text-[#61DAFB]",
  },
  {
    label: "Guitar",
    Logo: GuitarLogoMark,
    className: "left-[7%] bottom-[7%] rotate-[7deg]",
    containerClassName: "border-emerald-200 bg-emerald-50",
    logoClassName: "h-12 w-12 text-[#0F6E56]",
  },
  {
    label: "Math",
    Logo: MathLogoMark,
    className: "right-[7%] bottom-[7%] rotate-[-8deg]",
    containerClassName: "border-violet-200 bg-violet-50",
    logoClassName: "h-12 w-12 text-[#3C2A8A]",
  }
];

function FigmaLogoMark({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 38 56" fill="none" aria-hidden="true" className={className}>
      <path d="M19 28a9.5 9.5 0 1 1 19 0 9.5 9.5 0 0 1-19 0Z" fill="#1ABCFE" />
      <path d="M0 47.5A9.5 9.5 0 0 1 9.5 38H19v9.5A9.5 9.5 0 0 1 0 47.5Z" fill="#0ACF83" />
      <path d="M0 28a9.5 9.5 0 0 1 9.5-9.5H19v19H9.5A9.5 9.5 0 0 1 0 28Z" fill="#A259FF" />
      <path d="M0 9.5A9.5 9.5 0 0 1 9.5 0H19v19H9.5A9.5 9.5 0 0 1 0 9.5Z" fill="#F24E1E" />
      <path d="M19 0h9.5a9.5 9.5 0 1 1 0 19H19V0Z" fill="#FF7262" />
    </svg>
  );
}

function ReactLogoMark({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 64 58" fill="none" aria-hidden="true" className={className}>
      <circle cx="32" cy="29" r="5.6" fill="currentColor" />
      <ellipse cx="32" cy="29" rx="28" ry="10.8" stroke="currentColor" strokeWidth="4" />
      <ellipse cx="32" cy="29" rx="28" ry="10.8" stroke="currentColor" strokeWidth="4" transform="rotate(60 32 29)" />
      <ellipse cx="32" cy="29" rx="28" ry="10.8" stroke="currentColor" strokeWidth="4" transform="rotate(120 32 29)" />
    </svg>
  );
}

function GuitarLogoMark({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 48 48" fill="none" aria-hidden="true" className={className}>
      <path d="M18.5 36.5c-4 4-9.8 4.7-13 1.5s-2.5-9 1.5-13c2.8-2.8 6.5-4 9.6-3.4l3-3a6 6 0 0 1 8.4 8.4l-3 3c.6 3.1-.6 6.8-3.4 9.6Z" fill="currentColor" opacity=".16" />
      <path d="m18 29 7-7m-9.2-.3 3.5-3.5a6.2 6.2 0 0 1 8.8 8.8l-3.5 3.5M18.5 36.5c-4 4-9.8 4.7-13 1.5s-2.5-9 1.5-13c4-4 9.8-4.7 13-1.5s2.5 9-1.5 13Z" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" />
      <path d="m31 15 7-7m-3 3 6 6" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
      <circle cx="13" cy="31" r="4" fill="currentColor" />
    </svg>
  );
}

function MathLogoMark({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 48 48" fill="none" aria-hidden="true" className={className}>
      <rect x="5" y="5" width="38" height="38" rx="12" fill="currentColor" opacity=".12" />
      <path d="M14 16h10M19 11v10M29 15h8M29 21h8M15 30l8 8m0-8-8 8M30 32h8m-4-4v8" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function VideoLogoMark({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 48 48" fill="none" aria-hidden="true" className={className}>
      <rect x="7" y="12" width="24" height="24" rx="7" fill="currentColor" opacity=".14" />
      <path d="M12 18.5a4 4 0 0 1 4-4h13a4 4 0 0 1 4 4v11a4 4 0 0 1-4 4H16a4 4 0 0 1-4-4v-11Z" stroke="currentColor" strokeWidth="3" />
      <path d="m33 21 7-4.2a1.8 1.8 0 0 1 2.7 1.5v11.4a1.8 1.8 0 0 1-2.7 1.5L33 27" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" />
      <path d="m21 20 7 4-7 4v-8Z" fill="currentColor" />
    </svg>
  );
}

function LandingNav() {
  return (
    <header className="fixed inset-x-0 top-5 z-50 px-4">
      <div className="mx-auto flex h-14 max-w-5xl items-center justify-between rounded-full border border-zinc-200 bg-white/95 px-4 shadow-lg shadow-zinc-900/10 backdrop-blur-md sm:px-5">
        <Link href="/" aria-label="Barterly home">
          <BrandLogo iconClassName="h-7 w-7" textClassName="text-lg text-zinc-900" />
        </Link>

        <nav className="hidden items-center gap-1 md:flex">
          {navLinks.map((link) => (
            <a
              key={link.href}
              href={link.href}
              className="rounded-full px-3 py-2 text-sm font-medium text-zinc-600 transition-colors hover:bg-zinc-100 hover:text-zinc-900"
            >
              {link.label}
            </a>
          ))}
        </nav>

        <div className="hidden items-center gap-2 md:flex">
          <Link
            href="/login"
            className="rounded-full border border-zinc-200 px-4 py-2 text-sm font-semibold text-zinc-700 transition-colors hover:bg-zinc-50 hover:text-zinc-900"
          >
            Access
          </Link>
          <Link
            href="/register"
            className="rounded-full bg-[#1D9E75] px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-[#0F6E56]"
          >
            Start Swapping
          </Link>
        </div>

        <details className="group md:hidden">
          <summary
            className="flex cursor-pointer list-none rounded-full p-2 text-zinc-700 transition-colors hover:bg-zinc-100 [&::-webkit-details-marker]:hidden"
            aria-label="Toggle navigation menu"
          >
            <Menu className="h-5 w-5 group-open:hidden" />
            <X className="hidden h-5 w-5 group-open:block" />
          </summary>
          <div className="absolute left-4 right-4 top-16 rounded-2xl border border-zinc-200 bg-white p-4 shadow-xl">
            <div className="flex flex-col gap-1">
              {navLinks.map((link) => (
                <a
                  key={link.href}
                  href={link.href}
                  className="rounded-xl px-3 py-3 text-sm font-medium text-zinc-700 transition-colors hover:bg-zinc-50"
                >
                  {link.label}
                </a>
              ))}
              <div className="mt-3 grid grid-cols-2 gap-3">
                <Link
                  href="/login"
                  className="rounded-xl border border-zinc-200 px-4 py-3 text-center text-sm font-semibold text-zinc-900"
                >
                  Access
                </Link>
                <Link
                  href="/register"
                  className="rounded-xl bg-[#1D9E75] px-4 py-3 text-center text-sm font-semibold text-white"
                >
                  Join
                </Link>
              </div>
            </div>
          </div>
        </details>
      </div>
    </header>
  );
}

function SectionShell({
  id,
  children,
  className = "",
  showLine = true,
}: {
  id?: string;
  children: React.ReactNode;
  className?: string;
  showLine?: boolean;
}) {
  return (
    <section id={id} className={`relative isolate overflow-hidden bg-[#FAFAFA] ${className}`}>
      <div className="absolute inset-0 -z-20 bg-[radial-gradient(circle_at_1px_1px,#d4d4d8_1px,transparent_0)] bg-[length:12px_12px] opacity-35" />
      <div className="absolute inset-x-0 bottom-0 -z-20 h-1/2 bg-[radial-gradient(circle_at_bottom,#1D9E7514,transparent_55%)]" />
      {showLine && (
        <div className="absolute left-1/2 top-0 -z-10 hidden h-full w-px -translate-x-1/2 bg-[#1D9E75]/35 md:block" />
      )}
      {children}
    </section>
  );
}

function SectionPill({ children }: { children: React.ReactNode }) {
  return (
    <span className="inline-flex rounded-full border border-zinc-300 bg-white/90 px-4 py-2 text-sm font-semibold text-zinc-700 shadow-sm">
      {children}
    </span>
  );
}

function SectionTitle({
  eyebrow,
  title,
  accent,
}: {
  eyebrow?: string;
  title: string;
  accent?: string;
}) {
  return (
    <div className="relative z-10 mx-auto max-w-4xl text-center">
      <div className="absolute inset-x-0 -top-6 bottom-[-1.5rem] -z-10 mx-auto w-[min(100%,54rem)] rounded-[2rem] bg-[#FAFAFA]" />
      {eyebrow && <SectionPill>{eyebrow}</SectionPill>}
      <h2 className="mt-6 font-display text-4xl font-bold leading-[1.05] text-zinc-900 sm:text-5xl lg:text-6xl">
        {title}
        {accent && <span className="block text-[#1D9E75]">{accent}</span>}
      </h2>
    </div>
  );
}

function HeroSkillFlow() {
  return (
    <SectionShell className="pt-32">
      <div className="mx-auto max-w-7xl px-4 pb-24 sm:px-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.55 }}
          className="relative z-10 mx-auto max-w-5xl text-center"
        >
          <div className="absolute inset-x-0 -top-6 bottom-[-1.5rem] -z-10 mx-auto w-[min(100%,58rem)] rounded-[2rem] bg-[#FAFAFA]" />
          <SectionPill>Skill exchange, not another paid course</SectionPill>
          <h1 className="mt-6 font-display text-5xl font-bold leading-[0.98] text-zinc-900 sm:text-6xl lg:text-7xl">
            STOP PAYING FOR SKILLS
            <span className="block text-[#1D9E75]">START SWAPPING WHAT YOU KNOW</span>
          </h1>
          <p className="mx-auto mt-6 max-w-2xl text-base leading-7 text-zinc-600 sm:text-lg">
            Barterly turns existing skills into learning power. Offer what you
            know, request what you need, and use proposals, credits, and
            reputation to keep each exchange trusted.
          </p>
          <div className="mt-8 flex flex-col justify-center gap-3 sm:flex-row">
            <Link
              href="/register"
              className="inline-flex items-center justify-center gap-2 rounded-full bg-[#1D9E75] px-7 py-3.5 text-sm font-semibold text-white shadow-lg shadow-emerald-900/10 transition-colors hover:bg-[#0F6E56]"
            >
              Start Swapping
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/marketplace"
              className="inline-flex items-center justify-center gap-2 rounded-full border border-zinc-200 bg-white px-7 py-3.5 text-sm font-semibold text-zinc-900 transition-colors hover:bg-zinc-50"
            >
              Browse Marketplace
              <Search className="h-4 w-4" />
            </Link>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 24 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.55, delay: 0.12 }}
          className="relative mx-auto mt-16 max-w-5xl rounded-[2rem] border border-zinc-200 bg-white/95 p-5 shadow-2xl shadow-zinc-900/10 sm:p-8"
        >
          <div className="absolute inset-5 rounded-[1.5rem] bg-[radial-gradient(circle_at_20%_20%,#1D9E751c,transparent_30%),radial-gradient(circle_at_80%_75%,#534AB71f,transparent_34%)]" />
          <svg
            className="pointer-events-none absolute inset-0 hidden h-full w-full md:block"
            viewBox="0 0 980 540"
            fill="none"
            aria-hidden="true"
          >
            <path d="M160 125 C315 132 340 250 465 260" stroke="#1D9E75" strokeWidth="3" strokeDasharray="8 10" />
            <path d="M160 385 C315 378 340 288 465 280" stroke="#5DCAA5" strokeWidth="3" strokeDasharray="8 10" />
            <path d="M820 125 C660 132 640 250 515 260" stroke="#3C2A8A" strokeWidth="3" strokeDasharray="8 10" />
            <path d="M820 385 C660 378 640 288 515 280" stroke="#534AB7" strokeWidth="3" strokeDasharray="8 10" />
          </svg>

          <div className="relative grid gap-4 md:grid-cols-[1fr_1.05fr_1fr] md:items-center">
            <div className="grid gap-4">
              {flowPairs.slice(0, 2).map((pair) => (
                <FlowCard key={pair.from} {...pair} />
              ))}
            </div>

            <div className="rounded-[1.5rem] border border-emerald-200 bg-white p-6 text-center shadow-xl shadow-emerald-950/10">
              <div className="mx-auto flex h-20 w-20 items-center justify-center rounded-3xl bg-[#1D9E75]">
                <img src="/brand/icon-only-logo -nobg.png" alt="" className="h-14 w-14" />
              </div>
              <p className="mt-5 text-sm font-semibold text-[#1D9E75]">
                All swaps flow into
              </p>
              <h2 className="mt-1 text-3xl font-bold text-zinc-900">
                Barterly Marketplace
              </h2>
              <div className="mt-6 grid grid-cols-2 gap-2 text-left">
                <MiniStat label="Offers" value="Teal" className="bg-emerald-50 text-[#0F6E56]" />
                <MiniStat label="Wants" value="Violet" className="bg-violet-50 text-[#3C2A8A]" />
                <MiniStat label="Proposals" value="Blue" className="bg-blue-50 text-blue-700" />
                <MiniStat label="Credits" value="Earn" className="bg-zinc-100 text-zinc-700" />
              </div>
            </div>

            <div className="grid gap-4">
              {flowPairs.slice(2).map((pair) => (
                <FlowCard key={pair.from} {...pair} />
              ))}
            </div>
          </div>
        </motion.div>
      </div>
    </SectionShell>
  );
}

function FlowCard({
  from,
  to,
  icon: Icon,
  side,
}: {
  from: string;
  to: string;
  icon: LucideIcon;
  side: string;
}) {
  const tone =
    side === "offer"
      ? "bg-emerald-50 text-[#0F6E56] border-emerald-200"
      : "bg-violet-50 text-[#3C2A8A] border-violet-200";

  return (
    <div className="rounded-2xl border border-zinc-200 bg-white p-4 shadow-sm">
      <div className="mb-4 flex items-center justify-between gap-3">
        <span className={`flex h-11 w-11 items-center justify-center rounded-xl border ${tone}`}>
          <Icon className="h-5 w-5" />
        </span>
        <span className="rounded-full bg-zinc-100 px-2.5 py-1 text-xs font-semibold text-zinc-600">
          {side === "offer" ? "Offer" : "Want"}
        </span>
      </div>
      <div className="flex items-center gap-2 text-sm font-bold text-zinc-900">
        <span className="min-w-0 truncate">{from}</span>
        <ArrowRight className="h-4 w-4 shrink-0 text-[#1D9E75]" />
        <span className="min-w-0 truncate text-[#3C2A8A]">{to}</span>
      </div>
    </div>
  );
}

function MiniStat({
  label,
  value,
  className,
}: {
  label: string;
  value: string;
  className: string;
}) {
  return (
    <div className={`rounded-xl px-3 py-2 ${className}`}>
      <p className="text-[11px] font-medium uppercase opacity-70">{label}</p>
      <p className="text-sm font-bold">{value}</p>
    </div>
  );
}

function ProblemTimeline() {
  return (
    <SectionShell className="py-24 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionTitle
          eyebrow="The problem"
          title="YOU NEED HELP, BUT"
          accent="LEARNING GETS EXPENSIVE"
        />

        <div className="relative mx-auto mt-14 max-w-5xl">
          <div className="absolute left-4 top-0 h-full w-px bg-[#1D9E75]/35 sm:left-1/2 sm:-translate-x-1/2" />
          <div className="space-y-12 sm:space-y-16">
            {problemItems.map((item, index) => (
              <ProblemRow key={item.title} item={item} index={index} />
            ))}
          </div>
          <div className="relative mx-auto mt-14 max-w-md rounded-2xl border border-blue-200 bg-blue-50 px-6 py-4 text-center text-sm font-semibold text-blue-700 shadow-sm">
            Learning is valuable, but paying for every skill adds up.
          </div>
        </div>
      </div>
    </SectionShell>
  );
}

function ProblemRow({
  item,
  index,
}: {
  item: {
    label: string;
    title: string;
    description: string;
    side: string;
    tone: string;
    marker: string;
  };
  index: number;
}) {
  const toneMap: Record<string, string> = {
    red: "bg-red-100 text-red-700 border-red-200",
    blue: "bg-blue-100 text-blue-700 border-blue-200",
    yellow: "bg-yellow-100 text-yellow-800 border-yellow-200",
    gray: "bg-zinc-100 text-zinc-700 border-zinc-200",
  };
  const isRight = item.side === "right";

  return (
    <motion.div
      initial={{ opacity: 0, y: 18 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, amount: 0.35 }}
      transition={{ duration: 0.35, delay: index * 0.06 }}
      className="relative grid gap-4 sm:grid-cols-[1fr_104px_1fr] sm:items-center"
    >
      <div className={`${isRight ? "hidden sm:block" : "sm:text-right"}`}>
        {!isRight && <TimelineText item={item} toneClass={toneMap[item.tone]} />}
      </div>
      <div className="absolute left-4 top-0 flex h-16 w-16 -translate-x-1/2 items-center justify-center rounded-2xl border border-zinc-300 bg-white text-center shadow-sm sm:static sm:translate-x-0">
        <span className="text-xs font-bold text-zinc-600">
          STEP
          <span className="block text-2xl text-zinc-900">{item.marker}</span>
        </span>
      </div>
      <div className={`${isRight ? "ml-10 sm:ml-0" : "ml-10 sm:hidden"}`}>
        {isRight ? (
          <TimelineText item={item} toneClass={toneMap[item.tone]} />
        ) : (
          <TimelineText item={item} toneClass={toneMap[item.tone]} />
        )}
      </div>
    </motion.div>
  );
}

function TimelineText({
  item,
  toneClass,
}: {
  item: { label: string; title: string; description: string };
  toneClass: string;
}) {
  return (
    <div className="rounded-2xl border border-zinc-200 bg-white p-5 shadow-sm">
      <span className={`inline-flex rounded-full border px-3 py-1 text-xs font-bold ${toneClass}`}>
        {item.label}
      </span>
      <h3 className="mt-3 text-xl font-bold text-zinc-900">{item.title}</h3>
      <p className="mt-2 text-sm leading-6 text-zinc-600">{item.description}</p>
    </div>
  );
}

function HowItWorksCards() {
  return (
    <SectionShell id="how-it-works" className="py-24 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionTitle
          eyebrow="From profile to first swap"
          title="START EXCHANGING"
          accent="IN THREE MOVES"
        />

        <div className="mx-auto mt-14 grid max-w-5xl gap-8">
          {howItWorks.map((step, index) => (
            <StepCard key={step.number} step={step} index={index} />
          ))}
        </div>
      </div>
    </SectionShell>
  );
}

function StepCard({
  step,
  index,
}: {
  step: {
    number: string;
    title: string;
    description: string;
    icon: LucideIcon;
    tone: string;
  };
  index: number;
}) {
  const reverse = index % 2 === 1;
  const toneClass =
    step.tone === "offer"
      ? "bg-[#1D9E75] text-white"
      : step.tone === "want"
        ? "bg-[#3C2A8A] text-white"
        : "bg-blue-700 text-white";

  return (
    <motion.article
      initial={{ opacity: 0, y: 24 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, amount: 0.25 }}
      transition={{ duration: 0.4, delay: index * 0.06 }}
      className="grid gap-6 rounded-[2rem] border border-zinc-200 bg-white p-6 shadow-xl shadow-zinc-900/5 md:grid-cols-2 md:items-center md:p-8"
    >
      <div className={reverse ? "md:order-2" : ""}>
        <span className={`inline-flex h-12 w-12 items-center justify-center rounded-full text-lg font-bold ${toneClass}`}>
          {step.number}
        </span>
        <h3 className="mt-6 max-w-md text-3xl font-bold leading-tight text-zinc-900">
          {step.title}
        </h3>
        <p className="mt-5 max-w-md text-sm leading-7 text-zinc-600">
          {step.description}
        </p>
      </div>
      <div className={reverse ? "md:order-1" : ""}>
        <StepVisual step={step} index={index} />
      </div>
    </motion.article>
  );
}

function StepVisual({
  step,
  index,
}: {
  step: { icon: LucideIcon; tone: string; title: string };
  index: number;
}) {
  const Icon = step.icon;
  const visualBg =
    step.tone === "offer"
      ? "from-emerald-50 to-white"
      : step.tone === "want"
        ? "from-violet-50 to-white"
        : "from-blue-50 to-white";

  return (
    <div className={`relative min-h-64 overflow-hidden rounded-2xl border border-zinc-200 bg-gradient-to-br ${visualBg} p-5`}>
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_1px_1px,#a1a1aa_1px,transparent_0)] bg-[length:14px_14px] opacity-20" />
      <div className="relative flex h-full min-h-56 flex-col justify-between">
        <div className="flex items-center justify-between">
          <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-white text-[#1D9E75] shadow-sm">
            <Icon className="h-6 w-6" />
          </span>
          <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-zinc-600 shadow-sm">
            Barterly
          </span>
        </div>
        {index === 0 && (
          <div className="space-y-3">
            <ProfileSkillRow label="Offering" value="Design Systems" tone="offer" />
            <ProfileSkillRow label="Seeking" value="React Animation" tone="want" />
            <ProfileSkillRow label="Trust" value="Verified profile" tone="proposal" />
          </div>
        )}
        {index === 1 && (
          <div className="rounded-2xl bg-white p-4 shadow-lg">
            <div className="mb-4 h-24 rounded-xl bg-zinc-900/90" />
            <h4 className="font-bold text-zinc-900">Maya Chen</h4>
            <p className="text-sm text-zinc-500">@maya.designs</p>
            <div className="mt-3 flex flex-wrap gap-2">
              <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-semibold text-emerald-800">UX Audits</span>
              <span className="rounded-full bg-violet-100 px-2 py-1 text-xs font-semibold text-[#3C2A8A]">Motion</span>
            </div>
          </div>
        )}
        {index === 2 && (
          <div className="space-y-3">
            <div className="rounded-2xl border border-blue-200 bg-blue-50 p-4">
              <div className="flex items-center justify-between gap-3">
                <span className="text-sm font-bold text-blue-700">Proposal</span>
                <span className="rounded-full bg-yellow-100 px-2 py-1 text-xs font-bold text-yellow-800">Pending</span>
              </div>
              <p className="mt-3 text-sm font-semibold text-zinc-900">Design audit for coding help</p>
            </div>
            <div className="rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-sm font-semibold text-[#0F6E56]">
              Completion confirms credits and reputation.
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function ProfileSkillRow({
  label,
  value,
  tone,
}: {
  label: string;
  value: string;
  tone: "offer" | "want" | "proposal";
}) {
  const className =
    tone === "offer"
      ? "bg-emerald-50 text-[#0F6E56] border-emerald-200"
      : tone === "want"
        ? "bg-violet-50 text-[#3C2A8A] border-violet-200"
        : "bg-blue-50 text-blue-700 border-blue-200";

  return (
    <div className={`rounded-xl border p-3 ${className}`}>
      <p className="text-xs font-semibold opacity-75">{label}</p>
      <p className="font-bold">{value}</p>
    </div>
  );
}

function BenefitsPanel() {
  return (
    <SectionShell id="benefits" className="py-16 sm:py-20">
      <div className="mx-auto max-w-6xl px-4 sm:px-6">
        <div className="rounded-[2rem] bg-[#1D1340] px-5 py-14 text-white shadow-2xl shadow-violet-950/25 sm:px-8 lg:px-12">
          <div className="mx-auto max-w-3xl text-center">
            <h2 className="font-display text-4xl font-bold leading-tight sm:text-5xl">
              BUILT FOR LEARNERS.
              <span className="block text-[#5DCAA5]">POWERED BY SKILL SHARERS.</span>
            </h2>
          </div>
          <div className="mt-12 grid gap-4 md:grid-cols-4">
            {benefitCards.map((card) => (
              <div key={card.title} className="rounded-2xl border border-white/10 bg-white/[0.06] p-5">
                <span className="flex h-12 w-12 items-center justify-center rounded-xl bg-[#1D9E75]/15 text-[#5DCAA5]">
                  <card.icon className="h-6 w-6" />
                </span>
                <h3 className="mt-5 font-bold text-white">{card.title}</h3>
                <p className="mt-3 text-sm leading-6 text-white/60">{card.description}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </SectionShell>
  );
}

function ComparisonSection() {
  return (
    <SectionShell id="comparison" className="py-24 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionTitle
          eyebrow="Our edge"
          title="MARKETPLACES CHARGE MONEY."
          accent="BARTERLY TRADES VALUE."
        />
        <div className="mx-auto mt-12 grid max-w-5xl gap-5 md:grid-cols-2">
          <ComparisonCard
            title="Barterly"
            subtitle="Skill exchange with reputation"
            items={barterlyBenefits}
            icon={CheckCircle2}
            iconClassName="text-[#1D9E75]"
            className="border-emerald-200 bg-white"
          />
          <ComparisonCard
            title="Traditional Learning"
            subtitle="Pay first, hope it fits"
            items={traditionalCosts}
            icon={XCircle}
            iconClassName="text-zinc-500"
            className="border-zinc-200 bg-zinc-100/80"
          />
        </div>
      </div>
    </SectionShell>
  );
}

function ComparisonCard({
  title,
  subtitle,
  items,
  icon: Icon,
  iconClassName,
  className,
}: {
  title: string;
  subtitle: string;
  items: string[];
  icon: LucideIcon;
  iconClassName: string;
  className: string;
}) {
  return (
    <div className={`rounded-[2rem] border p-7 shadow-xl shadow-zinc-900/5 ${className}`}>
      <div className="flex items-center justify-between gap-4 border-b border-zinc-200 pb-5">
        <div>
          <h3 className="text-2xl font-bold text-zinc-900">{title}</h3>
          <p className="mt-1 text-sm text-zinc-500">{subtitle}</p>
        </div>
        {title === "Barterly" && <BrandLogo showText={false} iconClassName="h-10 w-10" />}
      </div>
      <div className="mt-7 grid gap-4">
        {items.map((item) => (
          <div key={item} className="flex items-center gap-4">
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-white shadow-sm">
              <Icon className={`h-5 w-5 ${iconClassName}`} />
            </span>
            <span className="text-sm font-semibold text-zinc-700">{item}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function GrowthCycle() {
  return (
    <SectionShell className="py-24 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionTitle
          eyebrow="Growth cycle"
          title="POWERING A"
          accent="SELF-REINFORCING SKILL LOOP"
        />

        <div className="relative mx-auto mt-12 min-h-[560px] max-w-4xl">
          <div className="absolute left-1/2 top-1/2 hidden h-[320px] w-[320px] -translate-x-1/2 -translate-y-1/2 rounded-full border border-[#1D9E75]/60 md:block" />
          <div className="absolute left-1/2 top-1/2 hidden h-[250px] w-[250px] -translate-x-1/2 -translate-y-1/2 rounded-full border-[16px] border-zinc-200/70 md:block" />
          <div className="absolute left-1/2 top-1/2 flex h-40 w-40 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full bg-white shadow-2xl shadow-zinc-900/10">
            <BrandLogo showText={false} iconClassName="h-20 w-20" />
          </div>
          <div className="grid gap-3 pt-[430px] md:block md:pt-0">
            {growthNodes.map((node) => (
              <OrbitNode key={node.label} {...node} />
            ))}
          </div>
        </div>
      </div>
    </SectionShell>
  );
}

function OrbitNode({
  label,
  icon: Icon,
  position,
}: {
  label: string;
  icon: LucideIcon;
  position: string;
}) {
  return (
    <div className={`rounded-2xl border border-zinc-200 bg-white p-4 shadow-sm md:absolute md:w-44 ${position}`}>
      <div className="flex items-center gap-3">
        <span className="flex h-11 w-11 items-center justify-center rounded-full bg-[#1D9E75] text-white">
          <Icon className="h-5 w-5" />
        </span>
        <span className="text-sm font-bold text-zinc-900">{label}</span>
      </div>
    </div>
  );
}

function DashboardPreview() {
  return (
    <SectionShell id="preview" className="py-24 sm:py-28">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionTitle
          eyebrow="Your dashboard"
          title="SEE YOUR SWAPS"
          accent="MOVE FROM MATCH TO REPUTATION"
        />

        <div className="relative mx-auto mt-14 max-w-6xl rounded-[2rem] border border-zinc-200 bg-gradient-to-br from-[#3C2A8A] via-[#534AB7] to-[#1D9E75] p-5 shadow-2xl shadow-zinc-900/15 sm:p-8">
          <div className="rounded-2xl bg-white shadow-xl">
            <div className="flex items-center justify-between rounded-t-2xl bg-[#1D1340] px-5 py-4 text-white">
              <BrandLogo iconClassName="h-7 w-7" textClassName="text-lg text-white" />
              <div className="flex items-center gap-2 text-xs text-white/70">
                <Coins className="h-4 w-4 text-[#5DCAA5]" />
                240 credits
              </div>
            </div>
            <div className="grid gap-5 p-5 lg:grid-cols-[0.9fr_1.1fr]">
              <div className="space-y-4">
                <DashboardPanel title="Marketplace card">
                  <div className="rounded-2xl bg-zinc-900 p-4 text-white">
                    <div className="mb-8 h-24 rounded-xl bg-[url('/brand/brand-pattern.png')] bg-cover opacity-80" />
                    <div className="flex items-end justify-between gap-4">
                      <div>
                        <h3 className="font-bold">Maya Chen</h3>
                        <p className="text-sm text-white/60">@maya.designs</p>
                      </div>
                      <BadgeCheck className="h-5 w-5 fill-blue-500 text-blue-500" />
                    </div>
                    <div className="mt-4 flex flex-wrap gap-2">
                      <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-bold text-emerald-800">UX Audit</span>
                      <span className="rounded-full bg-violet-100 px-2 py-1 text-xs font-bold text-[#3C2A8A]">Motion</span>
                    </div>
                  </div>
                </DashboardPanel>
                <DashboardPanel title="Profile skills">
                  <div className="grid gap-3 sm:grid-cols-2">
                    <SkillListCard title="Offering" icon={Code2} items={["Next.js", "PostgreSQL"]} className="bg-emerald-50 text-[#0F6E56]" />
                    <SkillListCard title="Seeking" icon={Camera} items={["Photography", "Lighting"]} className="bg-violet-50 text-[#3C2A8A]" />
                  </div>
                </DashboardPanel>
              </div>

              <div className="space-y-4">
                <DashboardPanel title="Proposal status">
                  <div className="rounded-2xl border border-blue-200 bg-blue-50 p-5">
                    <div className="flex items-center justify-between gap-3">
                      <div>
                        <p className="text-sm font-bold text-blue-700">Design audit for React animation</p>
                        <p className="mt-1 text-xs text-zinc-500">Waiting for recipient response</p>
                      </div>
                      <span className="rounded-full bg-yellow-100 px-3 py-1 text-xs font-bold text-yellow-800">Pending</span>
                    </div>
                  </div>
                </DashboardPanel>
                <div className="grid gap-4 sm:grid-cols-2">
                  <PreviewStat icon={Coins} label="Swap Credits" value="240" />
                  <PreviewStat icon={Star} label="Reputation" value="4.9" />
                </div>
                <DashboardPanel title="Completion path">
                  <div className="grid gap-3">
                    {["Proposal accepted", "Session completed", "Credits awarded"].map((item, index) => (
                      <div key={item} className="flex items-center gap-3 rounded-xl bg-zinc-50 px-4 py-3">
                        <span className={`h-3 w-3 rounded-full ${index < 2 ? "bg-[#1D9E75]" : "bg-zinc-300"}`} />
                        <span className="text-sm font-semibold text-zinc-700">{item}</span>
                      </div>
                    ))}
                  </div>
                </DashboardPanel>
              </div>
            </div>
          </div>
        </div>
      </div>
    </SectionShell>
  );
}

function DashboardPanel({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="rounded-2xl border border-zinc-200 bg-white p-4">
      <p className="mb-3 text-xs font-bold uppercase text-zinc-500">{title}</p>
      {children}
    </div>
  );
}

function SkillListCard({
  title,
  icon: Icon,
  items,
  className,
}: {
  title: string;
  icon: LucideIcon;
  items: string[];
  className: string;
}) {
  return (
    <div className={`rounded-xl p-4 ${className}`}>
      <div className="flex items-center gap-2">
        <Icon className="h-4 w-4" />
        <span className="text-sm font-bold">{title}</span>
      </div>
      <div className="mt-3 space-y-2">
        {items.map((item) => (
          <p key={item} className="truncate text-sm font-semibold">
            {item}
          </p>
        ))}
      </div>
    </div>
  );
}

function PreviewStat({
  icon: Icon,
  label,
  value,
}: {
  icon: LucideIcon;
  label: string;
  value: string;
}) {
  return (
    <div className="rounded-2xl border border-zinc-200 bg-white p-5">
      <Icon className="h-5 w-5 text-[#1D9E75]" />
      <p className="mt-4 text-3xl font-bold text-zinc-900">{value}</p>
      <p className="text-sm text-zinc-500">{label}</p>
    </div>
  );
}

function FinalCTA() {
  return (
    <SectionShell className="py-24 sm:py-32" showLine={false}>
      <div className="absolute inset-x-0 bottom-0 -z-10 h-3/4 bg-[radial-gradient(circle_at_bottom,#1D9E7526,transparent_58%)]" />
      <div className="relative mx-auto max-w-6xl px-4 text-center sm:px-6">
        <div className="pointer-events-none absolute inset-0 hidden md:block">
          {floatingSkillBadges.map((badge) => (
            <span
              key={badge.label}
              aria-label={badge.label}
              title={badge.label}
              className={`absolute flex h-[86px] w-[86px] items-center justify-center rounded-3xl border shadow-xl shadow-zinc-900/10 ${badge.containerClassName} ${badge.className}`}
            >
              <badge.Logo className={badge.logoClassName} />
            </span>
          ))}
        </div>
        <h2 className="mx-auto max-w-3xl font-display text-4xl font-bold leading-tight text-zinc-900 sm:text-6xl">
          READY TO TURN YOUR SKILLS INTO CURRENCY?
        </h2>
        <p className="mx-auto mt-5 max-w-2xl text-zinc-600">
          Create your profile, list what you can teach, and start exchanging
          value with people who want to learn what you already know.
        </p>
        <Link
          href="/register"
          className="mt-8 inline-flex items-center justify-center gap-2 rounded-full bg-[#1D9E75] px-8 py-4 text-sm font-bold text-white shadow-lg shadow-emerald-900/10 transition-colors hover:bg-[#0F6E56]"
        >
          Create Your Profile
          <ChevronRight className="h-4 w-4" />
        </Link>
      </div>
    </SectionShell>
  );
}

function LandingFooter() {
  const year = new Date().getFullYear();

  return (
    <footer className="bg-[#1D1340] py-10 text-white">
      <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 sm:px-6 md:flex-row md:items-center md:justify-between">
        <div>
          <BrandLogo textClassName="text-xl text-white" />
          <p className="mt-3 max-w-md text-sm text-white/55">
            Trade talent, build trust, and grow through skill swaps.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-4 text-sm text-white/60">
          {navLinks.map((link) => (
            <Link key={link.href} href={link.href} className="transition-colors hover:text-white">
              {link.label}
            </Link>
          ))}
          <Link href="/marketplace" className="transition-colors hover:text-white">
            Marketplace
          </Link>
          <a href="#" aria-label="GitHub" className="transition-colors hover:text-white">
            <Github className="h-5 w-5" />
          </a>
        </div>
        <p className="text-sm text-white/45">© {year} Barterly</p>
      </div>
    </footer>
  );
}

export default function LandingPage() {
  return (
    <ReactLenis root>
      <main className="min-h-screen bg-[#FAFAFA] text-zinc-900">
        <LandingNav />
        <HeroSkillFlow />
        <ProblemTimeline />
        <HowItWorksCards />
        <BenefitsPanel />
        <ComparisonSection />
        <GrowthCycle />
        <DashboardPreview />
        <FinalCTA />
        <LandingFooter />
      </main>
    </ReactLenis>
  );
}
