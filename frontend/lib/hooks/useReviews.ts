import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { reviewsApi } from "../api/reviews";
import type { CreateReviewDto } from "../types/reviews";

export const REVIEWS_QUERY_KEY = ["reviews"] as const;

/**
 * Get all reviews for a specific user
 */
export function useUserReviews(userId: string) {
  return useQuery({
    queryKey: [...REVIEWS_QUERY_KEY, "user", userId],
    queryFn: async () => {
      const response = await reviewsApi.getReviewsByUserId(userId);
      return response.data;
    },
    enabled: !!userId,
  });
}

/**
 * Get all reviews for a specific proposal
 */
export function useProposalReviews(proposalId: number) {
  return useQuery({
    queryKey: [...REVIEWS_QUERY_KEY, "proposal", proposalId],
    queryFn: async () => {
      const response = await reviewsApi.getReviewsForProposal(proposalId);
      return response.data;
    },
    enabled: proposalId > 0,
  });
}

/**
 * Check if the current user can review a specific proposal
 */
export function useCanReviewProposal(proposalId: number) {
  return useQuery({
    queryKey: [...REVIEWS_QUERY_KEY, "can-review", proposalId],
    queryFn: async () => {
      const response = await reviewsApi.canUserReviewProposal(proposalId);
      return response.data;
    },
    enabled: proposalId > 0,
  });
}

/**
 * Get reputation statistics for a specific user
 */
export function useUserReputation(userId: string) {
  return useQuery({
    queryKey: [...REVIEWS_QUERY_KEY, "reputation", userId],
    queryFn: async () => {
      const response = await reviewsApi.getUserReputation(userId);
      return response.data;
    },
    enabled: !!userId,
  });
}

/**
 * Mutation to create a new review
 */
export function useCreateReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateReviewDto) => reviewsApi.createReview(payload),
    onSuccess: (response, variables) => {
      // Invalidate relevant queries
      queryClient.invalidateQueries({
        queryKey: [...REVIEWS_QUERY_KEY, "proposal", variables.proposalId],
      });
      queryClient.invalidateQueries({
        queryKey: [...REVIEWS_QUERY_KEY, "can-review", variables.proposalId],
      });
      // Invalidate reputation queries (we don't know the reviewee ID here, so invalidate all)
      queryClient.invalidateQueries({
        queryKey: [...REVIEWS_QUERY_KEY, "reputation"],
      });
      queryClient.invalidateQueries({
        queryKey: [...REVIEWS_QUERY_KEY, "user"],
      });
    },
  });
}
