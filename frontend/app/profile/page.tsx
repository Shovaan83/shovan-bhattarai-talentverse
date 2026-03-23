"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { motion, type Variants } from "framer-motion";
import axios from "axios";
import Link from "next/link";
import {
  Plus,
  MapPin,
  Link as LinkIcon,
  Twitter,
  Github,
  Edit2,
  Zap,
  Target,
  Trash2,
  Settings,
  Star,
  Award,
} from "lucide-react";
import { accountApi } from "@/lib/api/account";
import { skillsApi } from "@/lib/api/skills";
import type { AddSkillPayload } from "@/lib/types/skills";
import type { UserSkill } from "@/lib/types/skills";
import SkillModal, { SkillType } from "./components/SkillModal";
import EditProfileModal from "./components/EditProfileModal";
import LinkedAccountsSettings from "./components/LinkedAccountsSettings";
import VerificationRequestForm from "./components/VerificationRequestForm";
import { useUserReputation, useUserReviews } from "@/lib/hooks/useReviews";
import ReputationBadge from "@/app/components/reviews/ReputationBadge";
import ReviewList from "@/app/components/reviews/ReviewList";
import { StatsStrip } from "./components/StatsStrip";
import { useAllBadges } from "@/lib/hooks/useBadges";
import BadgeGrid from "@/app/components/badges/BadgeGrid";

const containerVariants: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
    },
  },
};

const itemVariants: Variants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: { duration: 0.5, ease: [0.25, 0.1, 0.25, 1] as const },
  },
};

