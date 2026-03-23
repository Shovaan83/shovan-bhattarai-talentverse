import axiosInstance from "../axios";
import type { BadgeDto, ServiceResponse } from "../types/badges";

export const badgesApi = {
  /**
   * Get all badges (with earned status for current user)
   */
  getAllBadges: async (): Promise<BadgeDto[]> => {
    const response = await axiosInstance.get<ServiceResponse<BadgeDto[]>>(
      "/badges"
    );
    return response.data.data;
  },

  /**
   * Get only the badges the current user has earned
   */
  getMyBadges: async (): Promise<BadgeDto[]> => {
    const response = await axiosInstance.get<ServiceResponse<BadgeDto[]>>(
      "/badges/mine"
    );
    return response.data.data;
  },

  /**
   * Get badges earned by a specific user
   */
  getUserBadges: async (userId: string): Promise<BadgeDto[]> => {
    const response = await axiosInstance.get<ServiceResponse<BadgeDto[]>>(
      `/badges/user/${userId}`
    );
    return response.data.data;
  },
};
