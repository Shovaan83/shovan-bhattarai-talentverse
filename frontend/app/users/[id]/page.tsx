'use client';

import { useState, use, useMemo } from 'react';
import { motion } from 'framer-motion';
import toast from 'react-hot-toast';
import { 
  ArrowLeft, 
  Star, 
  ArrowRightLeft, 
  Calendar, 
  User,
  MessageSquare,
  Send,
  X,
  AlertCircle,
  Clock,
  CheckCircle
} from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useUserProfile } from '@/lib/hooks/useMarketplace';
import { useCreateProposal, useProposals } from '@/lib/hooks/useProposals';
import { useSkills } from '@/lib/hooks/useSkills';
import { useUserReviews, useUserReputation } from '@/lib/hooks/useReviews';
import ReputationBadge from '@/app/components/reviews/ReputationBadge';
import ReviewList from '@/app/components/reviews/ReviewList';
import VerifiedBadge from '@/app/components/VerifiedBadge';

interface CreateProposalModalProps {
  isOpen: boolean;
  onClose: () => void;
  targetUser: {
    id: string;
    displayName: string;
    offeredSkills: Array<{ id: number; skillName: string }>;
    wantedSkills: Array<{ id: number; skillName: string }>;
  };
  currentUserSkills: Array<{ id: number; skillName: string; skillType: string }>;
}

