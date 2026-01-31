"use client";

import { useRouter } from "next/navigation";
import AuthLayout from "../components/AuthLayout";
import Enable2FA from "../components/Enable2FA";
import { useEffect, useState } from "react";
import api from "@/lib/axios";

export default function Setup2FAPage() {
  const router = useRouter();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isCheckingProfile, setIsCheckingProfile] = useState(true);

  useEffect(() => {
    const checkAuthAndProfile = async () => {
      const token = localStorage.getItem("token");
      if (!token) {
        router.push("/login");
        return;
      }

      try {
        // Check if profile is complete
        const response = await api.get("/account/me");
        if (response.data.success && response.data.data) {
          const user = response.data.data;
          
          // If profile is not complete, redirect to onboarding
          if (user.isProfileComplete === false) {
            router.push("/onboarding");
            return;
          }
        }
        
        setIsAuthenticated(true);
      } catch (error) {
        console.error("Error checking profile:", error);
        setIsAuthenticated(true); // Continue to 2FA setup anyway
      } finally {
        setIsCheckingProfile(false);
      }
    };

    checkAuthAndProfile();
  }, [router]);

  if (isCheckingProfile || !isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-gray-600">Loading...</div>
      </div>
    );
  }

  const handleSuccess = async () => {
    // After 2FA is enabled, check profile completeness again
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
        router.push("/dashboard");
      }
    } catch (error) {
      console.error("Error checking profile after 2FA:", error);
      router.push("/dashboard");
    }
  };

  const handleSkip = async () => {
    // Same logic when skipping 2FA
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
        router.push("/dashboard");
      }
    } catch (error) {
      console.error("Error checking profile after skip:", error);
      router.push("/dashboard");
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