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

const TRANSACTION_TYPE_LABELS: Record<string, string> = {
  Earn: "Earned",
  Spend: "Spent",
  SwapReward: "Swap Reward",
  Purchase: "Purchase",
  SignupBonus: "Welcome Bonus",
  BadgeReward: "Badge Reward",
};

const TRANSACTION_TYPE_COLORS: Record<string, string> = {
  Earn: "text-emerald-600",
  Spend: "text-red-500",
  SwapReward: "text-amber-500",
  Purchase: "text-blue-500",
  SignupBonus: "text-violet-500",
  BadgeReward: "text-yellow-500",
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
    <div className="relative min-h-screen p-4 md:p-8 bg-emerald-950 text-white overflow-hidden">
      {/* Background */}
      <div className="absolute top-0 right-0 w-1/2 h-full bg-gradient-to-l from-amber-900/20 to-transparent pointer-events-none" />
      <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-amber-800/10 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        {/* Header */}
        <header className="mb-8">
          <div className="flex items-center gap-3 mb-1">
            <div className="p-2 bg-amber-500/20 rounded-xl">
              <Coins className="w-6 h-6 text-amber-400" />
            </div>
            <h1 className="text-3xl font-heading font-bold text-white">Credits</h1>
          </div>
          <p className="text-emerald-200/70 font-sans ml-1">
            Earn credits by completing swaps. Spend them however you like.
          </p>
        </header>

        {/* Wallet Stats */}
        {walletLoading ? (
          <div className="flex items-center justify-center h-36">
            <Loader2 className="w-8 h-8 animate-spin text-amber-400" />
          </div>
        ) : wallet ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8"
          >
            {[
              {
                label: "Current Balance",
                value: wallet.balance.toLocaleString(),
                icon: Coins,
                color: "from-amber-400 to-orange-500",
                suffix: " credits",
              },
              {
                label: "Total Earned",
                value: wallet.totalEarned.toLocaleString(),
                icon: TrendingUp,
                color: "from-emerald-400 to-teal-500",
                suffix: " credits",
              },
              {
                label: "Total Spent",
                value: wallet.totalSpent.toLocaleString(),
                icon: ArrowDownLeft,
                color: "from-red-400 to-rose-500",
                suffix: " credits",
              },
              {
                label: "Swaps Completed",
                value: wallet.totalSwapsCompleted,
                icon: Zap,
                color: "from-violet-400 to-purple-500",
                suffix: " swaps",
              },
            ].map((stat) => (
              <div
                key={stat.label}
                className="bg-white/10 backdrop-blur-sm rounded-2xl p-5 border border-white/10"
              >
                <div
                  className={`w-10 h-10 rounded-xl bg-gradient-to-br ${stat.color} flex items-center justify-center mb-3`}
                >
                  <stat.icon className="w-5 h-5 text-white" />
                </div>
                <p className="text-2xl font-heading font-bold text-white">
                  {stat.value}
                  <span className="text-sm font-normal text-white/50 ml-1">{stat.suffix}</span>
                </p>
                <p className="text-xs text-white/50 uppercase tracking-wider mt-1">
                  {stat.label}
                </p>
              </div>
            ))}
          </motion.div>
        ) : null}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Credit Packs */}
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.1 }}
            className="lg:col-span-1"
          >
            <div className="bg-white/10 backdrop-blur-sm rounded-2xl p-6 border border-white/10">
              <div className="flex items-center gap-2 mb-5">
                <ShoppingCart className="w-5 h-5 text-amber-400" />
                <h2 className="font-heading font-bold text-lg text-white">
                  Buy Credits
                </h2>
              </div>
              {packsLoading ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin text-amber-400" />
                </div>
              ) : (
                <div className="space-y-3">
                  {(packs ?? []).map((pack) => (
                    <div
                      key={pack.id}
                      className="relative bg-white/5 hover:bg-white/10 rounded-xl p-4 border border-white/10 hover:border-amber-400/30 transition-all"
                    >
                      {pack.badgeLabel && (
                        <span className="absolute -top-2 right-3 bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                          {pack.badgeLabel}
                        </span>
                      )}
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="font-semibold text-white">{pack.name}</p>
                          <p className="text-sm text-amber-300 font-mono">
                            {pack.credits.toLocaleString()} credits
                          </p>
                        </div>
                        <button
                          onClick={() => handlePurchase(pack.id)}
                          disabled={checkoutPending}
                          className="bg-amber-500 hover:bg-amber-400 disabled:opacity-50 text-white font-bold text-sm px-3 py-1.5 rounded-lg transition-colors"
                        >
                          ${pack.priceUsd.toFixed(2)}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              <div className="mt-4 pt-4 border-t border-white/10">
                <p className="text-xs text-white/40 text-center">
                  Secure payments via Stripe. Credits are non-refundable.
                </p>
              </div>
            </div>

            {/* Leaderboard Teaser */}
            <a href="/leaderboard" className="block mt-4">
              <div className="bg-gradient-to-br from-amber-500/20 to-orange-600/20 rounded-2xl p-5 border border-amber-400/20 hover:border-amber-400/40 transition-all group">
                <div className="flex items-center gap-2 mb-2">
                  <Trophy className="w-5 h-5 text-amber-400" />
                  <h3 className="font-heading font-bold text-white">Leaderboard</h3>
                  <ArrowUpRight className="w-4 h-4 text-amber-400 ml-auto group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-transform" />
                </div>
                <p className="text-sm text-white/60">
                  See how your credit balance ranks against the TalentVerse community.
                </p>
              </div>
            </a>
          </motion.div>

          {/* Transaction History */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.15 }}
            className="lg:col-span-2"
          >
            <div className="bg-white/10 backdrop-blur-sm rounded-2xl p-6 border border-white/10">
              <h2 className="font-heading font-bold text-lg text-white mb-5">
                Transaction History
              </h2>

              {txLoading ? (
                <div className="flex justify-center py-12">
                  <Loader2 className="w-6 h-6 animate-spin text-amber-400" />
                </div>
              ) : txData && txData.transactions.length > 0 ? (
                <>
                  <div className="space-y-2">
                    {txData.transactions.map((tx, index) => (
                      <div
                        key={`${tx.id ?? tx.transactionId ?? tx.transactionDate}-${index}`}
                        className="flex items-center gap-4 p-3 rounded-xl bg-white/5 hover:bg-white/10 transition-colors"
                      >
                        <div
                          className={`w-9 h-9 rounded-lg flex items-center justify-center shrink-0 ${
                            tx.amount > 0
                              ? "bg-emerald-500/20"
                              : "bg-red-500/20"
                          }`}
                        >
                          {tx.amount > 0 ? (
                            <ArrowUpRight className="w-4 h-4 text-emerald-400" />
                          ) : (
                            <ArrowDownLeft className="w-4 h-4 text-red-400" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-white truncate">
                            {tx.description}
                          </p>
                          <p className="text-xs text-white/40">
                            {new Date(tx.transactionDate).toLocaleDateString()}{" "}
                            &middot;{" "}
                            <span
                              className={
                                TRANSACTION_TYPE_COLORS[String(tx.type)] ??
                                "text-white/60"
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
                                ? "text-emerald-400"
                                : "text-red-400"
                            }`}
                          >
                            {tx.amount > 0 ? "+" : ""}
                            {tx.amount.toLocaleString()}
                          </p>
                          <p className="text-xs text-white/40">
                            bal: {tx.balanceAfter.toLocaleString()}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Pagination */}
                  {txData.totalPages > 1 && (
                    <div className="flex items-center justify-between mt-5 pt-4 border-t border-white/10">
                      <p className="text-xs text-white/40">
                        Page {txData.page} of {txData.totalPages}
                      </p>
                      <div className="flex gap-2">
                        <button
                          disabled={filter.page <= 1}
                          onClick={() =>
                            setFilter((f) => ({ ...f, page: f.page - 1 }))
                          }
                          className="p-1.5 rounded-lg bg-white/10 hover:bg-white/20 disabled:opacity-30 transition-colors"
                        >
                          <ChevronLeft className="w-4 h-4" />
                        </button>
                        <button
                          disabled={filter.page >= txData.totalPages}
                          onClick={() =>
                            setFilter((f) => ({ ...f, page: f.page + 1 }))
                          }
                          className="p-1.5 rounded-lg bg-white/10 hover:bg-white/20 disabled:opacity-30 transition-colors"
                        >
                          <ChevronRight className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  )}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-16 text-white/40">
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
