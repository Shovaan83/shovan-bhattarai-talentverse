import { useQuery } from "@tanstack/react-query";
import { badgesApi } from "../api/badges";

export const BADGES_QUERY_KEY = ["badges"] as const;

/**
 * Get all badges with earned status for the current user
 */
export function useAllBadges() {
  return useQuery({
    queryKey: [...BADGES_QUERY_KEY, "all"],
    queryFn: badgesApi.getAllBadges,
  });
}

/**
 * Get only the badges the current user has earned
 */
export function useMyBadges() {
  return useQuery({
    queryKey: [...BADGES_QUERY_KEY, "mine"],
    queryFn: badgesApi.getMyBadges,
  });
}

/**
 * Get badges earned by a specific user (for public profile view)
 */
export function useUserBadges(userId: string) {
  return useQuery({
    queryKey: [...BADGES_QUERY_KEY, "user", userId],
    queryFn: () => badgesApi.getUserBadges(userId),
    enabled: !!userId,
  });
}
