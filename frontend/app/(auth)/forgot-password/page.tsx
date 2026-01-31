"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Link from "next/link";
import { ArrowLeft, Mail, Loader2, CheckCircle, KeyRound } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import axiosInstance from "@/lib/axios";
import AuthLayout from "../components/AuthLayout";

const forgotPasswordSchema = z.object({
  email: z.string().email("Invalid email address"),
});

const resetPasswordSchema = z.object({
  code: z.string().length(6, "Code must be 6 digits"),
  newPassword: z.string().min(6, "Password must be at least 6 characters"),
  confirmPassword: z.string().min(6, "Password must be at least 6 characters"),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});

type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;
type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

export default function ForgotPasswordPage() {
  const [step, setStep] = useState<"email" | "reset" | "success">("email");
  const [isLoading, setIsLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [userEmail, setUserEmail] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const emailForm = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const resetForm = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmitEmail = async (data: ForgotPasswordFormData) => {
    setIsLoading(true);
    setApiError(null);

    try {
      const response = await axiosInstance.post("/account/forgot-password", {
        email: data.email,
      });

      if (response.data.success) {
        setUserEmail(data.email);
        setStep("reset");
      } else {
        setApiError(response.data.message || "Failed to send reset code");
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message || "Failed to send reset code. Please try again.";
      setApiError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const onSubmitReset = async (data: ResetPasswordFormData) => {
    setIsLoading(true);
    setApiError(null);

    try {
      const response = await axiosInstance.post("/account/reset-password", {
        email: userEmail,
        code: data.code,
        newPassword: data.newPassword,
      });

      if (response.data.success) {
        setStep("success");
      } else {
        setApiError(response.data.message || "Failed to reset password");
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message || "Failed to reset password. Please try again.";
      setApiError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Reset Your Password"
      subtitle="Don't worry, we'll help you recover your account securely."
    >
      <AnimatePresence mode="wait">
        {step === "email" && (
          <motion.div
            key="email"
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 20 }}
            transition={{ duration: 0.3 }}
          >
            <div className="mb-8">
              <Link
                href="/login"
                className="inline-flex items-center text-gray-500 hover:text-emerald-600 transition-colors group mb-6"
              >
                <ArrowLeft className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
                <span className="font-medium">Back to Login</span>
              </Link>

              <h2 className="text-3xl md:text-4xl font-heading font-bold text-gray-900 mb-3">
                Forgot Password?
              </h2>
              <p className="text-gray-600">
                Enter your email address and we'll send you a code to reset your password.
              </p>
            </div>

            <form onSubmit={emailForm.handleSubmit(onSubmitEmail)} className="space-y-5">
              {apiError && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: "auto" }}
                  className="p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3"
                >
                  <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
                    <span className="block w-1.5 h-1.5 bg-current rounded-full" />
                  </div>
                  <p className="text-sm text-red-600 font-medium">{apiError}</p>
                </motion.div>
              )}

              <div className="space-y-1.5">
                <label htmlFor="email" className="block text-sm font-semibold text-gray-700">
                  Email Address
                </label>
                <div className="relative group">
                  <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
                  <input
                    id="email"
                    type="email"
                    {...emailForm.register("email")}
                    className={`w-full pl-12 pr-4 py-3.5 bg-gray-50 border ${
                      emailForm.formState.errors.email
                        ? "border-red-300 focus:border-red-500 focus:ring-red-200"
                        : "border-gray-200 focus:border-emerald-600 focus:ring-emerald-100"
                    } rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                    placeholder="your@example.com"
                  />
                </div>
                {emailForm.formState.errors.email && (
                  <p className="text-sm text-red-600 font-medium pl-1">
                    {emailForm.formState.errors.email.message}
                  </p>
                )}
              </div>

              <button
                type="submit"
                disabled={isLoading}
                className="w-full py-4 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-4 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" />
                    <span>Sending Code...</span>
                  </>
                ) : (
                  "Send Reset Code"
                )}
              </button>
            </form>
          </motion.div>
        )}

        {step === "reset" && (
          <motion.div
            key="reset"
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.3 }}
          >
            <div className="mb-8">
              <button
                onClick={() => setStep("email")}
                className="inline-flex items-center text-gray-500 hover:text-emerald-600 transition-colors group mb-6"
              >
                <ArrowLeft className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
                <span className="font-medium">Back</span>
              </button>

              <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 rounded-full mb-4">
                <Mail className="w-8 h-8 text-emerald-600" />
              </div>

              <h2 className="text-3xl md:text-4xl font-heading font-bold text-gray-900 mb-3">
                Check Your Email
              </h2>
              <p className="text-gray-600">
                We've sent a 6-digit code to <span className="font-semibold">{userEmail}</span>
              </p>
            </div>

            <form onSubmit={resetForm.handleSubmit(onSubmitReset)} className="space-y-5">
              {apiError && (
                <motion.div
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: "auto" }}
                  className="p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3"
                >
                  <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
                    <span className="block w-1.5 h-1.5 bg-current rounded-full" />
                  </div>
                  <p className="text-sm text-red-600 font-medium">{apiError}</p>
                </motion.div>
              )}

              <div className="space-y-1.5">
                <label htmlFor="code" className="block text-sm font-semibold text-gray-700">
                  Reset Code
                </label>
                <div className="relative group">
                  <KeyRound className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
                  <input
                    id="code"
                    type="text"
                    inputMode="numeric"
                    maxLength={6}
                    {...resetForm.register("code")}
                    className={`w-full pl-12 pr-4 py-3.5 bg-gray-50 border ${
                      resetForm.formState.errors.code
                        ? "border-red-300 focus:border-red-500 focus:ring-red-200"
                        : "border-gray-200 focus:border-emerald-600 focus:ring-emerald-100"
                    } rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                    placeholder="Enter 6-digit code"
                  />
                </div>
                {resetForm.formState.errors.code && (
                  <p className="text-sm text-red-600 font-medium pl-1">
                    {resetForm.formState.errors.code.message}
                  </p>
                )}
              </div>

              <div className="space-y-1.5">
                <label htmlFor="newPassword" className="block text-sm font-semibold text-gray-700">
                  New Password
                </label>
                <div className="relative group">
                  <input
                    id="newPassword"
                    type={showPassword ? "text" : "password"}
                    {...resetForm.register("newPassword")}
                    className={`w-full pl-4 pr-12 py-3.5 bg-gray-50 border ${
                      resetForm.formState.errors.newPassword
                        ? "border-red-300 focus:border-red-500 focus:ring-red-200"
                        : "border-gray-200 focus:border-emerald-600 focus:ring-emerald-100"
                    } rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                    placeholder="Enter new password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showPassword ? "🙈" : "👁️"}
                  </button>
                </div>
                {resetForm.formState.errors.newPassword && (
                  <p className="text-sm text-red-600 font-medium pl-1">
                    {resetForm.formState.errors.newPassword.message}
                  </p>
                )}
              </div>

              <div className="space-y-1.5">
                <label htmlFor="confirmPassword" className="block text-sm font-semibold text-gray-700">
                  Confirm Password
                </label>
                <div className="relative group">
                  <input
                    id="confirmPassword"
                    type={showConfirmPassword ? "text" : "password"}
                    {...resetForm.register("confirmPassword")}
                    className={`w-full pl-4 pr-12 py-3.5 bg-gray-50 border ${
                      resetForm.formState.errors.confirmPassword
                        ? "border-red-300 focus:border-red-500 focus:ring-red-200"
                        : "border-gray-200 focus:border-emerald-600 focus:ring-emerald-100"
                    } rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                    placeholder="Confirm new password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showConfirmPassword ? "🙈" : "👁️"}
                  </button>
                </div>
                {resetForm.formState.errors.confirmPassword && (
                  <p className="text-sm text-red-600 font-medium pl-1">
                    {resetForm.formState.errors.confirmPassword.message}
                  </p>
                )}
              </div>

              <button
                type="submit"
                disabled={isLoading}
                className="w-full py-4 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-4 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2"
              >
                {isLoading ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" />
                    <span>Resetting Password...</span>
                  </>
                ) : (
                  "Reset Password"
                )}
              </button>
            </form>
          </motion.div>
        )}

        {step === "success" && (
          <motion.div
            key="success"
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="text-center py-8"
          >
            <div className="inline-flex items-center justify-center w-20 h-20 bg-emerald-100 rounded-full mb-6">
              <CheckCircle className="w-10 h-10 text-emerald-600" />
            </div>
            <h2 className="text-3xl font-heading font-bold text-gray-900 mb-3">
              Password Reset Successfully!
            </h2>
            <p className="text-gray-600 mb-8">
              Your password has been reset. You can now login with your new password.
            </p>
            <Link
              href="/login"
              className="inline-flex items-center justify-center px-8 py-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 transition-all"
            >
              Go to Login
            </Link>
          </motion.div>
        )}
      </AnimatePresence>
    </AuthLayout>
  );
}
