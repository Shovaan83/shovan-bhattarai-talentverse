"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useMutation } from "@tanstack/react-query";
import { Upload, MapPin, Link as LinkIcon, Check, Loader2 } from "lucide-react";
import api from "@/lib/axios";
import { ensureAuthToken, setAuthToken } from "@/lib/utils/auth";
import type { CompleteOnboardingPayload, ImageUploadResult, SocialLinks } from "@/lib/types/account";

export default function OnboardingPage() {
  const router = useRouter();
  const [step, setStep] = useState(1);
  const [profilePictureUrl, setProfilePictureUrl] = useState("");
  const [bio, setBio] = useState("");
  const [location, setLocation] = useState("");
  const [socialLinks, setSocialLinks] = useState<SocialLinks>({});
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [isCheckingSession, setIsCheckingSession] = useState(true);

  useEffect(() => {
    let isMounted = true;

    const initializeSession = async () => {
      const tokenFromQuery = new URLSearchParams(window.location.search).get("token");
      if (tokenFromQuery) {
        setAuthToken(tokenFromQuery);
        window.history.replaceState(null, "", "/onboarding");
      }

      const token = await ensureAuthToken();
      if (!isMounted) {
        return;
      }

      if (!token) {
        router.replace("/login");
        return;
      }

      setIsCheckingSession(false);
    };

    initializeSession();

    return () => {
      isMounted = false;
    };
  }, [router]);

  // Image upload mutation
  const uploadImageMutation = useMutation({
    mutationFn: async (file: File) => {
      const formData = new FormData();
      formData.append("file", file);
      const response = await api.post<{ data: ImageUploadResult }>("/account/upload-profile-picture", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      return response.data.data;
    },
    onSuccess: (data) => {
      setProfilePictureUrl(data.url);
      setImagePreview(data.url);
    },
  });

  // Complete onboarding mutation
  const completeOnboardingMutation = useMutation({
    mutationFn: async (payload: CompleteOnboardingPayload) => {
      const response = await api.post("/account/complete-onboarding", payload);
      return response.data;
    },
    onSuccess: (data) => {
      // Update token with new claims (IsProfileComplete = true)
      if (data?.data?.token) {
        setAuthToken(data.data.token);
      }
      router.push("/dashboard");
    },
  });

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);

      // Upload to Cloudinary
      uploadImageMutation.mutate(file);
    }
  };

  const handleComplete = () => {
    if (!profilePictureUrl || !location) {
      return;
    }

    completeOnboardingMutation.mutate({
      bio,
      location,
      profilePictureUrl,
      socialLinks: Object.keys(socialLinks).length > 0 ? socialLinks : undefined,
    });
  };

  if (isCheckingSession) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center p-4">
        <div className="flex flex-col items-center gap-3 text-zinc-600">
          <Loader2 className="w-10 h-10 animate-spin text-[#1D9E75]" />
          <p className="font-medium">Preparing onboarding...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center p-4">
      <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl border border-zinc-200 p-8">
        {/* Progress indicator */}
        <div className="flex items-center justify-between mb-8">
          {[1, 2, 3].map((s) => (
            <div key={s} className="flex items-center flex-1">
              <div
                className={`w-10 h-10 rounded-full flex items-center justify-center font-semibold ${
                  s < step
                    ? "bg-[#1D9E75] text-white"
                    : s === step
                    ? "bg-zinc-900 text-white"
                    : "bg-zinc-200 text-zinc-500"
                }`}
              >
                {s < step ? <Check className="w-5 h-5" /> : s}
              </div>
              {s < 3 && (
                <div
                  className={`flex-1 h-1 mx-2 ${
                    s < step ? "bg-[#1D9E75]" : "bg-zinc-200"
                  }`}
                />
              )}
            </div>
          ))}
        </div>

        {/* Step 1: Profile Picture */}
        {step === 1 && (
          <div className="space-y-6">
            <div className="text-center">
              <Upload className="w-12 h-12 text-[#1D9E75] mx-auto mb-4" />
              <h2 className="text-2xl font-display font-bold text-zinc-900 mb-2">
                Add Your Profile Picture
              </h2>
              <p className="text-gray-600">
                Let others see the face behind the skills! (Required)
              </p>
            </div>

            <div className="flex flex-col items-center space-y-4">
              {imagePreview ? (
                <div className="relative">
                  <img
                    src={imagePreview}
                    alt="Profile preview"
                    className="w-40 h-40 rounded-full object-cover border-4 border-[#1D9E75]"
                  />
                  <label
                    htmlFor="image-upload"
                    className="absolute bottom-0 right-0 bg-[#1D9E75] text-white p-2 rounded-full cursor-pointer hover:bg-[#178a65] transition"
                  >
                    <Upload className="w-5 h-5" />
                    <input
                      id="image-upload"
                      type="file"
                      accept="image/jpeg,image/png,image/webp"
                      onChange={handleImageChange}
                      className="hidden"
                    />
                  </label>
                </div>
              ) : (
                <label
                  htmlFor="image-upload"
                  className="w-40 h-40 rounded-full border-4 border-dashed border-zinc-200 flex items-center justify-center cursor-pointer hover:border-[#1D9E75] transition"
                >
                  <Upload className="w-10 h-10 text-gray-400" />
                  <input
                    id="image-upload"
                    type="file"
                    accept="image/jpeg,image/png,image/webp"
                    onChange={handleImageChange}
                    className="hidden"
                  />
                </label>
              )}

              {uploadImageMutation.isPending && (
                <p className="text-sm text-gray-600">Uploading...</p>
              )}
              {uploadImageMutation.isError && (
                <p className="text-sm text-red-600">
                  Upload failed. Please try again.
                </p>
              )}
              <p className="text-xs text-gray-500">
                Max 5MB • JPEG, PNG, or WebP
              </p>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setStep(2)}
                disabled={!profilePictureUrl}
                className="w-full px-6 py-3 bg-[#1D9E75] text-white rounded-lg hover:bg-[#178a65] transition disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}

        {/* Step 2: Bio & Location */}
        {step === 2 && (
          <div className="space-y-6">
            <div className="text-center">
              <MapPin className="w-12 h-12 text-[#1D9E75] mx-auto mb-4" />
              <h2 className="text-2xl font-display font-bold text-zinc-900 mb-2">
                Tell us about yourself
              </h2>
              <p className="text-gray-600">
                Help others know who you are and where you&apos;re from
              </p>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Bio
                </label>
                <textarea
                  value={bio}
                  onChange={(e) => setBio(e.target.value)}
                  placeholder="Tell us about your skills, interests, or what you're looking to learn..."
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                  rows={4}
                  maxLength={500}
                />
                <p className="text-xs text-gray-500 mt-1">{bio.length}/500</p>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Location <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={location}
                  onChange={(e) => setLocation(e.target.value)}
                  placeholder="e.g., San Francisco, CA"
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                  maxLength={200}
                />
              </div>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setStep(1)}
                className="flex-1 px-6 py-3 bg-zinc-100 rounded-lg text-zinc-700 hover:bg-zinc-200 transition"
              >
                Back
              </button>
              <button
                onClick={() => setStep(3)}
                disabled={!location.trim()}
                className="flex-1 px-6 py-3 bg-[#1D9E75] text-white rounded-lg hover:bg-[#178a65] transition disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}

        {/* Step 3: Social Links */}
        {step === 3 && (
          <div className="space-y-6">
            <div className="text-center">
              <LinkIcon className="w-12 h-12 text-[#1D9E75] mx-auto mb-4" />
              <h2 className="text-2xl font-display font-bold text-zinc-900 mb-2">
                Connect Your Profiles
              </h2>
              <p className="text-gray-600">
                Optional: Add links to your social media and portfolio
              </p>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  GitHub
                </label>
                <input
                  type="url"
                  value={socialLinks.gitHubUrl || ""}
                  onChange={(e) =>
                    setSocialLinks({ ...socialLinks, gitHubUrl: e.target.value })
                  }
                  placeholder="https://github.com/username"
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  LinkedIn
                </label>
                <input
                  type="url"
                  value={socialLinks.linkedInUrl || ""}
                  onChange={(e) =>
                    setSocialLinks({ ...socialLinks, linkedInUrl: e.target.value })
                  }
                  placeholder="https://linkedin.com/in/username"
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Twitter
                </label>
                <input
                  type="url"
                  value={socialLinks.twitterUrl || ""}
                  onChange={(e) =>
                    setSocialLinks({ ...socialLinks, twitterUrl: e.target.value })
                  }
                  placeholder="https://twitter.com/username"
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Website
                </label>
                <input
                  type="url"
                  value={socialLinks.websiteUrl || ""}
                  onChange={(e) =>
                    setSocialLinks({ ...socialLinks, websiteUrl: e.target.value })
                  }
                  placeholder="https://yourwebsite.com"
                  className="w-full px-4 py-3 bg-white border border-zinc-200 rounded-lg focus:border-[#1D9E75] focus:ring-2 focus:ring-[#1D9E75]/10 focus:outline-none transition"
                />
              </div>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setStep(2)}
                className="flex-1 px-6 py-3 bg-zinc-100 rounded-lg text-zinc-700 hover:bg-zinc-200 transition"
              >
                Back
              </button>
              <button
                onClick={handleComplete}
                disabled={completeOnboardingMutation.isPending}
                className="flex-1 px-6 py-3 bg-[#1D9E75] text-white rounded-lg hover:bg-[#178a65] transition disabled:opacity-50"
              >
                {completeOnboardingMutation.isPending
                  ? "Saving..."
                  : "Complete Setup"}
              </button>
            </div>

            {completeOnboardingMutation.isError && (
              <p className="text-sm text-red-600 text-center">
                Failed to complete onboarding. Please try again.
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