function CreateProposalModal({ isOpen, onClose, targetUser, currentUserSkills }: CreateProposalModalProps) {
  const [selectedMySkill, setSelectedMySkill] = useState<number | null>(null);
  const [selectedTheirSkill, setSelectedTheirSkill] = useState<number | null>(null);
  const [creditAmount, setCreditAmount] = useState('10');
  const [message, setMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const createProposal = useCreateProposal();

  const myOfferedSkills = currentUserSkills.filter(s => s.skillType === 'Offered');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedMySkill || !selectedTheirSkill) return;

    setErrorMessage(null); // Clear previous errors

    const parsedCreditAmount = Number(creditAmount);
    if (!Number.isFinite(parsedCreditAmount) || parsedCreditAmount <= 0) {
      setErrorMessage('Credit amount must be greater than 0.');
      return;
    }

    try {
      await createProposal.mutateAsync({
        proposerUserSkillId: selectedMySkill,
        recipientUserSkillId: selectedTheirSkill,
        creditAmount: parsedCreditAmount,
        message: message || undefined,
      });
      toast.success('Proposal sent successfully! Check your proposals page to track the status.');
      onClose();
    } catch (error: any) {
      console.error('Failed to create proposal:', error);
      // Extract error message from backend response
      const backendMessage = error.response?.data?.message || 
                           error.response?.data?.errors?.[0] ||
                           'Failed to create proposal. Please try again.';
      setErrorMessage(backendMessage);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        className="relative bg-white rounded-3xl p-6 w-full max-w-lg border border-zinc-200 shadow-xl"
      >
        <button
          onClick={onClose}
          className="absolute top-4 right-4 p-2 rounded-xl hover:bg-zinc-100 transition-colors text-zinc-500"
        >
          <X className="w-5 h-5" />
        </button>

        <h2 className="text-xl font-bold mb-6 text-zinc-900">Propose a Skill Swap</h2>
        <p className="text-zinc-500 mb-6">
          Create a swap proposal with <span className="text-zinc-900 font-medium">{targetUser.displayName}</span>
        </p>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Error Message Display */}
          {errorMessage && (
            <div className="p-4 rounded-xl bg-red-50 border border-red-200 text-red-700 text-sm flex items-start gap-3">
              <AlertCircle className="w-5 h-5 flex-shrink-0 mt-0.5 text-red-500" />
              <p>{errorMessage}</p>
            </div>
          )}

          {/* My Skill Selection */}
          <div>
            <label className="block text-sm font-medium text-[#1D9E75] mb-2">
              I will teach (my skill)
            </label>
            <div className="grid grid-cols-2 gap-2">
              {myOfferedSkills.map((skill) => (
                <button
                  key={skill.id}
                  type="button"
                  onClick={() => setSelectedMySkill(skill.id)}
                  className={`p-3 rounded-xl text-sm text-left transition-colors ${
                    selectedMySkill === skill.id
                      ? 'bg-[#1D9E75] text-white border-[#1D9E75]'
                      : 'bg-[#1D9E75]/10 text-[#1D9E75] border-[#1D9E75]/20 hover:bg-[#1D9E75]/20'
                  } border`}
                >
                  {skill.skillName}
                </button>
              ))}
            </div>
            {myOfferedSkills.length === 0 && (
              <p className="text-amber-600 text-sm">
                You need to add offered skills to your profile first.
              </p>
            )}
          </div>

          {/* Their Skill Selection */}
          <div>
            <label className="block text-sm font-medium text-[#3C2A8A] mb-2">
              I want to learn (their skill)
            </label>
            <div className="grid grid-cols-2 gap-2">
              {targetUser.offeredSkills.map((skill) => (
                <button
                  key={skill.id}
                  type="button"
                  onClick={() => setSelectedTheirSkill(skill.id)}
                  className={`p-3 rounded-xl text-sm text-left transition-colors ${
                    selectedTheirSkill === skill.id
                      ? 'bg-[#3C2A8A] text-white border-[#3C2A8A]'
                      : 'bg-[#3C2A8A]/10 text-[#3C2A8A] border-[#3C2A8A]/20 hover:bg-[#3C2A8A]/20'
                  } border`}
                >
                  {skill.skillName}
                </button>
              ))}
            </div>
          </div>

          {/* Message */}
          <div>
            <label className="block text-sm font-medium text-zinc-700 mb-2">
              Proposed credits
            </label>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={creditAmount}
              onChange={(e) => setCreditAmount(e.target.value)}
              className="w-full px-4 py-3 rounded-xl bg-white border border-zinc-200 text-zinc-900 placeholder-zinc-400 focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75]"
            />
            <p className="mt-2 text-xs text-zinc-500">
              Set the credit amount you want attached to this swap proposal.
            </p>
          </div>

          {/* Message */}
          <div>
            <label className="block text-sm font-medium text-zinc-700 mb-2">
              Message (optional)
            </label>
            <textarea
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Introduce yourself and explain what you'd like to learn..."
              rows={3}
              className="w-full px-4 py-3 rounded-xl bg-white border border-zinc-200 text-zinc-900 placeholder-zinc-400 focus:outline-none focus:border-[#1D9E75] focus:ring-1 focus:ring-[#1D9E75] resize-none"
            />
          </div>

          {/* Submit */}
          <button
            type="submit"
            disabled={!selectedMySkill || !selectedTheirSkill || createProposal.isPending}
            className="w-full py-3 rounded-xl bg-[#1D9E75] hover:bg-[#0F6E56] text-white transition-all font-medium disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            {createProposal.isPending ? (
              'Sending...'
            ) : (
              <>
                <Send className="w-4 h-4" />
                Send Proposal
              </>
            )}
          </button>
        </form>
      </motion.div>
    </div>
  );
}

