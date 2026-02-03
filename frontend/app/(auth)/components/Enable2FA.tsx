"use client";

import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Shield, Loader2, Mail, CheckCircle, AlertTriangle, X } from "lucide-react";
import axiosInstance from "@/lib/axios";
import EmailStatusNotification from "./EmailStatusNotification";

interface Enable2FAProps {
  onSuccess?: () => void;
  onSkip?: () => void;
}

export default function Enable2FA({ onSuccess, onSkip }: Enable2FAProps) {
  const [step, setStep] = useState<"initial" | "code-sent" | "verify">("initial");
  const [code, setCode] = useState(["", "", "", "", "", ""]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [emailStatus, setEmailStatus] = useState<"idle" | "sending" | "sent" | "error">("idle");
  const [userEmail, setUserEmail] = useState<string>("");
  const [showSkipDialog, setShowSkipDialog] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const handleRequestCode = async () => {
    setIsLoading(true);
    setError(null);
    setEmailStatus("sending");

    try {
      const token = localStorage.getItem("token");
      const response = await axiosInstance.post(
        "/account/request-2fa-code",
        {},
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (response.data.success) {
        // Extract email from message if available
        const message = response.data.message || "";
        const emailMatch = message.match(/sent to ([^\s.]+@[^\s.]+)/);
        if (emailMatch) {
          setUserEmail(emailMatch[1]);
        }
        
        setEmailStatus("sent");
        setStep("verify");
        setTimeout(() => {
          inputRefs.current[0]?.focus();
        }, 100);
      } else {
        setEmailStatus("error");
        setError(response.data.message || "Failed to send code");
      }
    } catch (error: any) {
      setEmailStatus("error");
      const errorMessage =
        error.response?.data?.message || "Failed to send verification code";
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

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

  const handleVerifyCode = async (e: React.FormEvent) => {
    e.preventDefault();
    const verificationCode = code.join("");
    
    if (verificationCode.length !== 6) {
      setError("Please enter all 6 digits");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const token = localStorage.getItem("token");
      const response = await axiosInstance.post(
        "/account/enable-2fa",
        { code: verificationCode },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (response.data.success) {
        setSuccess(true);
        setTimeout(() => {
          onSuccess?.();
        }, 2000);
      } else {
        setError(response.data.message || "Verification failed");
        setCode(["", "", "", "", "", ""]);
        inputRefs.current[0]?.focus();
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message || "Invalid or expired code";
      setError(errorMessage);
      setCode(["", "", "", "", "", ""]);
      inputRefs.current[0]?.focus();
    } finally {
      setIsLoading(false);
    }
  };

  const handleSkipClick = () => {
    setShowSkipDialog(true);
  };

  const handleConfirmSkip = () => {
    setShowSkipDialog(false);
    onSkip?.();
  };

  const handleCancelSkip = () => {
    setShowSkipDialog(false);
  };

  if (success) {
    return (
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        className="text-center py-8"
      >
        <div className="inline-flex items-center justify-center w-20 h-20 bg-emerald-100 rounded-full mb-6">
          <CheckCircle className="w-10 h-10 text-emerald-600" />
        </div>
        <h3 className="text-2xl font-bold text-gray-900 mb-2">
          2FA Enabled Successfully!
        </h3>
        <p className="text-gray-600">
          Your account is now protected with two-factor authentication.
        </p>
      </motion.div>
    );
  }

  if (step === "verify") {
    return (
      <motion.div
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        className="w-full"
      >
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 rounded-full mb-4">
            <Mail className="w-8 h-8 text-emerald-600" />
          </div>
          <h3 className="text-2xl font-heading font-bold text-gray-900 mb-2">
            Check Your Email
          </h3>
          <p className="text-gray-600">
            We've sent a 6-digit verification code to your email
          </p>
        </div>

        <form onSubmit={handleVerifyCode} className="space-y-6">
          {/* Email Status Notification */}
          <EmailStatusNotification 
            status={emailStatus} 
            email={userEmail}
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

          <div className="space-y-3">
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
                "Enable 2FA"
              )}
            </button>

            {onSkip && (
              <button
                type="button"
                onClick={handleSkipClick}
                className="w-full py-3 text-sm text-gray-600 hover:text-gray-900 font-medium transition-colors"
              >
                Skip for now
              </button>
            )}
          </div>
        </form>
      </motion.div>
    );
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="w-full"
    >
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 rounded-full mb-4">
          <Shield className="w-8 h-8 text-emerald-600" />
        </div>
        <h3 className="text-2xl font-heading font-bold text-gray-900 mb-2">
          Enable Two-Factor Authentication
        </h3>
        <p className="text-gray-600 max-w-md mx-auto">
          Add an extra layer of security to your account. We'll send a verification code to your email whenever you log in.
        </p>
      </div>

      {error && (
        <motion.div
          initial={{ opacity: 0, height: 0 }}
          animate={{ opacity: 1, height: "auto" }}
          className="mb-6 p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3"
        >
          <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
            <span className="block w-1.5 h-1.5 bg-current rounded-full" />
          </div>
          <p className="text-sm text-red-600 font-medium">{error}</p>
        </motion.div>
      )}

      <div className="space-y-4 mb-8">
        <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-xl">
          <div className="flex-shrink-0 w-6 h-6 bg-emerald-600 rounded-full flex items-center justify-center text-white text-sm font-bold">
            1
          </div>
          <div>
            <h4 className="font-semibold text-gray-900 mb-1">Receive Code</h4>
            <p className="text-sm text-gray-600">
              We'll send a 6-digit code to your registered email address
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-xl">
          <div className="flex-shrink-0 w-6 h-6 bg-emerald-600 rounded-full flex items-center justify-center text-white text-sm font-bold">
            2
          </div>
          <div>
            <h4 className="font-semibold text-gray-900 mb-1">Verify Code</h4>
            <p className="text-sm text-gray-600">
              Enter the code to complete 2FA setup
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 p-4 bg-gray-50 rounded-xl">
          <div className="flex-shrink-0 w-6 h-6 bg-emerald-600 rounded-full flex items-center justify-center text-white text-sm font-bold">
            3
          </div>
          <div>
            <h4 className="font-semibold text-gray-900 mb-1">Secure Login</h4>
            <p className="text-sm text-gray-600">
              You'll need to verify your identity on each login
            </p>
          </div>
        </div>
      </div>

      <div className="space-y-3">
        <button
          onClick={handleRequestCode}
          disabled={isLoading}
          className="w-full py-4 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-4 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2"
        >
          {isLoading ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>Sending Code...</span>
            </>
          ) : (
            <>
              <Shield className="w-5 h-5" />
              <span>Enable 2FA</span>
            </>
          )}
        </button>

        {onSkip && (
          <button
            type="button"
            onClick={handleSkipClick}
            className="w-full py-3 text-sm text-gray-600 hover:text-gray-900 font-medium transition-colors"
          >
            I'll do this later
          </button>
        )}
      </div>

      {/* Skip Confirmation Dialog */}
      <AnimatePresence>
        {showSkipDialog && (
          <>
            {/* Backdrop */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={handleCancelSkip}
              className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            />

            {/* Dialog */}
            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
              <motion.div
                initial={{ opacity: 0, scale: 0.95, y: 20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.95, y: 20 }}
                className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 relative"
                onClick={(e) => e.stopPropagation()}
              >
                {/* Close Button */}
                <button
                  onClick={handleCancelSkip}
                  className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>

                {/* Icon */}
                <div className="flex justify-center mb-4">
                  <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center">
                    <AlertTriangle className="w-8 h-8 text-amber-600" />
                  </div>
                </div>

                {/* Title */}
                <h3 className="text-2xl font-bold text-gray-900 text-center mb-3">
                  Skip 2FA Setup?
                </h3>

                {/* Warning Message */}
                <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-6">
                  <p className="text-sm text-amber-900 leading-relaxed">
                    <strong className="font-semibold">Security Warning:</strong> Two-factor authentication (2FA) 
                    significantly protects your account from unauthorized access. Without 2FA, your account is more 
                    vulnerable to security breaches. We strongly recommend enabling it now.
                  </p>
                </div>

                {/* Buttons */}
                <div className="flex gap-3">
                  <button
                    onClick={handleCancelSkip}
                    className="flex-1 py-3 px-4 bg-emerald-600 text-white rounded-lg font-semibold hover:bg-emerald-700 transition-colors"
                  >
                    Setup 2FA
                  </button>
                  <button
                    onClick={handleConfirmSkip}
                    className="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                  >
                    Skip Anyway
                  </button>
                </div>

                {/* Footer Note */}
                <p className="text-xs text-gray-500 text-center mt-4">
                  You can enable 2FA later from your profile settings
                </p>
              </motion.div>
            </div>
          </>
        )}
      </AnimatePresence>
    </motion.div>
  );
}