export default function ProfilePage() {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [activeModalType, setActiveModalType] = useState<SkillType>(
    SkillType.Offer
  );
  const [isEditProfileOpen, setIsEditProfileOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<'overview' | 'settings'>('overview');

  const {
    data: user,
    isLoading: userLoading,
    isError: userError,
  } = useQuery({
    queryKey: ["me"],
    queryFn: accountApi.getMe,
  });

  const {
    data: skills,
    isLoading: skillsLoading,
    isError: skillsError,
  } = useQuery({
    queryKey: ["my-skills"],
    queryFn: skillsApi.getMySkills,
  });

  const { data: userReputation } = useUserReputation(user?.id ?? '');
  const { data: userReviews, isLoading: reviewsLoading } = useUserReviews(user?.id ?? '');
  const { data: allBadges, isLoading: badgesLoading } = useAllBadges();

  const createSkillMutation = useMutation({
    mutationFn: skillsApi.addSkill,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["my-skills"] });
      setIsModalOpen(false);
    },
  });

  const updateProfileMutation = useMutation({
    mutationFn: accountApi.updateMe,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["me"] });
      setIsEditProfileOpen(false);
    },
  });

  const updateProfileErrorMessage = (() => {
    if (!updateProfileMutation.isError) return null;

    const err = updateProfileMutation.error;
    if (axios.isAxiosError(err)) {
      const data = err.response?.data as
        | { message?: string; errors?: string[] }
        | undefined;
      return (
        data?.message ||
        data?.errors?.[0] ||
        (typeof err.response?.status === "number"
          ? `Failed to update profile (HTTP ${err.response.status}).`
          : "Failed to update profile.")
      );
    }

    return "Failed to update profile.";
  })();

  const deleteSkillMutation = useMutation({
    mutationFn: skillsApi.deleteSkill,
    onMutate: async (deletedId) => {
      await queryClient.cancelQueries({ queryKey: ["my-skills"] });

      const previousSkills = queryClient.getQueryData<UserSkill[]>(["my-skills"]);

      queryClient.setQueryData<UserSkill[]>(["my-skills"], (old) =>
        (old ?? []).filter((skill) => skill.userSkillId !== deletedId)
      );

      return { previousSkills };
    },
    onError: (_err, _deletedId, context) => {
      queryClient.setQueryData(["my-skills"], context?.previousSkills);
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["my-skills"] });
    },
  });

  const skillsArray = Array.isArray(skills) ? skills : [];
  const offers = skillsArray.filter((s) => s.type === "Offer");
  const wants = skillsArray.filter((s) => s.type === "Want");

  const handleOpenModal = (type: SkillType) => {
    setActiveModalType(type);
    setIsModalOpen(true);
  };

  const isLoading = userLoading || skillsLoading;
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-emerald-950 text-white selection:bg-emerald-200 selection:text-emerald-950">
        <div className="w-12 h-12 border-4 border-emerald-500 rounded-full animate-spin border-t-transparent" />
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen p-4 md:p-8 bg-emerald-950 text-white selection:bg-emerald-200 selection:text-emerald-950">
        <div className="max-w-7xl mx-auto">
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-center">
            Failed to load your profile. Please login again.
          </div>
        </div>
      </div>
    );
  }

  const displayName = user.username;
  const handle = user.username ? `@${user.username}` : "@user";
  const bio = user.bio ?? "";
  const avatarUrl = user.profilePictureUrl ?? "";

  const credits = user.creditBalance ?? 0;
  const averageRating = userReputation?.averageRating ?? 0;
  const totalReviews = userReputation?.totalReviews ?? 0;
  const hasMinimumReviews = userReputation?.hasMinimumReviews ?? false;
  const totalSwaps = userReputation?.completedSwaps ?? 0;

  return (
    <div className="relative min-h-screen p-4 md:p-8 bg-emerald-950 text-white overflow-hidden selection:bg-emerald-200 selection:text-emerald-950">
      {/* Hero-style background */}
      <div className="absolute top-0 right-0 w-1/2 h-full bg-gradient-to-l from-emerald-900/50 to-transparent pointer-events-none" />
      <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-emerald-800/20 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        <header className="mb-8">
          <h1 className="text-3xl font-heading font-bold text-white">
            My Profile
          </h1>
          <p className="text-emerald-200/80 font-sans">
            Manage your identity and skill portfolio
          </p>
        </header>

        {/* Tabs */}
        <div className="flex gap-4 mb-8">
          <button
            onClick={() => setActiveTab('overview')}
            className={`px-6 py-3 rounded-xl font-bold transition-all ${
              activeTab === 'overview'
                ? 'bg-emerald-600 text-white shadow-lg'
                : 'bg-emerald-900/50 text-emerald-200 hover:bg-emerald-800/50'
            }`}
          >
            Overview
          </button>
          <button
            onClick={() => setActiveTab('settings')}
            className={`px-6 py-3 rounded-xl font-bold transition-all flex items-center gap-2 ${
              activeTab === 'settings'
                ? 'bg-emerald-600 text-white shadow-lg'
                : 'bg-emerald-900/50 text-emerald-200 hover:bg-emerald-800/50'
            }`}
          >
            <Settings className="w-4 h-4" />
            Settings
          </button>
        </div>

        {/* Overview Tab */}
        {activeTab === 'overview' && (

        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="visible"
          className="grid grid-cols-1 md:grid-cols-12 gap-6 auto-rows-[minmax(180px,auto)]"
        >
          <motion.div
            variants={itemVariants}
            className="col-span-1 md:col-span-4 lg:col-span-3 row-span-2 relative group"
          >
            <div className="h-full bg-white rounded-3xl shadow-2xl shadow-black/20 overflow-hidden border border-white/10 flex flex-col sticky top-6">
              <div className="h-32 bg-gradient-to-br from-emerald-900 to-emerald-950" />
              <div className="px-6 pb-6 flex-1 flex flex-col relative">
                <div className="absolute -top-12 left-6">
                  {avatarUrl ? (
                    <img
                      src={avatarUrl}
                      alt={displayName}
                      className="w-24 h-24 rounded-2xl border-4 border-white shadow-md object-cover"
                    />
                  ) : (
                    <div className="w-24 h-24 rounded-2xl border-4 border-white shadow-md bg-white flex items-center justify-center">
                      <span className="font-heading font-bold text-2xl text-gray-400">
                        {displayName.slice(0, 1).toUpperCase()}
                      </span>
                    </div>
                  )}
                </div>

                <div className="mt-14 mb-4">
                  <h2 className="text-xl font-heading font-bold text-gray-900">
                    {displayName}
                  </h2>
                  <p className="text-sm text-gray-500 font-medium">
                    {handle}
                  </p>
                  {userReputation && (
                    <div className="mt-2">
                      <ReputationBadge
                        averageRating={userReputation.averageRating}
                        totalReviews={userReputation.totalReviews}
                        hasMinimumReviews={userReputation.hasMinimumReviews}
                        size="sm"
                        showCount={true}
                      />
                    </div>
                  )}
                </div>

                <p className="text-gray-600 text-sm leading-relaxed mb-6 font-sans">
                  {bio}
                </p>

                <div className="space-y-3 mb-8">
                  <div className="flex items-center gap-3 text-sm text-gray-500">
                    <MapPin size={16} />
                    <span>Itahari, Sunsari</span>
                  </div>
                  <div className="flex items-center gap-3 text-sm text-gray-500">
                    <LinkIcon size={16} />
                    <a
                      href="#"
                      className="hover:text-orange-500 transition-colors"
                    >
                      shovan.com.np
                    </a>
                  </div>
                </div>

                <div className="flex gap-4 mt-auto">
                  <button
                    className="p-2 rounded-full bg-gray-100 text-gray-600 hover:bg-gray-200 transition-colors"
                    type="button"
                  >
                    <Twitter size={18} />
                  </button>
                  <button
                    className="p-2 rounded-full bg-gray-100 text-gray-600 hover:bg-gray-200 transition-colors"
                    type="button"
                  >
                    <Github size={18} />
                  </button>
                </div>

                <button
                  className="mt-6 w-full py-2.5 flex items-center justify-center gap-2 rounded-xl bg-orange-600 hover:bg-orange-700 text-sm font-semibold text-white transition-all"
                  type="button"
                  onClick={() => setIsEditProfileOpen(true)}
                >
                  <Edit2 size={14} />
                  Edit Profile
                </button>
              </div>
            </div>
          </motion.div>

          <motion.div
            variants={itemVariants}
            className="col-span-1 md:col-span-8 lg:col-span-9"
          >
            <StatsStrip
              swapCredits={credits}
              averageRating={averageRating}
              totalReviews={totalReviews}
              hasMinimumReviews={hasMinimumReviews}
              totalSwaps={totalSwaps}
            />
          </motion.div>

          <motion.div
            variants={itemVariants}
            className="col-span-1 md:col-span-4 lg:col-span-5 relative group overflow-hidden"
          >
            <div className="h-full bg-white rounded-3xl border border-gray-100 p-6 flex flex-col relative z-0 transition-all hover:border-gray-200 hover:shadow-lg hover:shadow-black/10">
              <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-2">
                  <div className="p-2 bg-emerald-100 text-emerald-600 rounded-lg">
                    <Zap size={18} fill="currentColor" />
                  </div>
                  <h3 className="font-heading font-bold text-lg text-gray-900">
                    Offers
                  </h3>
                </div>
                <button
                  onClick={() => handleOpenModal(SkillType.Offer)}
                  className="p-2 rounded-full bg-gray-50 text-emerald-600 shadow-sm border border-gray-100 hover:bg-emerald-50 transition-colors"
                  type="button"
                >
                  <Plus size={20} />
                </button>
              </div>

              {offers.length === 0 ? (
                <div className="flex-1 flex flex-col items-center justify-center text-gray-500 py-10">
                  <Zap size={48} className="mb-2 opacity-20" />
                  <p className="text-sm font-medium">No skills offered yet.</p>
                </div>
              ) : (
                <div className="space-y-3 overflow-y-auto custom-scrollbar pr-2 max-h-[400px]">
                  {offers.map((skill) => (
                    <div
                      key={skill.userSkillId}
                      className="group/card bg-white p-4 rounded-2xl border border-emerald-100 shadow-sm hover:shadow-md hover:-translate-y-1 transition-all duration-300 relative"
                    >
                      <div className="flex justify-between items-start mb-2">
                        <span className="text-[10px] font-bold uppercase tracking-wider text-emerald-600 bg-emerald-50 px-2 py-1 rounded-md">
                          {skill.category}
                        </span>
                        <button
                          onClick={() =>
                            deleteSkillMutation.mutate(skill.userSkillId)
                          }
                          className="text-gray-300 hover:text-red-500 transition-colors opacity-0 group-hover/card:opacity-100"
                          type="button"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                      <h4 className="font-bold text-gray-800 text-lg mb-1">
                        {skill.skillName}
                      </h4>
                      <p className="text-sm text-gray-500 leading-snug">
                        {skill.description}
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </motion.div>

          <motion.div
            variants={itemVariants}
            className="col-span-1 md:col-span-4 lg:col-span-4 relative group overflow-hidden"
          >
            <div className="h-full bg-white rounded-3xl border border-gray-100 p-6 flex flex-col relative z-0 transition-all hover:border-gray-200 hover:shadow-lg hover:shadow-black/10">
              <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-2">
                  <div className="p-2 bg-orange-100 text-orange-600 rounded-lg">
                    <Target size={18} />
                  </div>
                  <h3 className="font-heading font-bold text-lg text-gray-900">
                    Wants
                  </h3>
                </div>
                <button
                  onClick={() => handleOpenModal(SkillType.Want)}
                  className="p-2 rounded-full bg-gray-50 text-orange-600 shadow-sm border border-gray-100 hover:bg-orange-50 transition-colors"
                  type="button"
                >
                  <Plus size={20} />
                </button>
              </div>

              {wants.length === 0 ? (
                <div className="flex-1 flex flex-col items-center justify-center text-gray-500 py-10">
                  <Target size={48} className="mb-2 opacity-20" />
                  <p className="text-sm font-medium">No skills requested yet.</p>
                </div>
              ) : (
                <div className="flex flex-col gap-3 overflow-y-auto custom-scrollbar pr-2 max-h-[400px]">
                  {wants.map((skill) => (
                    <div
                      key={skill.userSkillId}
                      className="group/card bg-white p-3 rounded-xl border border-orange-100 shadow-sm hover:shadow-md hover:scale-[1.02] transition-all duration-300 flex items-center justify-between"
                    >
                      <div>
                        <h4 className="font-semibold text-gray-800">
                          {skill.skillName}
                        </h4>
                        <span className="text-xs text-orange-600 font-medium">
                          {skill.category}
                        </span>
                      </div>
                      <button
                        onClick={() =>
                          deleteSkillMutation.mutate(skill.userSkillId)
                        }
                        className="p-2 text-gray-300 hover:text-red-500 hover:bg-red-50 rounded-full transition-all opacity-0 group-hover/card:opacity-100"
                        type="button"
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </motion.div>

          {/* Reviews Section */}
          {userReputation?.hasMinimumReviews && (
            <motion.div
              variants={itemVariants}
              className="col-span-1 md:col-span-12 bg-emerald-900/30 rounded-3xl p-6 border border-emerald-800/50"
            >
              <h3 className="text-xl font-semibold mb-6 flex items-center gap-2 text-white">
                <Star className="w-6 h-6 text-yellow-400 fill-yellow-400" />
                Reviews ({userReputation.totalReviews})
              </h3>
              <ReviewList reviews={userReviews ?? []} isLoading={reviewsLoading} />
            </motion.div>
          )}

          {/* Badges Section */}
          <motion.div
            variants={itemVariants}
            className="col-span-1 md:col-span-12 bg-emerald-900/30 rounded-3xl p-6 border border-emerald-800/50"
          >
            <h3 className="text-xl font-semibold mb-6 flex items-center gap-2 text-white">
              <Award className="w-6 h-6 text-amber-400" />
              Badges &amp; Achievements
            </h3>
            <BadgeGrid badges={allBadges ?? []} isLoading={badgesLoading} />
          </motion.div>

        </motion.div>

        )}

        {(skillsError || userError) && (
          <div className="mt-6 p-4 bg-red-500/10 border border-red-500/20 rounded-xl text-red-200 text-center">
            Failed to load data. Please try again later.
          </div>
        )}

        {/* Settings Tab */}
        {activeTab === 'settings' && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="bg-white rounded-2xl p-8 shadow-xl"
          >
            <LinkedAccountsSettings />

            {/* ⭐ Identity Verification Section */}
            <div className="mt-8 pt-8 border-t border-gray-200">
              <h3 className="text-lg font-bold text-gray-900 mb-2">Identity Verification</h3>
              <p className="text-sm text-gray-600 mb-4">
                Get verified to increase trust on the platform and earn a special badge plus 25 credits.
              </p>
              <VerificationRequestForm />
            </div>

            {/* ⭐ Logout Section */}
            <div className="mt-8 pt-8 border-t border-gray-200">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Session Management</h3>
              <p className="text-sm text-gray-600 mb-4">
                Securely log out of your account. This will revoke your refresh token.
              </p>
              <button
                onClick={() => accountApi.logout()}
                className="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-xl font-semibold transition-colors shadow-md hover:shadow-lg"
              >
                Logout
              </button>
            </div>
          </motion.div>
        )}
      </div>

      <SkillModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={(data: AddSkillPayload) => createSkillMutation.mutate(data)}
        isSubmitting={createSkillMutation.isPending}
        defaultType={activeModalType}
      />

      <EditProfileModal
        isOpen={isEditProfileOpen}
        onClose={() => setIsEditProfileOpen(false)}
        user={user}
        onSubmit={(payload) => updateProfileMutation.mutate(payload)}
        isSubmitting={updateProfileMutation.isPending}
        errorMessage={updateProfileErrorMessage}
      />
    </div>
  );
}
