"use client";

import { motion } from "framer-motion";
import { XCircle, ArrowLeft, ShoppingCart } from "lucide-react";
import Link from "next/link";

export default function CreditsPurchaseCancelPage() {
  return (
    <div className="min-h-screen flex items-start md:items-center justify-center bg-[#FAFAFA] pt-20 md:pt-0 p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="bg-white rounded-3xl p-8 sm:p-10 border border-zinc-200 max-w-md w-full text-center shadow-lg"
      >
        <div className="w-20 h-20 bg-red-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
          <XCircle className="w-10 h-10 text-red-400" />
        </div>
        <h1 className="text-2xl font-display font-bold text-zinc-900 mb-2">
          Purchase Cancelled
        </h1>
        <p className="text-zinc-500 mb-8">
          Your payment was not completed. No credits were added and you have not
          been charged.
        </p>
        <div className="flex flex-col gap-3">
          <Link
            href="/credits"
            className="w-full inline-flex items-center justify-center gap-2 bg-zinc-900 hover:bg-zinc-800 text-white font-bold py-3 px-6 rounded-xl transition-colors"
          >
            <ShoppingCart className="w-5 h-5" />
            Back to Credits
          </Link>
          <Link
            href="/marketplace"
            className="w-full inline-flex items-center justify-center gap-2 bg-zinc-100 hover:bg-zinc-200 text-zinc-700 font-medium py-3 px-6 rounded-xl transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            Return to Marketplace
          </Link>
        </div>
      </motion.div>
    </div>
  );
}
