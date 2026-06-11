"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { ensureAuthenticatedUser } from "@/lib/utils/auth";

export default function ProfileLayout({
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
      <div className="min-h-screen bg-gray-50/50 flex items-center justify-center">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="flex flex-col items-center gap-4"
        >
          <div className="w-12 h-12 border-4 border-emerald-200 border-t-emerald-500 rounded-full animate-spin"></div>
          <p className="text-gray-500 font-medium">Loading your profile...</p>
        </motion.div>
      </div>
    );
  }

  return <>{children}</>;
}
