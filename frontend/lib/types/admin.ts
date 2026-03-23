// Admin Types

export interface AdminUserDto {
  id: string;
  userName: string;
  email: string;
  profilePictureUrl: string | null;
  createdAt: string;
  isVerified: boolean;
  isSuspended: boolean;
  isBanned: boolean;
  creditBalance: number;
  skillCount: number;
  completedSwaps: number;
  location: string | null;
}

export interface AdminUserListDto {
  users: AdminUserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UpdateUserStatusDto {
  action: 'Suspend' | 'Unsuspend' | 'Ban';
  reason?: string;
}

export interface UserGrowthPoint {
  month: string;
  count: number;
}

export interface TopSkillPoint {
  skillName: string;
  userCount: number;
}

export interface ProposalStatsDto {
  pending: number;
  accepted: number;
  declined: number;
  completed: number;
}

export interface AdminDashboardDto {
  totalUsers: number;
  activeUsersLast30Days: number;
  totalSwaps: number;
  totalCreditsCirculated: number;
  pendingVerifications: number;
  totalReviews: number;
  userGrowth: UserGrowthPoint[];
  topSkills: TopSkillPoint[];
  proposalStats: ProposalStatsDto;
}

// ───── Content Moderation ─────

export interface ReportContentDto {
  contentType: 'Skill' | 'Review';
  contentId: number;
  reason: string;
}

export interface RemoveContentDto {
  reason: string;
}

export interface FlaggedContentDto {
  reportId: number;
  contentType: string;
  contentId: number;
  reporterName: string;
  reason: string;
  createdAt: string;
  contentOwnerName: string | null;
  contentPreview: string | null;
  rating: number | null;
}

export interface FlaggedContentListDto {
  reports: FlaggedContentDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AdminSkillDto {
  userSkillId: number;
  skillName: string;
  category: string | null;
  type: number;
  description: string | null;
  userName: string;
  userId: string;
  createdAt: string;
}

export interface AdminSkillListDto {
  skills: AdminSkillDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AdminReviewDto {
  reviewId: number;
  reviewerName: string;
  revieweeName: string;
  rating: number;
  comment: string | null;
  proposalId: number;
  createdAt: string;
}

export interface AdminReviewListDto {
  reviews: AdminReviewDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ───── Dispute Resolution ─────

export interface AdminProposalDto {
  proposalId: number;
  proposerName: string;
  proposerId: string;
  recipientName: string;
  recipientId: string;
  proposerSkill: string;
  recipientSkill: string;
  status: string;
  proposerConfirmed: boolean;
  recipientConfirmed: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AdminProposalListDto {
  proposals: AdminProposalDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ResolveDisputeDto {
  action: 'ForceComplete' | 'ForceCancel';
  adminNote: string;
}
