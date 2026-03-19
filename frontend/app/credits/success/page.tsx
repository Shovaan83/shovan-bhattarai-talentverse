"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
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
    <div className="min-h-screen flex items-center justify-center bg-emerald-950 p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="bg-white/10 backdrop-blur-sm rounded-3xl p-10 border border-white/10 max-w-md w-full text-center"
      >
        <div className="w-20 h-20 bg-emerald-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
          <CheckCircle2 className="w-10 h-10 text-emerald-400" />
        </div>
        <h1 className="text-2xl font-heading font-bold text-white mb-2">
          Payment Successful!
        </h1>
        <p className="text-white/60 mb-8">
          Your credits have been added to your wallet. Start using them to unlock
          new opportunities on TalentVerse.
        </p>
        <div className="flex flex-col gap-3">
          <Link href="/credits">
            <button className="w-full flex items-center justify-center gap-2 bg-amber-500 hover:bg-amber-400 text-white font-bold py-3 px-6 rounded-xl transition-colors">
              <Coins className="w-5 h-5" />
              View My Wallet
            </button>
          </Link>
          <Link href="/marketplace">
            <button className="w-full flex items-center justify-center gap-2 bg-white/10 hover:bg-white/20 text-white font-medium py-3 px-6 rounded-xl transition-colors">
              Explore Marketplace
              <ArrowRight className="w-4 h-4" />
            </button>
          </Link>
        </div>
      </motion.div>
    </div>
  );
}
