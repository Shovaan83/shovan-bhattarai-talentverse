"use client";

import { useRouter } from "next/navigation";
import AuthLayout from "../components/AuthLayout";
import Enable2FA from "../components/Enable2FA";
import { useEffect, useState } from "react";
import api from "@/lib/axios";

export default function Setup2FAPage() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const checkAuth = async () => {
      const token = localStorage.getItem("token");
      if (!token) {
        router.push("/login");
        return;
      }
      
      // Simply verify token exists, don't check profile status
      // Let the flow complete: setup-2fa → onboarding → dashboard
      setIsAuthenticated(true);
    };

    checkAuth();
  }, [router]);

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#FAFAFA]">
        <div className="text-zinc-600 font-medium">Loading...</div>
      </div>
    );
  }

  const handleSuccess = async () => {
    // ⭐ After 2FA is enabled, mark setup as complete and go to onboarding
    try {
      const response = await api.get("/account/me");
      if (response.data.success && response.data.data) {
        const user = response.data.data;
        
        // After 2FA setup, go to onboarding (if not complete) or dashboard
        if (user.isProfileComplete === false) {
          router.push("/onboarding");
        } else {
          router.push("/dashboard");
        }
      } else {
        router.push("/onboarding");
      }
    } catch (error) {
      console.error("Error checking profile after 2FA:", error);
      router.push("/onboarding");
    }
  };

  const handleSkip = async () => {
    // ⚠️ SECURITY WARNING: User chose to skip 2FA setup
    // Redirect to onboarding but IsTwoFactorSetupComplete remains false
    // Middleware will enforce 2FA setup on subsequent logins
    try {
      const response = await api.get("/account/me");
      if (response.data.success && response.data.data) {
        const user = response.data.data;
        
        if (user.isProfileComplete === false) {
          router.push("/onboarding");
        } else {
          router.push("/dashboard");
        }
      } else {
        router.push("/onboarding");
      }
    } catch (error) {
      console.error("Error checking profile after skip:", error);
      router.push("/onboarding");
    }
  };

  return (
    <AuthLayout
      title="Secure Your Account"
      subtitle="Two-factor authentication adds an extra layer of security to your TalentVerse account."
    >
      <Enable2FA
        onSuccess={handleSuccess}
        onSkip={handleSkip}
      />
    </AuthLayout>
  );
}