export default function UserProfilePage({ params }: { params: Promise<{ id: string }> }) {
  const resolvedParams = use(params);
  const router = useRouter();
  const { data: user, isLoading, error } = useUserProfile(resolvedParams.id);
  const { data: proposalsData } = useProposals();
  const { data: mySkills, isLoading: isLoadingMySkills } = useSkills();
  const { data: userReviews, isLoading: isLoadingReviews } = useUserReviews(resolvedParams.id);
  const { data: userReputation } = useUserReputation(resolvedParams.id);
  const [showProposalModal, setShowProposalModal] = useState(false);

  // Map current user skills to the format expected by the modal
  const currentUserSkills = (mySkills || []).map(skill => ({
    id: skill.userSkillId,
    skillName: skill.skillName,
    skillType: skill.type === 'Offer' ? 'Offered' : 'Wanted',
  }));

  // Check if there's an active proposal with this user
  const activeProposal = useMemo(() => {
    if (!proposalsData?.proposals || !user) return null;
    return proposalsData.proposals.find(
      (p) => p.otherUserId === resolvedParams.id && p.status === 'Pending'
    );
  }, [proposalsData, user, resolvedParams.id]);

  if (isLoading || isLoadingMySkills) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-[#1D9E75]"></div>
      </div>
    );
  }

  if (error || !user) {
    return (
      <div className="min-h-screen bg-[#FAFAFA] flex items-center justify-center text-zinc-900">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-2">User not found</h2>
          <p className="text-zinc-500 mb-4">The user you're looking for doesn't exist.</p>
          <Link
            href="/marketplace"
            className="text-[#1D9E75] hover:text-[#0F6E56] transition-colors"
          >
            ← Back to Marketplace
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FAFAFA] text-zinc-900">
      {/* Header */}
      <div className="border-b border-zinc-200 bg-white/80 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-4xl mx-auto px-6 py-4">
          <div className="flex items-center gap-4">
            <button
              onClick={() => router.back()}
              className="p-2 rounded-xl bg-zinc-100 hover:bg-zinc-200 transition-colors text-zinc-600"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-xl font-bold text-zinc-900">{user.displayName}</h1>
              <p className="text-zinc-500 text-sm">@{user.userName}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-6 py-8">
        {/* Profile Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-3xl p-8 border border-zinc-200 shadow-sm mb-8"
        >
          <div className="flex flex-col md:flex-row gap-6">
            {/* Avatar */}
            <div className="w-24 h-24 rounded-3xl bg-gradient-to-br from-[#1D9E75] to-[#0F6E56] flex items-center justify-center flex-shrink-0">
              {user.profilePictureUrl ? (
                <img
                  src={user.profilePictureUrl}
                  alt={user.displayName}
                  className="w-full h-full rounded-3xl object-cover"
                />
              ) : (
                <User className="w-12 h-12 text-white" />
              )}
            </div>

            {/* Info */}
            <div className="flex-1">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <div className="flex items-center gap-2">
                    <h2 className="text-2xl font-bold text-zinc-900">{user.displayName}</h2>
                    {user.isVerified && <VerifiedBadge size="md" />}
                  </div>
                  <p className="text-zinc-500">@{user.userName}</p>
                  {/* Display reputation badge */}
                  {userReputation && (
                    <div className="mt-2">
                      <ReputationBadge
                        averageRating={userReputation.averageRating}
                        totalReviews={userReputation.totalReviews}
                        hasMinimumReviews={userReputation.hasMinimumReviews}
                        size="md"
                        showCount={true}
                      />
                    </div>
                  )}
                </div>
              </div>

              {user.bio && (
                <p className="text-zinc-600 mb-4">{user.bio}</p>
              )}

              <div className="flex flex-wrap gap-4 text-sm">
                <div className="flex items-center gap-2 text-zinc-500">
                  <ArrowRightLeft className="w-4 h-4 text-[#1D9E75]" />
                  <span>{user.completedSwaps} swaps completed</span>
                </div>
                <div className="flex items-center gap-2 text-zinc-500">
                  <Calendar className="w-4 h-4 text-[#1D9E75]" />
                  <span>Joined {new Date(user.joinedAt).toLocaleDateString()}</span>
                </div>
              </div>
            </div>
          </div>

          {/* CTA Button */}
          <div className="mt-6 pt-6 border-t border-zinc-200">
            {activeProposal ? (
              <div className="flex items-center gap-3 px-6 py-4 rounded-xl bg-blue-50 border border-blue-200">
                <Clock className="w-5 h-5 text-blue-500 flex-shrink-0" />
                <div>
                  <p className="text-blue-700 font-medium">Proposal Pending</p>
                  <p className="text-blue-600 text-sm">
                    You have an active proposal with this user. Check your proposals page for updates.
                  </p>
                </div>
              </div>
            ) : (
              <button
                onClick={() => setShowProposalModal(true)}
                className="w-full md:w-auto px-8 py-3 rounded-xl bg-[#1D9E75] hover:bg-[#0F6E56] text-white transition-all font-medium flex items-center justify-center gap-2"
              >
                <MessageSquare className="w-5 h-5" />
                Propose a Skill Swap
              </button>
            )}
          </div>
        </motion.div>

        {/* Skills Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Offered Skills */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white rounded-3xl p-6 border border-zinc-200 shadow-sm"
          >
            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2 text-zinc-900">
              <span className="w-2 h-2 rounded-full bg-[#1D9E75]"></span>
              Skills Offered
            </h3>
            {user.offeredSkills.length > 0 ? (
              <div className="space-y-3">
                {user.offeredSkills.map((skill) => (
                  <div
                    key={skill.id}
                    className="p-4 rounded-2xl bg-[#1D9E75]/10 border border-[#1D9E75]/20"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium text-zinc-900">{skill.skillName}</span>
                      <div className="flex gap-0.5">
                        {[...Array(5)].map((_, i) => (
                          <div
                            key={i}
                            className={`w-2 h-2 rounded-full ${
                              i < skill.proficiencyLevel
                                ? 'bg-[#1D9E75]'
                                : 'bg-zinc-200'
                            }`}
                          />
                        ))}
                      </div>
                    </div>
                    {skill.description && (
                      <p className="text-[#1D9E75] text-sm">{skill.description}</p>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-zinc-500 text-sm">No skills offered yet</p>
            )}
          </motion.div>

          {/* Wanted Skills */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="bg-white rounded-3xl p-6 border border-zinc-200 shadow-sm"
          >
            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2 text-zinc-900">
              <span className="w-2 h-2 rounded-full bg-[#3C2A8A]"></span>
              Skills Wanted
            </h3>
            {user.wantedSkills.length > 0 ? (
              <div className="space-y-3">
                {user.wantedSkills.map((skill) => (
                  <div
                    key={skill.id}
                    className="p-4 rounded-2xl bg-[#3C2A8A]/10 border border-[#3C2A8A]/20"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium text-zinc-900">{skill.skillName}</span>
                      <div className="flex gap-0.5">
                        {[...Array(5)].map((_, i) => (
                          <div
                            key={i}
                            className={`w-2 h-2 rounded-full ${
                              i < skill.proficiencyLevel
                                ? 'bg-[#3C2A8A]'
                                : 'bg-zinc-200'
                            }`}
                          />
                        ))}
                      </div>
                    </div>
                    {skill.description && (
                      <p className="text-[#3C2A8A] text-sm">{skill.description}</p>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-zinc-500 text-sm">No skills wanted yet</p>
            )}
          </motion.div>
        </div>

        {/* Reviews Section */}
        {userReputation && userReputation.hasMinimumReviews && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="mt-8 bg-white rounded-3xl p-6 border border-zinc-200 shadow-sm"
          >
            <h3 className="text-xl font-semibold mb-6 flex items-center gap-2 text-zinc-900">
              <Star className="w-6 h-6 text-yellow-400 fill-yellow-400" />
              Reviews ({userReputation.totalReviews})
            </h3>
            <ReviewList reviews={userReviews || []} isLoading={isLoadingReviews} />
          </motion.div>
        )}
      </div>

      {/* Proposal Modal */}
      <CreateProposalModal
        isOpen={showProposalModal}
        onClose={() => setShowProposalModal(false)}
        targetUser={{
          id: user.id,
          displayName: user.displayName,
          offeredSkills: user.offeredSkills,
          wantedSkills: user.wantedSkills,
        }}
        currentUserSkills={currentUserSkills}
      />
    </div>
  );
}
