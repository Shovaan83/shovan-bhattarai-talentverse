import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { verificationApi } from '../api/verification';
import type {
  SubmitVerificationRequestDto,
  ReviewVerificationDto,
} from '../types/verification';
import { toast } from 'sonner';

export const useVerificationStatus = () => {
  return useQuery({
    queryKey: ['verification', 'status'],
    queryFn: verificationApi.getMyStatus,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};

export const useUploadDocument = () => {
  return useMutation({
    mutationFn: (file: File) => verificationApi.uploadDocument(file),
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to upload document');
    },
  });
};

export const useSubmitVerificationRequest = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: SubmitVerificationRequestDto) => verificationApi.submitRequest(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['verification', 'status'] });
      queryClient.invalidateQueries({ queryKey: ['currentUser'] });
      toast.success('Verification request submitted successfully');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to submit verification request');
    },
  });
};

// Admin hooks
export const usePendingVerifications = (page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: ['admin', 'verifications', 'pending', page, pageSize],
    queryFn: () => verificationApi.getPendingRequests(page, pageSize),
    staleTime: 1000 * 60, // 1 minute
  });
};

export const useVerificationRequestById = (id: number) => {
  return useQuery({
    queryKey: ['admin', 'verifications', id],
    queryFn: () => verificationApi.getRequestById(id),
    enabled: !!id,
  });
};

export const useReviewVerification = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: ReviewVerificationDto }) =>
      verificationApi.reviewRequest(id, dto),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'verifications'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'verifications', variables.id] });
      toast.success(variables.dto.isApproved ? 'Request approved' : 'Request rejected');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to review verification request');
    },
  });
};
