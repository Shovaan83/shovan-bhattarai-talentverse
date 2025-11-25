"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Check } from "lucide-react";
import axiosInstance from "@/lib/axios";
import AuthLayout from "../components/AuthLayout";

const registerSchema = z.object({
  username: z.string().min(3, "Username must be at least 3 characters"),
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
  bio: z.string().optional(),
  agreeToTerms: z.boolean().optional(),
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

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
      const response = await axiosInstance.post("/api/account/register", {
        username: data.username,
        email: data.email,
        password: data.password,
        bio: data.bio || undefined,
      });

      if (response.data.token) {
        localStorage.setItem("token", response.data.token);
        router.push("/dashboard");
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message ||
        error.response?.data?.title ||
        "Registration failed. Please try again.";
      setApiError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthLayout>
      {/* Back Button */}
      <div className="mb-8">
        <button onClick={() => router.back()} className="text-gray-800 hover:text-gray-600 transition-colors">
          <ArrowLeft className="w-6 h-6" />
        </button>
      </div>

      {/* Header Section */}
      <div className="mb-8">
        <h2 className="text-4xl font-heading font-bold text-gray-900 mb-3">
          Create an Account
        </h2>
        <p className="text-sm text-gray-600 font-medium">
          Already have an account?{" "}
          <Link href="/login" className="text-orange-600 hover:text-orange-700 font-bold transition-colors">
            Log In
          </Link>
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* API Error Message */}
        {apiError && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl">
            <p className="text-sm text-red-600 font-medium">{apiError}</p>
          </div>
        )}

        {/* Username Field */}
        <div className="space-y-2">
          <label htmlFor="username" className="block text-sm font-semibold text-gray-700">
            Username
          </label>
          <input
            id="username"
            type="text"
            {...register("username")}
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-600 transition-all text-gray-900 placeholder-gray-400"
            placeholder="Choose a username"
          />
          {errors.username && (
            <p className="text-sm text-red-600 font-medium">
              {errors.username.message}
            </p>
          )}
        </div>

        {/* Email Field */}
        <div className="space-y-2">
          <label htmlFor="email" className="block text-sm font-semibold text-gray-700">
            Email
          </label>
          <input
            id="email"
            type="email"
            {...register("email")}
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-600 transition-all text-gray-900 placeholder-gray-400"
            placeholder="example@email.com"
          />
          {errors.email && (
            <p className="text-sm text-red-600 font-medium">
              {errors.email.message}
            </p>
          )}
        </div>

        {/* Password Field */}
        <div className="space-y-2">
          <label htmlFor="password" className="block text-sm font-semibold text-gray-700">
            Password
          </label>
          <input
            id="password"
            type="password"
            {...register("password")}
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-600 transition-all text-gray-900 placeholder-gray-400"
            placeholder="••••••••"
          />
          {errors.password && (
            <p className="text-sm text-red-600 font-medium">
              {errors.password.message}
            </p>
          )}
        </div>

        {/* Bio Field (Optional) */}
        <div className="space-y-2">
          <label htmlFor="bio" className="block text-sm font-semibold text-gray-700">
            Bio <span className="text-gray-400 font-normal text-xs">(Optional)</span>
          </label>
          <textarea
            id="bio"
            {...register("bio")}
            rows={3}
            className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-600 transition-all text-gray-900 placeholder-gray-400 resize-none"
            placeholder="Tell us about yourself..."
          />
          {errors.bio && (
            <p className="text-sm text-red-600 font-medium">
              {errors.bio.message}
            </p>
          )}
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={isLoading}
          className="w-full py-3.5 px-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-xl shadow-lg shadow-orange-500/30 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-orange-600 disabled:opacity-70 disabled:cursor-not-allowed transition-all text-base"
        >
          {isLoading ? "Creating account..." : "Create Account"}
        </button>

        {/* Terms Agreement */}
        <div className="flex items-center">
          <label className="flex items-center cursor-pointer group">
            <div className="relative flex items-center">
              <input
                type="checkbox"
                {...register("agreeToTerms")}
                className="peer h-5 w-5 cursor-pointer appearance-none rounded border border-gray-300 bg-white transition-all checked:border-orange-600 checked:bg-orange-600 hover:border-orange-600"
              />
              <Check className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-3.5 h-3.5 text-white opacity-0 peer-checked:opacity-100 pointer-events-none" />
            </div>
            <span className="ml-3 text-sm text-gray-600 group-hover:text-gray-900 transition-colors">
              I agree to the <span className="font-bold text-orange-600">Terms & Condition</span>
            </span>
          </label>
        </div>

        {/* Social Login */}
        <div className="pt-4 space-y-4">
          <div className="flex gap-4">
            {/* Google Button */}
            <button
              type="button"
              className="flex-1 flex items-center justify-center gap-2 py-3 px-4 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-all shadow-sm"
            >
              <svg className="w-5 h-5" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              <span className="text-sm font-medium text-gray-600">Continue with Google</span>
            </button>
          </div>
        </div>
      </form>
    </AuthLayout>
  );
}
