"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { ensureAuthenticatedUser } from "@/lib/utils/auth";

export default function ProposalsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;

    const verifySession = async () => {
      const user = await ensureAuthenticatedUser();
      if (!isMounted) {
        return;
      }

      if (!user) {
        router.push("/login");
        return;
      }

      if (!user.isProfileComplete) {
        router.push("/onboarding");
        return;
      }

      setIsLoading(false);
    };

    verifySession();

    return () => {
      isMounted = false;
    };
  }, [router]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="flex flex-col items-center gap-4"
        >
          <div className="w-12 h-12 border-4 border-[#1D9E75]/30 border-t-[#1D9E75] rounded-full animate-spin"></div>
          <p className="text-zinc-600 font-medium">Loading proposals...</p>
        </motion.div>
      </div>
    );
  }

  return <>{children}</>;
}
