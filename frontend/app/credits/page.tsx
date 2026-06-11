"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { motion } from "framer-motion";
import {
  Coins,
  ArrowUpRight,
  ArrowDownLeft,
  TrendingUp,
  ShoppingCart,
  Trophy,
  Zap,
  ChevronLeft,
  ChevronRight,
  Loader2,
} from "lucide-react";
import { useWallet, useTransactions, useCreditPacks, useCreateCheckoutSession } from "@/lib/hooks/useCredits";
import type { TransactionFilterDto } from "@/lib/types/credits";
import { accountApi } from "@/lib/api/account";
import Link from "next/link";
import { AnimatedNumber } from "@/app/components/ui/AnimatedNumber";

const TRANSACTION_TYPE_LABELS: Record<string, string> = {
  Earn: "Earned",
  Spend: "Spent",
  SwapReward: "Swap Reward",
  Purchase: "Purchase",
  SignupBonus: "Welcome Bonus",
  BadgeReward: "Badge Reward",
};

const TRANSACTION_TYPE_COLORS: Record<string, string> = {
  Earn: "text-brand-teal-600",
  Spend: "text-red-500",
  SwapReward: "text-zinc-700",
  Purchase: "text-blue-500",
  SignupBonus: "text-zinc-600",
  BadgeReward: "text-brand-gold-600",
};

