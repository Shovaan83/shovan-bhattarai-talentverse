export interface BadgeDto {
  badgeId: number;
  name: string;
  description: string;
  iconKey: string;
  tier: string;
  category: string;
  creditReward: number;
  earnedAt?: string | null;
  isEarned: boolean;
}

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
