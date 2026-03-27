"use client";

import { useState } from "react";
import { Eye, EyeOff, Lock } from "lucide-react";
import Link from "next/link";
import type { UseFormRegisterReturn } from "react-hook-form";

interface PasswordInputProps {
  id: string;
  label: string;
  placeholder: string;
  error?: string;
  registration: UseFormRegisterReturn;
  showForgotLink?: boolean;
  forgotHref?: string;
  forgotLabel?: string;
  size?: "md" | "sm";
}

export default function PasswordInput({
  id,
  label,
  placeholder,
  error,
  registration,
  showForgotLink = false,
  forgotHref = "/forgot-password",
  forgotLabel = "Forgot Password?",
  size = "md",
}: PasswordInputProps) {
  const [showPassword, setShowPassword] = useState(false);
  const compact = size === "sm";

  return (
    <div className={compact ? "space-y-1" : "space-y-1.5"}>
      <div className="flex items-center justify-between">
        <label htmlFor={id} className={compact ? "block text-xs font-semibold text-gray-700" : "block text-sm font-semibold text-gray-700"}>
          {label}
        </label>
        {showForgotLink ? (
          <Link href={forgotHref} className="text-sm font-semibold text-orange-600 hover:text-orange-700 transition-colors">
            {forgotLabel}
          </Link>
        ) : null}
      </div>

      <div className="relative group">
        <Lock
          className={compact
            ? "absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 group-focus-within:text-[#1D9E75] transition-colors"
            : "absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 group-focus-within:text-[#1D9E75] transition-colors"}
        />
        <input
          id={id}
          type={showPassword ? "text" : "password"}
          {...registration}
          className={`${compact ? "w-full pl-10 pr-10 py-2.5 text-sm rounded-lg focus:ring-2" : "w-full pl-12 pr-12 py-3.5 rounded-xl focus:ring-4"} bg-zinc-50 border ${
            error
              ? "border-red-300 focus:border-red-500 focus:ring-red-200"
              : "border-zinc-200 focus:border-[#1D9E75] focus:ring-[#1D9E75]/10"
          } focus:outline-none transition-all text-zinc-900 placeholder-gray-400 font-medium`}
          placeholder={placeholder}
        />
        <button
          type="button"
          onClick={() => setShowPassword((prev) => !prev)}
          className={compact
            ? "absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
            : "absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"}
          aria-label={showPassword ? "Hide password" : "Show password"}
        >
          {showPassword ? <EyeOff className={compact ? "w-4 h-4" : "w-5 h-5"} /> : <Eye className={compact ? "w-4 h-4" : "w-5 h-5"} />}
        </button>
      </div>

      {error ? <p className={compact ? "text-xs text-red-600 font-medium pl-1" : "text-sm text-red-600 font-medium pl-1"}>{error}</p> : null}
    </div>
  );
}