export default function CreditsPage() {
  const [filter, setFilter] = useState<TransactionFilterDto>({
    page: 1,
    pageSize: 10,
  });

  const { data: wallet, isLoading: walletLoading } = useWallet();
  const { data: txData, isLoading: txLoading } = useTransactions(filter);
  const { data: packs, isLoading: packsLoading } = useCreditPacks();
  const { mutate: createCheckout, isPending: checkoutPending } = useCreateCheckoutSession();

  // Get current user for context
  const { data: currentUser } = useQuery({
    queryKey: ["me"],
    queryFn: accountApi.getMe,
  });

  const handlePurchase = (packId: string) => {
    const origin = window.location.origin;
    createCheckout(
      {
        packId,
        successUrl: `${origin}/credits/success?session_id={CHECKOUT_SESSION_ID}`,
        cancelUrl: `${origin}/credits/cancel`,
      },
      {
        onSuccess: (session) => {
          window.location.href = session.url;
        },
      }
    );
  };

  return (
    <div className="min-h-screen bg-[#FAFAFA] text-zinc-900 p-4 md:p-8 overflow-x-hidden">
      <div className="max-w-7xl mx-auto">
        <header className="mb-8">
          <div className="flex items-center gap-3 mb-1">
            <div className="p-2 bg-zinc-100 rounded-xl">
              <Coins className="w-6 h-6 text-zinc-700" />
            </div>
            <h1 className="text-3xl font-display font-bold text-zinc-900">Credits</h1>
          </div>
          <p className="text-zinc-600 ml-1">
            Earn credits by completing swaps. Spend them however you like.
          </p>
        </header>

        {walletLoading ? (
          <div className="flex items-center justify-center h-48">
            <Loader2 className="w-8 h-8 animate-spin text-zinc-400" />
          </div>
        ) : wallet ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-8"
          >
            <div className="rounded-3xl p-6 md:p-8 bg-linear-to-br from-zinc-800 to-zinc-900 text-white shadow-xl border border-zinc-700/40">
              <div className="flex flex-col md:flex-row md:items-end md:justify-between gap-6">
                <div>
                  <p className="text-sm uppercase tracking-[0.16em] text-white/70 mb-3">Wallet Balance</p>
                  <div className="flex items-end gap-2">
                    <AnimatedNumber
                      value={wallet.balance}
                      className="text-4xl md:text-5xl font-display font-bold leading-none"
                    />
                    <span className="text-sm md:text-base text-[#FAC775] font-semibold mb-1">credits</span>
                  </div>
                  <p className="text-sm text-white/70 mt-2">
                    {currentUser?.username ? `Welcome back, ${currentUser.username}.` : "Use credits to accelerate your skill swap journey."}
                  </p>
                </div>
                <div className="flex gap-3">
                  <a href="#buy-credits" className="px-4 py-2.5 rounded-xl bg-brand-teal-500 hover:bg-brand-teal-600 text-white font-semibold transition-colors">
                    Buy Credits
                  </a>
                  <a href="#transactions" className="px-4 py-2.5 rounded-xl bg-white/10 hover:bg-white/20 text-white font-semibold transition-colors">
                    View History
                  </a>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mt-6">
                {[
                  { label: "Total Earned", value: wallet.totalEarned.toLocaleString(), icon: TrendingUp, tone: "from-brand-teal-500 to-brand-teal-700" },
                  { label: "Total Spent", value: wallet.totalSpent.toLocaleString(), icon: ArrowUpRight, tone: "from-red-500 to-red-700" },
                  { label: "Swaps Completed", value: wallet.totalSwapsCompleted.toLocaleString(), icon: Zap, tone: "from-zinc-500 to-zinc-700" },
                ].map((stat) => (
                  <div key={stat.label} className="rounded-2xl bg-white/10 border border-white/15 p-4">
                    <div className={`w-9 h-9 rounded-xl bg-linear-to-br ${stat.tone} flex items-center justify-center mb-2`}>
                      <stat.icon className="w-4 h-4 text-white" />
                    </div>
                    <p className="text-lg font-display font-semibold">{stat.value}</p>
                    <p className="text-xs text-white/70 uppercase tracking-wider">{stat.label}</p>
                  </div>
                ))}
              </div>
            </div>
          </motion.div>
        ) : null}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="lg:col-span-1"
          >
            <div id="buy-credits" className="bg-white rounded-2xl p-6 border border-zinc-200 shadow-sm">
              <div className="flex items-center gap-2 mb-5">
                <ShoppingCart className="w-5 h-5 text-zinc-600" />
                <h2 className="font-display font-bold text-lg text-zinc-900">Buy Credits</h2>
              </div>
              {packsLoading ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin text-zinc-400" />
                </div>
              ) : (
                <div className="space-y-3">
                  {(packs ?? []).map((pack) => (
                    <div
                      key={pack.id}
                      className="relative bg-zinc-50 hover:bg-zinc-100 rounded-xl p-4 border border-zinc-200 hover:border-[#1D9E75] transition-all"
                    >
                      {pack.badgeLabel && (
                        <span className="absolute -top-2 right-3 bg-[#FAC775] text-zinc-900 text-xs font-bold px-2 py-0.5 rounded-full">
                          {pack.badgeLabel}
                        </span>
                      )}
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="font-semibold text-zinc-900">{pack.name}</p>
                          <p className="text-sm text-[#EF9F27] font-mono font-semibold">
                            {pack.credits.toLocaleString()} credits
                          </p>
                        </div>
                        <button
                          onClick={() => handlePurchase(pack.id)}
                          disabled={checkoutPending}
                          className="bg-[#1D9E75] hover:bg-[#178a66] disabled:opacity-50 text-white font-bold text-sm px-3 py-1.5 rounded-lg transition-colors"
                        >
                          ${pack.priceUsd.toFixed(2)}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="mt-4 pt-4 border-t border-zinc-200">
                <p className="text-xs text-zinc-500 text-center">
                  Secure payments via Stripe. Credits are non-refundable.
                </p>
              </div>
            </div>

            <Link href="/leaderboard" className="block mt-4">
              <div className="bg-zinc-100 hover:bg-zinc-200 rounded-2xl p-5 border border-zinc-200 hover:border-zinc-300 transition-all group">
                <div className="flex items-center gap-2 mb-2">
                  <Trophy className="w-5 h-5 text-[#EF9F27]" />
                  <h3 className="font-display font-bold text-zinc-900">Leaderboard</h3>
                  <ArrowUpRight className="w-4 h-4 text-zinc-600 ml-auto group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-transform" />
                </div>
                <p className="text-sm text-zinc-600">
                  See how your credit balance ranks against the Barterly community.
                </p>
              </div>
            </Link>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.15 }}
            className="lg:col-span-2"
          >
            <div id="transactions">
              <h2 className="font-display font-bold text-lg text-zinc-900 mb-4">
                Transaction History
              </h2>

              {txLoading ? (
                <div className="flex justify-center py-12 bg-white border border-zinc-200 rounded-xl">
                  <Loader2 className="w-6 h-6 animate-spin text-zinc-400" />
                </div>
              ) : txData && txData.transactions.length > 0 ? (
                <>
                  <div className="bg-white border border-zinc-200 rounded-xl divide-y divide-zinc-100">
                    {txData.transactions.map((tx, index) => (
                      <div
                        key={`${tx.id ?? tx.transactionId ?? tx.transactionDate}-${index}`}
                        className="px-5 py-4 flex items-center gap-3 md:gap-4 hover:bg-zinc-50 transition-colors"
                      >
                        <div
                          className={`w-9 h-9 rounded-full flex items-center justify-center shrink-0 ${
                            tx.amount > 0
                              ? "bg-[#E1F5EE]"
                              : "bg-red-50"
                          }`}
                        >
                          {tx.amount > 0 ? (
                            <ArrowDownLeft className="w-4 h-4 text-[#1D9E75]" />
                          ) : (
                            <ArrowUpRight className="w-4 h-4 text-red-500" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-zinc-900 truncate">
                            {tx.description}
                          </p>
                          <p className="text-xs text-zinc-500">
                            {new Date(tx.transactionDate).toLocaleDateString()}{" "}
                            &middot;{" "}
                            <span
                              className={
                                TRANSACTION_TYPE_COLORS[String(tx.type)] ??
                                "text-zinc-600"
                              }
                            >
                              {TRANSACTION_TYPE_LABELS[String(tx.type)] ??
                                tx.typeLabel ??
                                String(tx.type)}
                            </span>
                          </p>
                        </div>
                        <div className="text-right shrink-0">
                          <p
                            className={`font-bold font-mono text-sm ${
                              tx.amount > 0
                                ? "text-[#1D9E75]"
                                : "text-red-500"
                            }`}
                          >
                            {tx.amount > 0 ? "+" : ""}
                            {tx.amount.toLocaleString()}
                          </p>
                          <p className="text-xs text-zinc-500">
                            bal: {tx.balanceAfter.toLocaleString()}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Pagination */}
                  {txData.totalPages > 1 && (
                    <div className="flex items-center justify-between mt-4 px-1">
                      <p className="text-xs text-zinc-500">
                        Page {txData.page} of {txData.totalPages}
                      </p>
                      <div className="flex gap-2">
                        <button
                          disabled={filter.page <= 1}
                          onClick={() =>
                            setFilter((f) => ({ ...f, page: f.page - 1 }))
                          }
                          className="p-1.5 rounded-lg bg-zinc-100 hover:bg-zinc-200 disabled:opacity-30 transition-colors"
                        >
                          <ChevronLeft className="w-4 h-4 text-zinc-700" />
                        </button>
                        <button
                          disabled={filter.page >= txData.totalPages}
                          onClick={() =>
                            setFilter((f) => ({ ...f, page: f.page + 1 }))
                          }
                          className="p-1.5 rounded-lg bg-zinc-100 hover:bg-zinc-200 disabled:opacity-30 transition-colors"
                        >
                          <ChevronRight className="w-4 h-4 text-zinc-700" />
                        </button>
                      </div>
                    </div>
                  )}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-16 bg-zinc-100 rounded-xl text-zinc-400">
                  <Coins className="w-12 h-12 mb-3 opacity-30" />
                  <p className="font-medium">No transactions yet</p>
                  <p className="text-sm mt-1">
                    Complete a swap or purchase credits to get started.
                  </p>
                </div>
              )}
            </div>
          </motion.div>
        </div>
      </div>
    </div>
  );
}
