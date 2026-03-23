import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminApi } from '../api/admin';
import type { UpdateUserStatusDto, RemoveContentDto, ResolveDisputeDto } from '../types/admin';

export function useAdminUsers(query?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'users', query, page, pageSize],
    queryFn: () => adminApi.searchUsers(query, page, pageSize),
  });
}

export function useAdminDashboard() {
  return useQuery({
    queryKey: ['admin', 'dashboard'],
    queryFn: () => adminApi.getDashboard(),
    staleTime: 60_000, // cache for 1 minute
  });
}

export function useUpdateUserStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      userId,
      dto,
    }: {
      userId: string;
      dto: UpdateUserStatusDto;
    }) => adminApi.updateUserStatus(userId, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'users'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] });
    },
  });
}

// ───── Content Moderation ─────

export function useFlaggedContent(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'moderation', 'reports', page, pageSize],
    queryFn: () => adminApi.getFlaggedContent(page, pageSize),
  });
}

export function useAdminSkills(query?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'moderation', 'skills', query, page, pageSize],
    queryFn: () => adminApi.searchSkills(query, page, pageSize),
  });
}

export function useAdminReviews(query?: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'moderation', 'reviews', query, page, pageSize],
    queryFn: () => adminApi.searchReviews(query, page, pageSize),
  });
}

export function useRemoveSkill() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userSkillId, dto }: { userSkillId: number; dto: RemoveContentDto }) =>
      adminApi.removeSkill(userSkillId, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'moderation'] });
    },
  });
}

export function useRemoveReview() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ reviewId, dto }: { reviewId: number; dto: RemoveContentDto }) =>
      adminApi.removeReview(reviewId, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'moderation'] });
    },
  });
}

export function useDismissReport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (reportId: number) => adminApi.dismissReport(reportId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'moderation'] });
    },
  });
}

// ───── Dispute Resolution ─────

export function useAdminProposals(query?: string, status?: number, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'disputes', query, status, page, pageSize],
    queryFn: () => adminApi.searchProposals(query, status, page, pageSize),
  });
}

export function useResolveDispute() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ proposalId, dto }: { proposalId: number; dto: ResolveDisputeDto }) =>
      adminApi.resolveDispute(proposalId, dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'disputes'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'dashboard'] });
    },
  });
}
