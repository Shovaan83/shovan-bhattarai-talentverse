"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Check, Mail, Lock, Loader2, Eye, EyeOff } from "lucide-react";
import { motion } from "framer-motion";
import axiosInstance from "@/lib/axios";
import AuthLayout from "../components/AuthLayout";
import TwoFactorVerification from "../components/TwoFactorVerification";

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
  rememberMe: z.boolean().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [show2FA, setShow2FA] = useState(false);
  const [userEmail, setUserEmail] = useState("");
  const [emailSent, setEmailSent] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      rememberMe: false,
    },
  });

  // Auto-fill email if remembered
  useEffect(() => {
    const rememberMe = localStorage.getItem("rememberMe");
    const savedEmail = localStorage.getItem("userEmail");
    if (rememberMe === "true" && savedEmail) {
      setValue("email", savedEmail);
      setValue("rememberMe", true);
    }
  }, [setValue]);



  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    setApiError(null);

    try {
      const response = await axiosInstance.post("/account/login", {
        email: data.email,
        password: data.password,
      });

      if (response.data.success) {
        const userData = response.data.data;
        
        if (userData?.isTwoFactorRequired) {
          setUserEmail(data.email);
          setEmailSent(true);
          setShow2FA(true);
        } else if (userData?.token) {
          // Handle Remember Me
          if (data.rememberMe) {
            localStorage.setItem("token", userData.token);
            localStorage.setItem("rememberMe", "true");
            localStorage.setItem("userEmail", data.email);
          } else {
            localStorage.setItem("token", userData.token);
            localStorage.removeItem("rememberMe");
            localStorage.removeItem("userEmail");
          }
          router.push("/profile");
        }
      } else {
        setApiError(response.data.message || "Login failed");
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.title ||
        "Login failed. Please check your credentials.";
      setApiError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  if (show2FA) {
    return (
      <AuthLayout 
        title="Welcome back, Creator." 
        subtitle="Log in to access your profile, manage your swaps, and connect with your network."
      >
        <TwoFactorVerification 
          email={userEmail} 
          onBack={() => {
            setShow2FA(false);
            setEmailSent(false);
          }}
          emailSent={emailSent}
        />
      </AuthLayout>
    );
  }

  return (
    <AuthLayout 
      title="Welcome back, Creator." 
      subtitle="Log in to access your profile, manage your swaps, and connect with your network."
    >
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* Back Button */}
        <div className="mb-8">
          <Link href="/" className="inline-flex items-center text-gray-500 hover:text-emerald-600 transition-colors group">
            <ArrowLeft className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
            <span className="font-medium">Back to Home</span>
          </Link>
        </div>

        {/* Header Section */}
        <div className="mb-8">
          <h2 className="text-3xl md:text-4xl font-heading font-bold text-gray-900 mb-3">
            Log in
          </h2>
          <p className="text-gray-600">
            Don&apos;t have an account?{" "}
            <Link href="/register" className="text-orange-600 hover:text-orange-700 font-bold transition-colors hover:underline">
              Create an Account
            </Link>
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          {/* API Error Message */}
          {apiError && (
            <motion.div 
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              className="p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3"
            >
              <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
                <span className="block w-1.5 h-1.5 bg-current rounded-full" />
              </div>
              <p className="text-sm text-red-600 font-medium">{apiError}</p>
            </motion.div>
          )}

          {/* Email Field */}
          <div className="space-y-1.5">
            <label htmlFor="email" className="block text-sm font-semibold text-gray-700">
              Email Address
            </label>
            <div className="relative group">
              <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
              <input
                id="email"
                type="email"
                {...register("email")}
                className={`w-full pl-12 pr-4 py-3.5 bg-gray-50 border ${errors.email ? 'border-red-300 focus:border-red-500 focus:ring-red-200' : 'border-gray-200 focus:border-emerald-600 focus:ring-emerald-100'} rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                placeholder="shovan@example.com"
              />
            </div>
            {errors.email && (
              <p className="text-sm text-red-600 font-medium pl-1">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Password Field */}
          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <label htmlFor="password" className="block text-sm font-semibold text-gray-700">
                Password
              </label>
              <Link href="/forgot-password" className="text-sm font-semibold text-orange-600 hover:text-orange-700 transition-colors">
                Forgot Password?
              </Link>
            </div>
            <div className="relative group">
              <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
              <input
                id="password"
                type={showPassword ? "text" : "password"}
                {...register("password")}
                className={`w-full pl-12 pr-12 py-3.5 bg-gray-50 border ${errors.password ? 'border-red-300 focus:border-red-500 focus:ring-red-200' : 'border-gray-200 focus:border-emerald-600 focus:ring-emerald-100'} rounded-xl focus:outline-none focus:ring-4 transition-all text-gray-900 placeholder-gray-400 font-medium`}
                placeholder="••••••••"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showPassword ? (
                  <EyeOff className="w-5 h-5" />
                ) : (
                  <Eye className="w-5 h-5" />
                )}
              </button>
            </div>
            {errors.password && (
              <p className="text-sm text-red-600 font-medium pl-1">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* Remember Me */}
          <div className="flex items-center pt-1">
            <label className="flex items-center cursor-pointer group select-none">
              <div className="relative flex items-center">
                <input
                  type="checkbox"
                  {...register("rememberMe")}
                  className="peer h-5 w-5 cursor-pointer appearance-none rounded-md border-2 border-gray-300 bg-white transition-all checked:border-emerald-600 checked:bg-emerald-600 hover:border-emerald-500"
                />
                <Check className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white opacity-0 peer-checked:opacity-100 pointer-events-none" strokeWidth={3} />
              </div>
              <span className="ml-3 text-sm font-medium text-gray-600 group-hover:text-gray-900 transition-colors">
                Remember me
              </span>
            </label>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-4 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-4 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" />
                <span>Signing in...</span>
              </>
            ) : (
              "Log In"
            )}
          </button>

          {/* Divider */}
          <div className="relative py-2">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-4 bg-white text-gray-500 font-medium">Or continue with</span>
            </div>
          </div>

          {/* Social Login Buttons */}
          <div className="grid grid-cols-2 gap-3">
            {/* Google */}
            <button
              type="button"
              onClick={() => window.location.href = `${process.env.NEXT_PUBLIC_API_URL}/account/external-login/Google`}
              className="flex items-center justify-center gap-2 py-3.5 px-4 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-all shadow-sm group"
            >
              <svg className="w-5 h-5 group-hover:scale-110 transition-transform" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              <span className="text-sm font-bold text-gray-700 group-hover:text-gray-900">Google</span>
            </button>

            {/* GitHub */}
            <button
              type="button"
              onClick={() => window.location.href = `${process.env.NEXT_PUBLIC_API_URL}/account/external-login/GitHub`}
              className="flex items-center justify-center gap-2 py-3.5 px-4 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-all shadow-sm group"
            >
              <svg className="w-5 h-5 group-hover:scale-110 transition-transform" fill="currentColor" viewBox="0 0 24 24">
                <path fillRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clipRule="evenodd"/>
              </svg>
              <span className="text-sm font-bold text-gray-700 group-hover:text-gray-900">GitHub</span>
            </button>
          </div>
        </form>
      </motion.div>
    </AuthLayout>
  );
}
