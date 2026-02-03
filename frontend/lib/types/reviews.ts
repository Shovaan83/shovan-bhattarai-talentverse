export interface CreateReviewDto {
  proposalId: number;
  rating: number;
  comment?: string;
}

export interface ReviewDto {
  reviewId: number;
  proposalId: number;
  reviewerId: string;
  reviewerUsername: string;
  reviewerProfilePictureUrl: string;
  revieweeId: string;
  revieweeUsername: string;
  rating: number;
  comment?: string;
  createdAt: string;
}

export interface UserReputationDto {
  userId: string;
  averageRating: number;
  totalReviews: number;
  completedSwaps: number;
  hasMinimumReviews: boolean;
}

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
