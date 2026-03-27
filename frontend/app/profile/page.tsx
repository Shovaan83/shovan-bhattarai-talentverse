"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { motion, type Variants } from "framer-motion";
import axios from "axios";
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
  Coins,
  ArrowRightLeft,
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
      <div className="flex items-center justify-center min-h-screen bg-[#FAFAFA] text-zinc-900">
        <div className="w-12 h-12 border-4 border-[#1D9E75] rounded-full animate-spin border-t-transparent" />
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen p-4 md:p-8 bg-[#FAFAFA] text-zinc-900">
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
    <div className="relative min-h-screen p-4 md:p-8 bg-[#FAFAFA] text-zinc-900 overflow-hidden">
      <div className="max-w-7xl mx-auto relative z-10">
        {/* Page Header */}
        <header className="mb-6">
          <h1 className="text-2xl font-display font-bold text-zinc-900">
            My Profile
          </h1>
          <p className="text-zinc-600 font-body text-sm">
            Manage your identity and skill portfolio
          </p>
        </header>

        {/* Tabs */}
        <div className="flex gap-1 mb-6 border-b border-zinc-200">
          <button
            onClick={() => setActiveTab('overview')}
            className={`px-4 py-2.5 font-medium text-sm transition-all ${
              activeTab === 'overview'
                ? 'text-zinc-900 border-b-2 border-[#1D9E75]'
                : 'text-zinc-500 hover:text-zinc-900'
            }`}
          >
            Overview
          </button>
          <button
            onClick={() => setActiveTab('settings')}
            className={`px-4 py-2.5 font-medium text-sm transition-all flex items-center gap-2 ${
              activeTab === 'settings'
                ? 'text-zinc-900 border-b-2 border-[#1D9E75]'
                : 'text-zinc-500 hover:text-zinc-900'
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
            className="space-y-6"
          >
            {/* Profile Header Strip */}
            <motion.div
              variants={itemVariants}
              className="bg-white border border-zinc-200 rounded-xl overflow-hidden"
            >
              {/* Cover gradient */}
              <div className="h-20 bg-gradient-to-br from-zinc-100 to-zinc-200" />
              
              <div className="px-6 pb-6">
                {/* Avatar + Info Row */}
                <div className="flex flex-col md:flex-row md:items-end gap-4 -mt-10">
                  {/* Avatar */}
                  <div className="shrink-0">
                    {avatarUrl ? (
                      <img
                        src={avatarUrl}
                        alt={displayName}
                        className="w-20 h-20 rounded-xl border-4 border-white shadow-sm object-cover bg-white"
                      />
                    ) : (
                      <div className="w-20 h-20 rounded-xl border-4 border-white shadow-sm bg-zinc-100 flex items-center justify-center">
                        <span className="font-heading font-bold text-xl text-zinc-400">
                          {displayName.slice(0, 1).toUpperCase()}
                        </span>
                      </div>
                    )}
                  </div>
                  
                  {/* User Info */}
                  <div className="flex-1 pt-2 md:pt-0">
                    <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
                      <div>
                        <h2 className="text-xl font-heading font-bold text-zinc-900 flex items-center gap-2">
                          {displayName}
                          {userReputation && (
                            <ReputationBadge
                              averageRating={userReputation.averageRating}
                              totalReviews={userReputation.totalReviews}
                              hasMinimumReviews={userReputation.hasMinimumReviews}
                              size="sm"
                              showCount={true}
                            />
                          )}
                        </h2>
                        <p className="text-sm text-zinc-500">{handle}</p>
                      </div>
                      
                      <button
                        className="px-4 py-2 flex items-center justify-center gap-2 rounded-lg bg-zinc-900 hover:bg-zinc-800 text-sm font-medium text-white transition-colors"
                        type="button"
                        onClick={() => setIsEditProfileOpen(true)}
                      >
                        <Edit2 size={14} />
                        Edit Profile
                      </button>
                    </div>
                    
                    {bio && (
                      <p className="text-zinc-600 text-sm mt-2 max-w-2xl">{bio}</p>
                    )}
                    
                    {/* Meta info */}
                    <div className="flex flex-wrap gap-4 mt-3 text-sm text-zinc-500">
                      <div className="flex items-center gap-1.5">
                        <MapPin size={14} />
                        <span>Itahari, Sunsari</span>
                      </div>
                      <a href="#" className="flex items-center gap-1.5 hover:text-[#1D9E75] transition-colors">
                        <LinkIcon size={14} />
                        <span>shovan.com.np</span>
                      </a>
                      <button className="p-1.5 rounded-md hover:bg-zinc-100 text-zinc-500 hover:text-zinc-700 transition-colors" type="button">
                        <Twitter size={14} />
                      </button>
                      <button className="p-1.5 rounded-md hover:bg-zinc-100 text-zinc-500 hover:text-zinc-700 transition-colors" type="button">
                        <Github size={14} />
                      </button>
                    </div>
                  </div>
                </div>
                
                {/* Stats Strip with dividers */}
                <div className="mt-6 pt-4 border-t border-zinc-100 flex flex-wrap items-center gap-6">
                  <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-md bg-amber-100">
                      <Coins size={14} className="text-amber-600" />
                    </div>
                    <div>
                      <span className="font-semibold text-zinc-900">{credits}</span>
                      <span className="text-xs text-zinc-500 ml-1">Credits</span>
                    </div>
                  </div>
                  
                  <div className="w-px h-6 bg-zinc-200" />
                  
                  <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-md bg-emerald-100">
                      <Star size={14} className="text-emerald-600" />
                    </div>
                    <div>
                      <span className="font-semibold text-zinc-900">
                        {hasMinimumReviews ? `${averageRating.toFixed(1)}` : "New"}
                      </span>
                      {hasMinimumReviews && (
                        <span className="text-xs text-zinc-500 ml-1">({totalReviews} reviews)</span>
                      )}
                    </div>
                  </div>
                  
                  <div className="w-px h-6 bg-zinc-200" />
                  
                  <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-md bg-violet-100">
                      <ArrowRightLeft size={14} className="text-violet-600" />
                    </div>
                    <div>
                      <span className="font-semibold text-zinc-900">{totalSwaps}</span>
                      <span className="text-xs text-zinc-500 ml-1">Swaps</span>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>

            {/* Skills Sections - Side by Side */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Offers Section */}
              <motion.div variants={itemVariants}>
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <div className="p-1.5 bg-emerald-100 text-[#1D9E75] rounded-md">
                      <Zap size={16} fill="currentColor" />
                    </div>
                    <h3 className="font-semibold text-zinc-900">Skills I Offer</h3>
                    <span className="text-xs text-zinc-400">({offers.length})</span>
                  </div>
                  <button
                    onClick={() => handleOpenModal(SkillType.Offer)}
                    className="p-1.5 rounded-md bg-zinc-100 text-zinc-600 hover:bg-zinc-200 transition-colors"
                    type="button"
                  >
                    <Plus size={16} />
                  </button>
                </div>
                
                {offers.length === 0 ? (
                  <div className="bg-white border border-zinc-200 rounded-xl p-8 flex flex-col items-center justify-center text-center">
                    <div className="p-3 rounded-full bg-zinc-100 mb-3">
                      <Zap size={24} className="text-zinc-400" />
                    </div>
                    <p className="text-sm text-zinc-500">No skills offered yet</p>
                    <button
                      onClick={() => handleOpenModal(SkillType.Offer)}
                      className="mt-3 text-sm text-[#1D9E75] hover:underline font-medium"
                    >
                      Add your first skill
                    </button>
                  </div>
                ) : (
                  <div className="bg-white border border-zinc-200 rounded-xl divide-y divide-zinc-100">
                    {offers.map((skill) => (
                      <div
                        key={skill.userSkillId}
                        className="px-4 py-3 flex items-center justify-between hover:bg-zinc-50 transition-colors group"
                      >
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <h4 className="font-medium text-zinc-900 truncate">
                              {skill.skillName}
                            </h4>
                            <span className="text-xs font-medium text-[#1D9E75] bg-emerald-50 px-2 py-0.5 rounded">
                              {skill.category}
                            </span>
                          </div>
                          {skill.description && (
                            <p className="text-sm text-zinc-500 truncate mt-0.5">
                              {skill.description}
                            </p>
                          )}
                        </div>
                        <button
                          onClick={() => deleteSkillMutation.mutate(skill.userSkillId)}
                          className="p-1.5 text-zinc-300 hover:text-red-500 hover:bg-red-50 rounded transition-all opacity-0 group-hover:opacity-100"
                          type="button"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </motion.div>

              {/* Wants Section */}
              <motion.div variants={itemVariants}>
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-2">
                    <div className="p-1.5 bg-violet-100 text-[#3C2A8A] rounded-md">
                      <Target size={16} />
                    </div>
                    <h3 className="font-semibold text-zinc-900">Skills I Want</h3>
                    <span className="text-xs text-zinc-400">({wants.length})</span>
                  </div>
                  <button
                    onClick={() => handleOpenModal(SkillType.Want)}
                    className="p-1.5 rounded-md bg-zinc-100 text-zinc-600 hover:bg-zinc-200 transition-colors"
                    type="button"
                  >
                    <Plus size={16} />
                  </button>
                </div>
                
                {wants.length === 0 ? (
                  <div className="bg-white border border-zinc-200 rounded-xl p-8 flex flex-col items-center justify-center text-center">
                    <div className="p-3 rounded-full bg-zinc-100 mb-3">
                      <Target size={24} className="text-zinc-400" />
                    </div>
                    <p className="text-sm text-zinc-500">No skills requested yet</p>
                    <button
                      onClick={() => handleOpenModal(SkillType.Want)}
                      className="mt-3 text-sm text-[#3C2A8A] hover:underline font-medium"
                    >
                      Add a skill you want
                    </button>
                  </div>
                ) : (
                  <div className="bg-white border border-zinc-200 rounded-xl divide-y divide-zinc-100">
                    {wants.map((skill) => (
                      <div
                        key={skill.userSkillId}
                        className="px-4 py-3 flex items-center justify-between hover:bg-zinc-50 transition-colors group"
                      >
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <h4 className="font-medium text-zinc-900 truncate">
                              {skill.skillName}
                            </h4>
                            <span className="text-xs font-medium text-[#3C2A8A] bg-violet-50 px-2 py-0.5 rounded">
                              {skill.category}
                            </span>
                          </div>
                        </div>
                        <button
                          onClick={() => deleteSkillMutation.mutate(skill.userSkillId)}
                          className="p-1.5 text-zinc-300 hover:text-red-500 hover:bg-red-50 rounded transition-all opacity-0 group-hover:opacity-100"
                          type="button"
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </motion.div>
            </div>

            {/* Reviews Section */}
            {userReputation?.hasMinimumReviews && (
              <motion.div variants={itemVariants}>
                <div className="flex items-center gap-2 mb-3">
                  <Star className="w-5 h-5 text-amber-500 fill-amber-500" />
                  <h3 className="font-semibold text-zinc-900">Reviews</h3>
                  <span className="text-xs text-zinc-400">({userReputation.totalReviews})</span>
                </div>
                <div className="bg-white border border-zinc-200 rounded-xl p-5">
                  <ReviewList reviews={userReviews ?? []} isLoading={reviewsLoading} />
                </div>
              </motion.div>
            )}

            {/* Badges Section */}
            <motion.div variants={itemVariants}>
              <div className="flex items-center gap-2 mb-3">
                <Award className="w-5 h-5 text-amber-500" />
                <h3 className="font-semibold text-zinc-900">Badges & Achievements</h3>
              </div>
              <div className="bg-white border border-zinc-200 rounded-xl p-5">
                <BadgeGrid badges={allBadges ?? []} isLoading={badgesLoading} />
              </div>
            </motion.div>
          </motion.div>
        )}

        {(skillsError || userError) && (
          <div className="mt-6 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-center">
            Failed to load data. Please try again later.
          </div>
        )}

        {/* Settings Tab */}
        {activeTab === 'settings' && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="bg-white border border-zinc-200 rounded-xl p-6"
          >
            <LinkedAccountsSettings />

            {/* Identity Verification Section */}
            <div className="mt-8 pt-8 border-t border-zinc-200">
              <h3 className="text-lg font-semibold text-zinc-900 mb-2">Identity Verification</h3>
              <p className="text-sm text-zinc-600 mb-4">
                Get verified to increase trust on the platform and earn a special badge plus 25 credits.
              </p>
              <VerificationRequestForm />
            </div>

            {/* Logout Section */}
            <div className="mt-8 pt-8 border-t border-zinc-200">
              <h3 className="text-lg font-semibold text-zinc-900 mb-2">Session Management</h3>
              <p className="text-sm text-zinc-600 mb-4">
                Securely log out of your account. This will revoke your refresh token.
              </p>
              <button
                onClick={() => accountApi.logout()}
                className="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium text-sm transition-colors"
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
