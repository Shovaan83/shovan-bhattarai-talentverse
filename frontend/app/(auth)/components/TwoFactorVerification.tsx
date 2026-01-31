"use client";

import { useState, useRef, useEffect } from "react";
import { motion } from "framer-motion";
import { Shield, Loader2, ArrowLeft } from "lucide-react";
import axiosInstance from "@/lib/axios";
import { useRouter } from "next/navigation";
import EmailStatusNotification from "./EmailStatusNotification";

interface TwoFactorVerificationProps {
  email: string;
  onBack: () => void;
  emailSent?: boolean;
}

export default function TwoFactorVerification({ email, onBack, emailSent = true }: TwoFactorVerificationProps) {
  const router = useRouter();
  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [emailStatus, setEmailStatus] = useState<"idle" | "sending" | "sent" | "error">(
    emailSent ? "sent" : "idle"
  );
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  useEffect(() => {
    inputRefs.current[0]?.focus();
  }, []);

  const handleChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;

    const newCode = [...code];
    newCode[index] = value.slice(-1);
    setCode(newCode);
    setError(null);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace" && !code[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData("text").slice(0, 6);
    
    if (!/^\d+$/.test(pastedData)) return;

    const newCode = [...code];
    for (let i = 0; i < pastedData.length; i++) {
      newCode[i] = pastedData[i];
    }
    setCode(newCode);
    
    const nextIndex = Math.min(pastedData.length, 5);
    inputRefs.current[nextIndex]?.focus();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const verificationCode = code.join("");
    
    if (verificationCode.length !== 6) {
      setError("Please enter all 6 digits");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const response = await axiosInstance.post("/account/login-2fa", {
        email,
        code: verificationCode,
      });

      if (response.data.success && response.data.data?.token) {
        localStorage.setItem("token", response.data.data.token);
        router.push("/profile");
      } else {
        setError(response.data.message || "Verification failed");
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message ||
        "Invalid or expired code. Please try again.";
      setError(errorMessage);
      setCode(["", "", "", "", "", ""]);
      inputRefs.current[0]?.focus();
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, x: 20 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.3 }}
      className="w-full"
    >
      <button
        onClick={onBack}
        className="mb-6 inline-flex items-center text-gray-500 hover:text-emerald-600 transition-colors group"
      >
        <ArrowLeft className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
        <span className="font-medium">Back to Login</span>
      </button>

      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 rounded-full mb-4">
          <Shield className="w-8 h-8 text-emerald-600" />
        </div>
        <h2 className="text-3xl font-heading font-bold text-gray-900 mb-2">
          Two-Factor Authentication
        </h2>
        <p className="text-gray-600">
          We've sent a 6-digit code to <span className="font-semibold">{email}</span>
        </p>
        <p className="text-sm text-gray-500 mt-1">
          Please check your email and enter the code below
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Email Status Notification */}
        <EmailStatusNotification 
          status={emailStatus} 
          email={email}
        />

        {error && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: "auto" }}
            className="p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3"
          >
            <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
              <span className="block w-1.5 h-1.5 bg-current rounded-full" />
            </div>
            <p className="text-sm text-red-600 font-medium">{error}</p>
          </motion.div>
        )}

        <div className="flex gap-3 justify-center">
          {code.map((digit, index) => (
            <input
              key={index}
              ref={(el) => { inputRefs.current[index] = el; }}
              type="text"
              inputMode="numeric"
              maxLength={1}
              value={digit}
              onChange={(e) => handleChange(index, e.target.value)}
              onKeyDown={(e) => handleKeyDown(index, e)}
              onPaste={index === 0 ? handlePaste : undefined}
              className="w-12 h-14 text-center text-2xl font-bold bg-gray-50 border-2 border-gray-200 rounded-xl focus:border-emerald-600 focus:ring-4 focus:ring-emerald-100 focus:outline-none transition-all"
              disabled={isLoading}
            />
          ))}
        </div>

        <button
          type="submit"
          disabled={isLoading || code.some((d) => !d)}
          className="w-full py-4 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-4 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2"
        >
          {isLoading ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>Verifying...</span>
            </>
          ) : (
            "Verify Code"
          )}
        </button>

        <div className="text-center">
          <button
            type="button"
            onClick={onBack}
            className="text-sm text-gray-600 hover:text-emerald-600 font-medium transition-colors"
          >
            Didn't receive the code? Try logging in again
          </button>
        </div>
      </form>
    </motion.div>
  );
}
