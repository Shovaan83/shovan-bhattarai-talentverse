"use client";

import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { motion } from "framer-motion";
import { CheckCircle2, Coins, ArrowRight } from "lucide-react";
import Link from "next/link";
import { CREDITS_QUERY_KEY } from "@/lib/hooks/useCredits";

export default function CreditsPurchaseSuccessPage() {
  const queryClient = useQueryClient();

  // Invalidate wallet and transactions so they refresh with new balance
  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: CREDITS_QUERY_KEY });
    // Also invalidate the "me" query so the navbar credit badge updates
    queryClient.invalidateQueries({ queryKey: ["me"] });
  }, [queryClient]);

  return (
    <div className="min-h-screen flex items-start md:items-center justify-center bg-[#FAFAFA] pt-20 md:pt-0 p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="bg-white rounded-3xl p-8 sm:p-10 border border-zinc-200 max-w-md w-full text-center shadow-lg"
      >
        <div className="w-20 h-20 bg-brand-teal-50 rounded-full flex items-center justify-center mx-auto mb-6">
          <CheckCircle2 className="w-10 h-10 text-brand-teal-600" />
        </div>
        <h1 className="text-2xl font-display font-bold text-zinc-900 mb-2">
          Payment Successful!
        </h1>
        <p className="text-zinc-500 mb-8">
          Your credits have been added to your wallet. Start using them to unlock
          new opportunities on Barterly.
        </p>
        <div className="flex flex-col gap-3">
          <Link
            href="/credits"
            className="w-full inline-flex items-center justify-center gap-2 bg-zinc-900 hover:bg-zinc-800 text-white font-bold py-3 px-6 rounded-xl transition-colors"
          >
            <Coins className="w-5 h-5" />
            View My Wallet
          </Link>
          <Link
            href="/marketplace"
            className="w-full inline-flex items-center justify-center gap-2 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 font-medium py-3 px-6 rounded-xl transition-colors"
          >
            Explore Marketplace
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </motion.div>
    </div>
  );
}
