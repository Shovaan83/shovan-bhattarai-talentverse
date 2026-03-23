"use client";

import { motion } from "framer-motion";
import { XCircle, ArrowLeft, ShoppingCart } from "lucide-react";
import Link from "next/link";

export default function CreditsPurchaseCancelPage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-emerald-950 p-4">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.4 }}
        className="bg-white/10 backdrop-blur-sm rounded-3xl p-10 border border-white/10 max-w-md w-full text-center"
      >
        <div className="w-20 h-20 bg-red-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
          <XCircle className="w-10 h-10 text-red-400" />
        </div>
        <h1 className="text-2xl font-heading font-bold text-white mb-2">
          Purchase Cancelled
        </h1>
        <p className="text-white/60 mb-8">
          Your payment was not completed. No credits were added and you have not
          been charged.
        </p>
        <div className="flex flex-col gap-3">
          <Link href="/credits">
            <button className="w-full flex items-center justify-center gap-2 bg-amber-500 hover:bg-amber-400 text-white font-bold py-3 px-6 rounded-xl transition-colors">
              <ShoppingCart className="w-5 h-5" />
              Back to Credits
            </button>
          </Link>
          <Link href="/marketplace">
            <button className="w-full flex items-center justify-center gap-2 bg-white/10 hover:bg-white/20 text-white font-medium py-3 px-6 rounded-xl transition-colors">
              <ArrowLeft className="w-4 h-4" />
              Return to Marketplace
            </button>
          </Link>
        </div>
      </motion.div>
    </div>
  );
}
