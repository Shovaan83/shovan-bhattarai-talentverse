"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Check, User, Mail, Lock, FileText, Loader2, Eye, EyeOff } from "lucide-react";
import { motion } from "framer-motion";
import axiosInstance from "@/lib/axios";
import AuthLayout from "../components/AuthLayout";

const registerSchema = z.object({
  username: z.string().min(3, "Username must be at least 3 characters"),
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
  bio: z.string().optional(),
  agreeToTerms: z.boolean().refine((val) => val === true, {
    message: "You must agree to the terms and conditions",
  }),
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    setApiError(null);

    try {
      const payload = {
        username: data.username,
        email: data.email,
        password: data.password,
        bio: data.bio || "",
      };

      console.log("Registration payload:", payload);

      const response = await axiosInstance.post("/account/register", payload);

      console.log("Registration response:", response.data);

      if (response.data.success && response.data.data?.token) {
        localStorage.setItem("token", response.data.data.token);
        
        // Check if profile is complete
        const userData = response.data.data;
        if (userData.isProfileComplete === false) {
          // New user needs to complete onboarding
          router.push("/onboarding");
        } else {
          // Profile already complete (shouldn't happen for new users, but handle it)
          router.push("/setup-2fa");
        }
      } else {
        setApiError(response.data.message || "Registration failed");
      }
    } catch (error: any) {
      console.error("Registration error:", error.response?.data);
      
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.title ||
        error.response?.data?.errors?.[0] ||
        "Registration failed. Please try again.";
      setApiError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Join the Revolution."
      subtitle="Create your account to start trading skills, building your portfolio, and saving money today."
    >
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
      >
        {/* Back Button */}
        <div className="mb-6">
          <Link href="/" className="inline-flex items-center text-gray-500 hover:text-emerald-600 transition-colors group">
            <ArrowLeft className="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
            <span className="font-medium">Back to Home</span>
          </Link>
        </div>

        {/* Header Section */}
        <div className="mb-6">
          <h2 className="text-2xl md:text-3xl font-heading font-bold text-gray-900 mb-2">
            Create Account
          </h2>
          <p className="text-sm text-gray-600">
            Already have an account?{" "}
            <Link href="/login" className="text-orange-600 hover:text-orange-700 font-bold transition-colors hover:underline">
              Log In
            </Link>
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* API Error Message */}
          {apiError && (
            <motion.div 
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              className="p-3 bg-red-50 border border-red-100 rounded-lg flex items-start gap-2"
            >
              <div className="bg-red-100 p-1 rounded-full text-red-600 mt-0.5">
                <span className="block w-1.5 h-1.5 bg-current rounded-full" />
              </div>
              <p className="text-xs text-red-600 font-medium">{apiError}</p>
            </motion.div>
          )}

          {/* Username Field */}
          <div className="space-y-1">
            <label htmlFor="username" className="block text-xs font-semibold text-gray-700">
              Username
            </label>
            <div className="relative group">
              <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
              <input
                id="username"
                type="text"
                {...register("username")}
                className={`w-full pl-10 pr-3 py-2.5 bg-gray-50 border ${errors.username ? 'border-red-300 focus:border-red-500 focus:ring-red-200' : 'border-gray-200 focus:border-emerald-600 focus:ring-emerald-100'} rounded-lg focus:outline-none focus:ring-2 transition-all text-sm text-gray-900 placeholder-gray-400 font-medium`}
                placeholder="Write a username"
              />
            </div>
            {errors.username && (
              <p className="text-xs text-red-600 font-medium pl-1">
                {errors.username.message}
              </p>
            )}
          </div>

          {/* Email Field */}
          <div className="space-y-1">
            <label htmlFor="email" className="block text-xs font-semibold text-gray-700">
              Email Address
            </label>
            <div className="relative group">
              <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
              <input
                id="email"
                type="email"
                {...register("email")}
                className={`w-full pl-10 pr-3 py-2.5 bg-gray-50 border ${errors.email ? 'border-red-300 focus:border-red-500 focus:ring-red-200' : 'border-gray-200 focus:border-emerald-600 focus:ring-emerald-100'} rounded-lg focus:outline-none focus:ring-2 transition-all text-sm text-gray-900 placeholder-gray-400 font-medium`}
                placeholder="shovan@example.com"
              />
            </div>
            {errors.email && (
              <p className="text-xs text-red-600 font-medium pl-1">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Password Field */}
          <div className="space-y-1">
            <label htmlFor="password" className="block text-xs font-semibold text-gray-700">
              Password
            </label>
            <div className="relative group">
              <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 group-focus-within:text-emerald-600 transition-colors" />
              <input
                id="password"
                type={showPassword ? "text" : "password"}
                {...register("password")}
                className={`w-full pl-10 pr-10 py-2.5 bg-gray-50 border ${errors.password ? 'border-red-300 focus:border-red-500 focus:ring-red-200' : 'border-gray-200 focus:border-emerald-600 focus:ring-emerald-100'} rounded-lg focus:outline-none focus:ring-2 transition-all text-sm text-gray-900 placeholder-gray-400 font-medium`}
                placeholder="Min. 8 characters with special character"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showPassword ? (
                  <EyeOff className="w-4 h-4" />
                ) : (
                  <Eye className="w-4 h-4" />
                )}
              </button>
            </div>
            {errors.password && (
              <p className="text-xs text-red-600 font-medium pl-1">
                {errors.password.message}
              </p>
            )}
          </div>


          {/* Terms Agreement */}
          <div className="space-y-1">
            <label className="flex items-start cursor-pointer group select-none">
              <div className="relative flex items-center mt-1">
                <input
                  type="checkbox"
                  {...register("agreeToTerms")}
                  className="peer h-4 w-4 cursor-pointer appearance-none rounded border-2 border-gray-300 bg-white transition-all checked:border-emerald-600 checked:bg-emerald-600 hover:border-emerald-500"
                />
                <Check className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-3 h-3 text-white opacity-0 peer-checked:opacity-100 pointer-events-none" strokeWidth={3} />
              </div>
              <span className="ml-2 text-xs font-medium text-gray-600 group-hover:text-gray-900 transition-colors">
                I agree to the <span className="font-bold text-emerald-600">Terms of Service</span> and <span className="font-bold text-emerald-600">Privacy Policy</span>
              </span>
            </label>
            {errors.agreeToTerms && (
              <p className="text-xs text-red-600 font-medium pl-6">
                {errors.agreeToTerms.message}
              </p>
            )}
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-3 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-lg shadow-lg shadow-orange-600/20 hover:shadow-orange-600/30 focus:outline-none focus:ring-2 focus:ring-orange-100 disabled:opacity-70 disabled:cursor-not-allowed transition-all transform active:scale-[0.98] flex items-center justify-center gap-2 text-sm"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Creating Account...</span>
              </>
            ) : (
              "Create Account"
            )}
          </button>

          {/* Divider */}
          <div className="relative py-1">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-xs">
              <span className="px-2 bg-white text-gray-500 font-medium">Or sign up with</span>
            </div>
          </div>

          {/* Social Login */}
          <button
            type="button"
            className="w-full flex items-center justify-center gap-2 py-2.5 px-4 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 hover:border-gray-300 transition-all shadow-sm group"
          >
            <svg className="w-4 h-4 group-hover:scale-110 transition-transform" viewBox="0 0 24 24">
              <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
              <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
              <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
              <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
            </svg>
            <span className="text-sm font-bold text-gray-700 group-hover:text-gray-900">Google</span>
          </button>
        </form>
      </motion.div>
    </AuthLayout>
  );
}
