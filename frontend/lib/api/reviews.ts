import axiosInstance from "../axios";
import type {
  CreateReviewDto,
  ReviewDto,
  UserReputationDto,
  ServiceResponse,
} from "../types/reviews";

export const reviewsApi = {
  /**
   * Submit a review for a completed proposal
   */
  createReview: async (
    payload: CreateReviewDto
  ): Promise<ServiceResponse<ReviewDto>> => {
    const response = await axiosInstance.post<ServiceResponse<ReviewDto>>(
      "/reviews",
      payload
    );
    return response.data;
  },

  /**
   * Get all reviews received by a specific user
   */
  getReviewsByUserId: async (
    userId: string
  ): Promise<ServiceResponse<ReviewDto[]>> => {
    const response = await axiosInstance.get<ServiceResponse<ReviewDto[]>>(
      `/reviews/user/${userId}`
    );
    return response.data;
  },

  /**
   * Get all reviews for a specific proposal (both parties' reviews)
   */
  getReviewsForProposal: async (
    proposalId: number
  ): Promise<ServiceResponse<ReviewDto[]>> => {
    const response = await axiosInstance.get<ServiceResponse<ReviewDto[]>>(
      `/reviews/proposal/${proposalId}`
    );
    return response.data;
  },

  /**
   * Check if the current user can review a specific proposal
   */
  canUserReviewProposal: async (
    proposalId: number
  ): Promise<ServiceResponse<boolean>> => {
    const response = await axiosInstance.get<ServiceResponse<boolean>>(
      `/reviews/can-review/${proposalId}`
    );
    return response.data;
  },

  /**
   * Get reputation statistics for a specific user
   */
  getUserReputation: async (
    userId: string
  ): Promise<ServiceResponse<UserReputationDto>> => {
    const response = await axiosInstance.get<ServiceResponse<UserReputationDto>>(
      `/reviews/reputation/${userId}`
    );
    return response.data;
  },
};
