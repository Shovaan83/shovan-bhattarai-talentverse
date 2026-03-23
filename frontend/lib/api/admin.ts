import api from '../axios';
import type {
  AdminUserListDto,
  AdminDashboardDto,
  UpdateUserStatusDto,
  FlaggedContentListDto,
  AdminSkillListDto,
  AdminReviewListDto,
  RemoveContentDto,
  ReportContentDto,
  AdminProposalListDto,
  ResolveDisputeDto,
} from '../types/admin';

export const adminApi = {
  searchUsers: async (
    query?: string,
    page = 1,
    pageSize = 20
  ): Promise<AdminUserListDto> => {
    const response = await api.get('/admin/users', {
      params: { query, page, pageSize },
    });
    return response.data.data;
  },

  updateUserStatus: async (
    userId: string,
    dto: UpdateUserStatusDto
  ): Promise<boolean> => {
    const response = await api.put(`/admin/users/${userId}/status`, dto);
    return response.data.data;
  },

  getDashboard: async (): Promise<AdminDashboardDto> => {
    const response = await api.get('/admin/dashboard');
    return response.data.data;
  },

  // ───── Content Moderation ─────

  getFlaggedContent: async (page = 1, pageSize = 20): Promise<FlaggedContentListDto> => {
    const response = await api.get('/admin/moderation/reports', { params: { page, pageSize } });
    return response.data.data;
  },

  searchSkills: async (query?: string, page = 1, pageSize = 20): Promise<AdminSkillListDto> => {
    const response = await api.get('/admin/moderation/skills', { params: { query, page, pageSize } });
    return response.data.data;
  },

  searchReviews: async (query?: string, page = 1, pageSize = 20): Promise<AdminReviewListDto> => {
    const response = await api.get('/admin/moderation/reviews', { params: { query, page, pageSize } });
    return response.data.data;
  },

  removeSkill: async (userSkillId: number, dto: RemoveContentDto): Promise<boolean> => {
    const response = await api.delete(`/admin/moderation/skills/${userSkillId}`, { data: dto });
    return response.data.data;
  },

  removeReview: async (reviewId: number, dto: RemoveContentDto): Promise<boolean> => {
    const response = await api.delete(`/admin/moderation/reviews/${reviewId}`, { data: dto });
    return response.data.data;
  },

  dismissReport: async (reportId: number): Promise<boolean> => {
    const response = await api.post(`/admin/moderation/reports/${reportId}/dismiss`);
    return response.data.data;
  },

  reportContent: async (dto: ReportContentDto): Promise<boolean> => {
    const response = await api.post('/reports', dto);
    return response.data.data;
  },

  // ───── Dispute Resolution ─────

  searchProposals: async (query?: string, status?: number, page = 1, pageSize = 20): Promise<AdminProposalListDto> => {
    const response = await api.get('/admin/disputes', { params: { query, status, page, pageSize } });
    return response.data.data;
  },

  resolveDispute: async (proposalId: number, dto: ResolveDisputeDto): Promise<boolean> => {
    const response = await api.put(`/admin/disputes/${proposalId}/resolve`, dto);
    return response.data.data;
  },
};
